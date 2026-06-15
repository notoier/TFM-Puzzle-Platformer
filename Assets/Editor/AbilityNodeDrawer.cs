using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AbilityNode), true)]
public class AbilityNodeDrawer : PropertyDrawer
{
    /// <summary>
    /// Draws a managed-reference ability node in the Inspector, including its validation state.
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        AbilityValidationResult validation = GetValidation(property);

        if (property.managedReferenceValue == null)
        {
            DrawEmptyState(position, property);
            EditorGUI.EndProperty();
            return;
        }

        DrawNodeState(position, property, validation);

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// Draws the placeholder UI for an empty node slot and exposes the create-node menu.
    /// </summary>
    private void DrawEmptyState(Rect position, SerializedProperty property)
    {
        Color prevColor = GUI.color;
        GUI.color = new Color32(255, 196, 0, 255);

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            property.displayName,
            true
        );

        GUI.color = prevColor;

        if (property.isExpanded)
        {
            Rect buttonRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            if (GUI.Button(buttonRect, "Create Node"))
            {
                ShowNodeMenu(property);
            }
        }
    }

    /// <summary>
    /// Draws an existing node, its visible serialized fields, and any validation message.
    /// </summary>
    private void DrawNodeState(Rect position, SerializedProperty property, AbilityValidationResult validation)
    {
        string name = property.managedReferenceValue
            .GetType()
            .Name
            .Replace("Node", "");

        Color prevColor = GUI.color;

        GUI.color = GetStateColor(validation.State);

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            $"{name} - {GetStateLabel(validation.State)}",
            true
        );

        GUI.color = prevColor;

        if (!property.isExpanded)
            return;

        EditorGUI.indentLevel++;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        float y = position.y + EditorGUIUtility.singleLineHeight;

        iterator.NextVisible(true);

        while (!SerializedProperty.EqualContents(iterator, end))
        {
            if (!ShouldDrawProperty(property, iterator))
            {
                iterator.NextVisible(false);
                continue;
            }

            float height = EditorGUI.GetPropertyHeight(iterator, true);

            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, height),
                iterator,
                true
            );

            y += height + 2;
            iterator.NextVisible(false);
        }

        if (!string.IsNullOrEmpty(validation.Message) && validation.State != AbilityValidationState.Complete)
        {
            Rect helpRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight * 2);
            EditorGUI.HelpBox(helpRect, validation.Message, GetMessageType(validation.State));
        }

        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// Calculates the Inspector height needed for the node, including nested fields and help boxes.
    /// </summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight * 2;

        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        iterator.NextVisible(true);

        while (!SerializedProperty.EqualContents(iterator, end))
        {
            if (!ShouldDrawProperty(property, iterator))
            {
                iterator.NextVisible(false);
                continue;
            }

            height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            iterator.NextVisible(false);
        }

        AbilityValidationResult validation = GetValidation(property);
        if (!string.IsNullOrEmpty(validation.Message) && validation.State != AbilityValidationState.Complete)
            height += EditorGUIUtility.singleLineHeight * 2 + 2;

        return height;
    }

    /// <summary>
    /// Shows a context menu with only the node types assignable to the selected managed-reference field.
    /// </summary>
    private void ShowNodeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        Type acceptedType = GetManagedReferenceFieldType(property) ?? typeof(AbilityNode);

        var nodeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(AbilityNode).IsAssignableFrom(t))
            .Where(t => acceptedType.IsAssignableFrom(t))
            .Where(t => !t.IsAbstract && !t.IsInterface);

        bool hasItems = false;
        foreach (var type in nodeTypes)
        {
            hasItems = true;
            string name = type.Name.Replace("Node", "");

            menu.AddItem(new GUIContent(name), false, () =>
            {
                object instance = Activator.CreateInstance(type);
                ApplyNodeDefaults(property, instance);
                property.managedReferenceValue = instance;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        if (!hasItems)
            menu.AddDisabledItem(new GUIContent($"No {acceptedType.Name} types available"));
       
        menu.ShowAsContext();
    }

    /// <summary>
    /// Applies context-aware defaults when a node is created inside another node's field.
    /// </summary>
    private void ApplyNodeDefaults(SerializedProperty property, object instance)
    {
        if (instance is ParameterNode parameterNode && TryGetDefaultParameterType(property, out ParameterType parameterType))
            parameterNode.parameterType = parameterType;
    }

    /// <summary>
    /// Infers the expected ParameterNode value type from the managed-reference field name.
    /// </summary>
    private bool TryGetDefaultParameterType(SerializedProperty property, out ParameterType parameterType)
    {
        parameterType = ParameterType.Float;

        return property.name switch
        {
            "localDirection" => SetParameterType(ParameterType.Vector3, out parameterType),
            "scaleParameter" => SetParameterType(ParameterType.Float, out parameterType),
            "weightParameter" => SetParameterType(ParameterType.Int, out parameterType),
            "visibilityParameter" => SetParameterType(ParameterType.Bool, out parameterType),
            _ => false
        };
    }

    /// <summary>
    /// Assigns a parameter type while keeping switch expressions compact.
    /// </summary>
    private bool SetParameterType(ParameterType input, out ParameterType output)
    {
        output = input;
        return true;
    }

    /// <summary>
    /// Resolves the declared field type of a managed reference so the create menu can be filtered.
    /// </summary>
    private Type GetManagedReferenceFieldType(SerializedProperty property)
    {
        string fieldTypeName = property.managedReferenceFieldTypename;
        if (string.IsNullOrWhiteSpace(fieldTypeName))
            return typeof(AbilityNode);

        string[] typeParts = fieldTypeName.Split(' ');
        if (typeParts.Length < 2)
            return typeof(AbilityNode);

        string assemblyName = typeParts[0];
        string typeName = typeParts[1];

        Type resolvedType = Type.GetType($"{typeName}, {assemblyName}");
        if (resolvedType != null)
            return resolvedType;

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName)
            ?? typeof(AbilityNode);
    }

    /// <summary>
    /// Checks recursively whether a node and its nested references/object fields are populated.
    /// </summary>
    private bool IsNodeFullyFilled(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
            return false;

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = iterator.GetEndProperty();

        iterator.NextVisible(true);

        while (!SerializedProperty.EqualContents(iterator, end))
        {
            if (iterator.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (iterator.managedReferenceValue == null)
                    return false;

                if (!IsNodeFullyFilled(iterator))
                    return false;
            }

            if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                iterator.objectReferenceValue == null)
                return false;

            iterator.NextVisible(false);
        }

        return true;
    }

    /// <summary>
    /// Decides whether a child property should be visible for the currently drawn node.
    /// </summary>
    private bool ShouldDrawProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        if (nodeProperty.managedReferenceValue is MovementNode)
            return ShouldDrawMovementProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is TargetNode)
            return ShouldDrawTargetProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is not ParameterNode)
            return true;

        string childName = childProperty.name;
        if (!IsParameterValueProperty(childName))
            return true;

        SerializedProperty parameterType = nodeProperty.FindPropertyRelative("parameterType");
        if (parameterType == null)
            return true;

        ParameterType selectedType = (ParameterType)parameterType.enumValueIndex;
        return childName == GetParameterValuePropertyName(selectedType);
    }

    /// <summary>
    /// Shows only the movement input field that matches the selected movement direction source.
    /// </summary>
    private bool ShouldDrawMovementProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "distance"
            && childName != "actorSelectionMode"
            && childName != "actorTag"
            && childName != "actorName"
            && childName != "actorKey"
            && childName != "directionSource"
            && childName != "positionSource"
            && childName != "positionKey"
            && childName != "localDirection"
            && childName != "targetNode")
            return true;

        TargetSource actorSource = GetMovementActorSource(nodeProperty);

        if (childName == "actorSelectionMode")
            return UsesTargetSelection(actorSource);

        if (childName == "actorTag")
            return actorSource == TargetSource.Tag;

        if (childName == "actorName")
            return actorSource == TargetSource.Name;

        if (childName == "actorKey")
            return actorSource == TargetSource.ContextTarget;

        SerializedProperty movementMode = nodeProperty.FindPropertyRelative("movementMode");
        MovementMode selectedMode = movementMode != null
            ? (MovementMode)movementMode.enumValueIndex
            : MovementMode.Distance;

        if (childName == "targetNode" && selectedMode == MovementMode.Position)
            return GetMovementPositionSource(nodeProperty) == MovementPositionSource.TargetNode;

        if (childName == "localDirection" && selectedMode == MovementMode.Position)
            return GetMovementPositionSource(nodeProperty) == MovementPositionSource.LocalDirection;

        if (childName == "positionKey" && selectedMode == MovementMode.Position)
            return GetMovementPositionSource(nodeProperty) == MovementPositionSource.ContextVector;

        if (childName == "positionSource")
            return selectedMode == MovementMode.Position;

        if (selectedMode == MovementMode.Position)
            return false;

        if (childName == "distance" || childName == "directionSource")
            return true;

        SerializedProperty directionSource = nodeProperty.FindPropertyRelative("directionSource");
        if (directionSource == null)
            return true;

        MovementDirectionSource selectedSource = (MovementDirectionSource)directionSource.enumValueIndex;
        return childName switch
        {
            "localDirection" => selectedSource == MovementDirectionSource.LocalDirection,
            "targetNode" => selectedSource == MovementDirectionSource.TargetNode,
            _ => true
        };
    }

    /// <summary>
    /// Reads which object MovementNode will move.
    /// </summary>
    private TargetSource GetMovementActorSource(SerializedProperty nodeProperty)
    {
        SerializedProperty actorSource = nodeProperty.FindPropertyRelative("actorSource");
        return actorSource != null
            ? (TargetSource)actorSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Reads the selected source used by MovementNode when it moves by position/vector.
    /// </summary>
    private MovementPositionSource GetMovementPositionSource(SerializedProperty nodeProperty)
    {
        SerializedProperty positionSource = nodeProperty.FindPropertyRelative("positionSource");
        return positionSource != null
            ? (MovementPositionSource)positionSource.enumValueIndex
            : MovementPositionSource.TargetNode;
    }

    /// <summary>
    /// Shows only the TargetNode fields required by the selected target source.
    /// </summary>
    private bool ShouldDrawTargetProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "targetSelectionMode"
            && childName != "contextTargetKey"
            && childName != "targetTag"
            && childName != "targetName")
            return true;

        TargetSource targetSource = GetTargetSource(nodeProperty);
        return childName switch
        {
            "targetSelectionMode" => UsesTargetSelection(targetSource),
            "contextTargetKey" => targetSource == TargetSource.ContextTarget,
            "targetTag" => targetSource == TargetSource.Tag,
            "targetName" => targetSource == TargetSource.Name,
            _ => true
        };
    }

    /// <summary>
    /// Reads the selected source used by TargetNode to gather candidates.
    /// </summary>
    private TargetSource GetTargetSource(SerializedProperty nodeProperty)
    {
        SerializedProperty targetSource = nodeProperty.FindPropertyRelative("targetSource");
        return targetSource != null
            ? (TargetSource)targetSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Returns whether a target source can produce multiple candidates and therefore needs selection.
    /// </summary>
    private bool UsesTargetSelection(TargetSource targetSource)
    {
        return targetSource == TargetSource.Tag || targetSource == TargetSource.Name;
    }

    /// <summary>
    /// Returns true when the property is one of ParameterNode's type-specific value fields.
    /// </summary>
    private bool IsParameterValueProperty(string propertyName)
    {
        return propertyName == "floatValue"
               || propertyName == "intValue"
               || propertyName == "boolValue"
               || propertyName == "vector3Value"
               || propertyName == "gameObjectValue";
    }

    /// <summary>
    /// Maps a ParameterType to the serialized field that stores that type's literal value.
    /// </summary>
    private string GetParameterValuePropertyName(ParameterType type)
    {
        return type switch
        {
            ParameterType.Float => "floatValue",
            ParameterType.Int => "intValue",
            ParameterType.Bool => "boolValue",
            ParameterType.Vector3 => "vector3Value",
            ParameterType.GameObject => "gameObjectValue",
            _ => "floatValue"
        };
    }

    /// <summary>
    /// Runs validation for the node, using root-node rules only for nodes in the ability's top-level list.
    /// </summary>
    private AbilityValidationResult GetValidation(SerializedProperty property)
    {
        if (property.managedReferenceValue == null)
            return AbilityValidationResult.Incomplete("Node is empty.");

        AbilityNode node = property.managedReferenceValue as AbilityNode;
        if (node == null)
            return AbilityValidationResult.Invalid("Managed reference is not an ability node.");

        AbilityValidationContext validationContext = GetValidationContext(property);
        return IsRootAbilityNode(property)
            ? node.ValidateAsRoot(validationContext)
            : node.Validate(validationContext);
    }

    /// <summary>
    /// Determines whether the property belongs directly to the ability's root nodes list.
    /// </summary>
    private bool IsRootAbilityNode(SerializedProperty property)
    {
        string path = property.propertyPath;
        if (!path.StartsWith("nodes.Array.data["))
            return false;

        int closingBracket = path.IndexOf(']');
        return closingBracket == path.Length - 1;
    }

    /// <summary>
    /// Builds validation context from the inspected ability asset's declared variables.
    /// </summary>
    private AbilityValidationContext GetValidationContext(SerializedProperty property)
    {
        Ability ability = property.serializedObject.targetObject as Ability;
        return ability != null ? new AbilityValidationContext(ability.variables) : null;
    }

    /// <summary>
    /// Chooses the foldout color used to represent a validation state.
    /// </summary>
    private Color GetStateColor(AbilityValidationState state)
    {
        return state switch
        {
            AbilityValidationState.Invalid => new Color32(255, 64, 64, 255),
            AbilityValidationState.Complete => new Color32(0, 220, 80, 255),
            AbilityValidationState.Ready => new Color32(64, 160, 255, 255),
            _ => new Color32(255, 196, 0, 255)
        };
    }

    /// <summary>
    /// Converts a validation state into the short status label shown beside the node name.
    /// </summary>
    private string GetStateLabel(AbilityValidationState state)
    {
        return state switch
        {
            AbilityValidationState.Invalid => "Invalid",
            AbilityValidationState.Complete => "Complete",
            AbilityValidationState.Ready => "Ready",
            _ => "Incomplete"
        };
    }

    /// <summary>
    /// Converts a validation state into the Unity HelpBox message style.
    /// </summary>
    private MessageType GetMessageType(AbilityValidationState state)
    {
        return state == AbilityValidationState.Invalid ? MessageType.Error : MessageType.Warning;
    }
}
