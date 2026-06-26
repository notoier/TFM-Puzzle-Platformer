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

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, height),
                iterator,
                true
            );

            if (EditorGUI.EndChangeCheck())
                ClearUnusedFields(property, iterator.name);

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
    /// Removes stale serialized values when a dropdown makes related fields irrelevant.
    /// </summary>
    private void ClearUnusedFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        switch (nodeProperty.managedReferenceValue)
        {
            case MovementNode:
                ClearUnusedMovementFields(nodeProperty, changedPropertyName);
                break;
            case TargetNode:
                ClearUnusedTargetFields(nodeProperty, changedPropertyName);
                break;
            case DeathNode:
                ClearUnusedDeathFields(nodeProperty, changedPropertyName);
                break;
            case ScaleNode:
                ClearUnusedScaleFields(nodeProperty, changedPropertyName);
                break;
            case SpawnNode:
                ClearUnusedSpawnFields(nodeProperty, changedPropertyName);
                break;
            case VisibilityNode:
                ClearUnusedVisibilityFields(nodeProperty, changedPropertyName);
                break;
            case WeightNode:
                ClearUnusedWeightFields(nodeProperty, changedPropertyName);
                break;
            case ReadWeightNode:
                ClearUnusedReadWeightFields(nodeProperty, changedPropertyName);
                break;
            case CancelNode:
                ClearUnusedCancelFields(nodeProperty, changedPropertyName);
                break;
            case ParameterNode:
                ClearUnusedParameterFields(nodeProperty, changedPropertyName);
                break;
        }
    }

    private void ClearUnusedMovementFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "actorSource")
            ClearTargetSourceFields(nodeProperty, GetMovementActorSource(nodeProperty), "actorTag", "actorName", "actorKey", "actorSelectionMode");

        if (changedPropertyName != "movementMode"
            && changedPropertyName != "directionSource"
            && changedPropertyName != "positionSource")
            return;

        SerializedProperty movementMode = nodeProperty.FindPropertyRelative("movementMode");
        MovementMode selectedMode = movementMode != null
            ? (MovementMode)movementMode.enumValueIndex
            : MovementMode.Distance;

        if (selectedMode == MovementMode.Position)
        {
            ClearRelativeProperty(nodeProperty, "distance");
            ClearRelativeProperty(nodeProperty, "directionSource");
            MovementPositionSource positionSource = GetMovementPositionSource(nodeProperty);
            if (positionSource != MovementPositionSource.LocalDirection)
                ClearRelativeProperty(nodeProperty, "localDirection");
            if (positionSource != MovementPositionSource.TargetNode)
                ClearRelativeProperty(nodeProperty, "targetNode");
            if (positionSource != MovementPositionSource.ContextVector)
                ClearRelativeProperty(nodeProperty, "positionKey");
            return;
        }

        ClearRelativeProperty(nodeProperty, "positionSource");
        ClearRelativeProperty(nodeProperty, "positionKey");

        SerializedProperty directionSource = nodeProperty.FindPropertyRelative("directionSource");
        MovementDirectionSource selectedDirection = directionSource != null
            ? (MovementDirectionSource)directionSource.enumValueIndex
            : MovementDirectionSource.LocalDirection;

        if (selectedDirection != MovementDirectionSource.LocalDirection)
            ClearRelativeProperty(nodeProperty, "localDirection");
        if (selectedDirection != MovementDirectionSource.TargetNode)
            ClearRelativeProperty(nodeProperty, "targetNode");
    }

    private void ClearUnusedTargetFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "targetSource")
            ClearTargetSourceFields(nodeProperty, GetTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");
    }

    private void ClearUnusedDeathFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "targetSource")
            ClearTargetSourceFields(nodeProperty, GetDeathTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");

        if (changedPropertyName == "affectAllTargets")
        {
            SerializedProperty affectAllTargets = nodeProperty.FindPropertyRelative("affectAllTargets");
            if (affectAllTargets != null && affectAllTargets.boolValue)
                ClearRelativeProperty(nodeProperty, "targetSelectionMode");
        }
    }

    private void ClearUnusedScaleFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "targetSource")
            ClearTargetSourceFields(nodeProperty, GetScaleTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");

        if (changedPropertyName != "valueSource")
            return;

        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        ScaleValueSource selectedSource = valueSource != null
            ? (ScaleValueSource)valueSource.enumValueIndex
            : ScaleValueSource.LocalValue;

        if (selectedSource == ScaleValueSource.LocalValue)
            ClearRelativeProperty(nodeProperty, "scaleParameter");
        else
            ClearRelativeProperty(nodeProperty, "scale");
    }

    private void ClearUnusedSpawnFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName != "positionSource")
            return;

        SerializedProperty positionSource = nodeProperty.FindPropertyRelative("positionSource");
        SpawnPositionSource selectedSource = positionSource != null
            ? (SpawnPositionSource)positionSource.enumValueIndex
            : SpawnPositionSource.LocalPosition;

        if (selectedSource == SpawnPositionSource.ActorPosition)
            ClearRelativeProperty(nodeProperty, "position");
    }

    private void ClearUnusedVisibilityFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName != "valueSource")
            return;

        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        VisibilityValueSource selectedSource = valueSource != null
            ? (VisibilityValueSource)valueSource.enumValueIndex
            : VisibilityValueSource.LocalValue;

        if (selectedSource == VisibilityValueSource.LocalValue)
            ClearRelativeProperty(nodeProperty, "visibilityParameter");
        else
            ClearRelativeProperty(nodeProperty, "isVisible");
    }

    private void ClearUnusedWeightFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "targetSource")
            ClearTargetSourceFields(nodeProperty, GetWeightTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");

        if (changedPropertyName != "valueSource")
            return;

        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        WeightValueSource selectedSource = valueSource != null
            ? (WeightValueSource)valueSource.enumValueIndex
            : WeightValueSource.ParameterNode;

        if (selectedSource == WeightValueSource.LocalValue)
            ClearRelativeProperty(nodeProperty, "weightParameter");
        else
            ClearRelativeProperty(nodeProperty, "weightValue");
    }

    private void ClearUnusedReadWeightFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "targetSource")
            ClearTargetSourceFields(nodeProperty, GetReadWeightTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");
    }

    private void ClearUnusedCancelFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName == "cancelMode")
        {
            SerializedProperty cancelMode = nodeProperty.FindPropertyRelative("cancelMode");
            CancelMode selectedMode = cancelMode != null
                ? (CancelMode)cancelMode.enumValueIndex
                : CancelMode.Always;

            if (selectedMode == CancelMode.Always)
            {
                ClearRelativeProperty(nodeProperty, "negateCondition");
                ClearRelativeProperty(nodeProperty, "targetSource");
                ClearRelativeProperty(nodeProperty, "targetSelectionMode");
                ClearRelativeProperty(nodeProperty, "targetTag");
                ClearRelativeProperty(nodeProperty, "targetName");
                ClearRelativeProperty(nodeProperty, "contextTargetKey");
                ClearRelativeProperty(nodeProperty, "contextVariableType");
                ClearRelativeProperty(nodeProperty, "contextVariableKey");
                ClearRelativeProperty(nodeProperty, "distanceComparison");
                ClearRelativeProperty(nodeProperty, "distance");
                return;
            }

            if (selectedMode == CancelMode.IfTargetExists)
            {
                ClearRelativeProperty(nodeProperty, "contextVariableType");
                ClearRelativeProperty(nodeProperty, "contextVariableKey");
                ClearRelativeProperty(nodeProperty, "targetSelectionMode");
                ClearRelativeProperty(nodeProperty, "distanceComparison");
                ClearRelativeProperty(nodeProperty, "distance");
                return;
            }

            if (selectedMode == CancelMode.IfContextVariableExists)
            {
                ClearRelativeProperty(nodeProperty, "targetSource");
                ClearRelativeProperty(nodeProperty, "targetSelectionMode");
                ClearRelativeProperty(nodeProperty, "targetTag");
                ClearRelativeProperty(nodeProperty, "targetName");
                ClearRelativeProperty(nodeProperty, "contextTargetKey");
                ClearRelativeProperty(nodeProperty, "distanceComparison");
                ClearRelativeProperty(nodeProperty, "distance");
                return;
            }

            if (selectedMode == CancelMode.IfTargetDistance)
            {
                ClearRelativeProperty(nodeProperty, "contextVariableType");
                ClearRelativeProperty(nodeProperty, "contextVariableKey");
                return;
            }
        }

        if (changedPropertyName == "targetSource"
            && (GetCancelMode(nodeProperty) == CancelMode.IfTargetExists || GetCancelMode(nodeProperty) == CancelMode.IfTargetDistance))
        {
            ClearTargetSourceFields(nodeProperty, GetCancelTargetSource(nodeProperty), "targetTag", "targetName", "contextTargetKey", "targetSelectionMode");
        }
    }

    private void ClearUnusedParameterFields(SerializedProperty nodeProperty, string changedPropertyName)
    {
        if (changedPropertyName != "parameterType")
            return;

        SerializedProperty parameterType = nodeProperty.FindPropertyRelative("parameterType");
        if (parameterType == null)
            return;

        string activeValueProperty = GetParameterValuePropertyName((ParameterType)parameterType.enumValueIndex);
        string[] valueProperties = { "floatValue", "intValue", "boolValue", "vector3Value", "gameObjectValue" };
        foreach (string valueProperty in valueProperties)
        {
            if (valueProperty != activeValueProperty)
                ClearRelativeProperty(nodeProperty, valueProperty);
        }
    }

    private void ClearTargetSourceFields(SerializedProperty nodeProperty, TargetSource targetSource, string tagProperty, string nameProperty, string contextKeyProperty, string selectionModeProperty)
    {
        if (targetSource != TargetSource.Tag)
            ClearRelativeProperty(nodeProperty, tagProperty);

        if (targetSource != TargetSource.Name)
            ClearRelativeProperty(nodeProperty, nameProperty);

        if (targetSource != TargetSource.ContextTarget)
            ClearRelativeProperty(nodeProperty, contextKeyProperty);

        if (!string.IsNullOrEmpty(selectionModeProperty) && !UsesTargetSelection(targetSource))
            ClearRelativeProperty(nodeProperty, selectionModeProperty);
    }

    private void ClearRelativeProperty(SerializedProperty nodeProperty, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return;

        SerializedProperty property = nodeProperty.FindPropertyRelative(propertyName);
        if (property == null)
            return;

        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Enum:
                property.intValue = 0;
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = false;
                break;
            case SerializedPropertyType.Float:
                property.floatValue = 0f;
                break;
            case SerializedPropertyType.String:
                property.stringValue = string.Empty;
                break;
            case SerializedPropertyType.Vector3:
                property.vector3Value = Vector3.zero;
                break;
            case SerializedPropertyType.ObjectReference:
                property.objectReferenceValue = null;
                break;
            case SerializedPropertyType.ManagedReference:
                property.managedReferenceValue = null;
                break;
        }
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

        if (nodeProperty.managedReferenceValue is ScaleNode)
            return ShouldDrawScaleProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is SpawnNode)
            return ShouldDrawSpawnProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is TargetNode)
            return ShouldDrawTargetProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is DeathNode)
            return ShouldDrawDeathProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is VisibilityNode)
            return ShouldDrawVisibilityProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is WeightNode)
            return ShouldDrawWeightProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is ReadWeightNode)
            return ShouldDrawReadWeightProperty(nodeProperty, childProperty);

        if (nodeProperty.managedReferenceValue is CancelNode)
            return ShouldDrawCancelProperty(nodeProperty, childProperty);

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
    /// Shows only the scale input field required by the selected value source.
    /// </summary>
    private bool ShouldDrawScaleProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "targetSelectionMode"
            && childName != "targetTag"
            && childName != "targetName"
            && childName != "contextTargetKey"
            && childName != "scale"
            && childName != "scaleParameter")
            return true;

        TargetSource targetSource = GetScaleTargetSource(nodeProperty);
        if (childName == "targetSelectionMode")
            return UsesTargetSelection(targetSource);
        if (childName == "targetTag")
            return targetSource == TargetSource.Tag;
        if (childName == "targetName")
            return targetSource == TargetSource.Name;
        if (childName == "contextTargetKey")
            return targetSource == TargetSource.ContextTarget;

        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        ScaleValueSource selectedSource = valueSource != null
            ? (ScaleValueSource)valueSource.enumValueIndex
            : ScaleValueSource.LocalValue;

        return childName switch
        {
            "scale" => selectedSource == ScaleValueSource.LocalValue,
            "scaleParameter" => selectedSource == ScaleValueSource.ParameterNode,
            _ => true
        };
    }

    /// <summary>
    /// Shows only the spawn position field required by the selected position source.
    /// </summary>
    private bool ShouldDrawSpawnProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "position")
            return true;

        SerializedProperty positionSource = nodeProperty.FindPropertyRelative("positionSource");
        SpawnPositionSource selectedSource = positionSource != null
            ? (SpawnPositionSource)positionSource.enumValueIndex
            : SpawnPositionSource.LocalPosition;

        return selectedSource == SpawnPositionSource.LocalPosition;
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
    /// Shows only the visibility input field required by the selected value source.
    /// </summary>
    private bool ShouldDrawVisibilityProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "isVisible" && childName != "visibilityParameter")
            return true;

        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        VisibilityValueSource selectedSource = valueSource != null
            ? (VisibilityValueSource)valueSource.enumValueIndex
            : VisibilityValueSource.LocalValue;

        return childName switch
        {
            "isVisible" => selectedSource == VisibilityValueSource.LocalValue,
            "visibilityParameter" => selectedSource == VisibilityValueSource.ParameterNode,
            _ => true
        };
    }

    /// <summary>
    /// Shows only the WeightNode target fields required by the selected target source.
    /// </summary>
    private bool ShouldDrawWeightProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "targetSelectionMode"
            && childName != "targetTag"
            && childName != "targetName"
            && childName != "contextTargetKey"
            && childName != "weightValue"
            && childName != "weightParameter")
            return true;

        TargetSource targetSource = GetWeightTargetSource(nodeProperty);
        SerializedProperty valueSource = nodeProperty.FindPropertyRelative("valueSource");
        WeightValueSource selectedSource = valueSource != null
            ? (WeightValueSource)valueSource.enumValueIndex
            : WeightValueSource.ParameterNode;

        return childName switch
        {
            "targetSelectionMode" => UsesTargetSelection(targetSource),
            "targetTag" => targetSource == TargetSource.Tag,
            "targetName" => targetSource == TargetSource.Name,
            "contextTargetKey" => targetSource == TargetSource.ContextTarget,
            "weightValue" => selectedSource == WeightValueSource.LocalValue,
            "weightParameter" => selectedSource == WeightValueSource.ParameterNode,
            _ => true
        };
    }

    /// <summary>
    /// Shows only the ReadWeightNode target fields required by the selected target source.
    /// </summary>
    private bool ShouldDrawReadWeightProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "targetSelectionMode"
            && childName != "targetTag"
            && childName != "targetName"
            && childName != "contextTargetKey")
            return true;

        TargetSource targetSource = GetReadWeightTargetSource(nodeProperty);
        return childName switch
        {
            "targetSelectionMode" => UsesTargetSelection(targetSource),
            "targetTag" => targetSource == TargetSource.Tag,
            "targetName" => targetSource == TargetSource.Name,
            "contextTargetKey" => targetSource == TargetSource.ContextTarget,
            _ => true
        };
    }

    /// <summary>
    /// Shows only the CancelNode fields required by the selected cancel mode and target source.
    /// </summary>
    private bool ShouldDrawCancelProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "negateCondition"
            && childName != "targetSource"
            && childName != "targetSelectionMode"
            && childName != "targetTag"
            && childName != "targetName"
            && childName != "contextTargetKey"
            && childName != "contextVariableType"
            && childName != "contextVariableKey"
            && childName != "distanceComparison"
            && childName != "distance")
            return true;

        CancelMode selectedMode = GetCancelMode(nodeProperty);

        if (selectedMode == CancelMode.Always)
            return false;

        if (childName == "negateCondition")
            return true;

        bool usesTarget = selectedMode == CancelMode.IfTargetExists || selectedMode == CancelMode.IfTargetDistance;
        bool usesDistance = selectedMode == CancelMode.IfTargetDistance;
        bool usesContextVariable = selectedMode == CancelMode.IfContextVariableExists;

        TargetSource targetSource = GetCancelTargetSource(nodeProperty);
        return childName switch
        {
            "targetSource" => usesTarget,
            "targetSelectionMode" => usesDistance && UsesTargetSelection(targetSource),
            "targetTag" => usesTarget && targetSource == TargetSource.Tag,
            "targetName" => usesTarget && targetSource == TargetSource.Name,
            "contextTargetKey" => usesTarget && targetSource == TargetSource.ContextTarget,
            "contextVariableType" => usesContextVariable,
            "contextVariableKey" => usesContextVariable,
            "distanceComparison" => usesDistance,
            "distance" => usesDistance,
            _ => true
        };
    }

    /// <summary>
    /// Shows only the DeathNode fields required by the selected target source.
    /// </summary>
    private bool ShouldDrawDeathProperty(SerializedProperty nodeProperty, SerializedProperty childProperty)
    {
        string childName = childProperty.name;
        if (childName != "targetSelectionMode"
            && childName != "affectAllTargets"
            && childName != "contextTargetKey"
            && childName != "targetTag"
            && childName != "targetName")
            return true;

        TargetSource targetSource = GetDeathTargetSource(nodeProperty);
        bool usesSelection = UsesTargetSelection(targetSource);
        bool affectAllTargets = GetBool(nodeProperty, "affectAllTargets");

        return childName switch
        {
            "targetSelectionMode" => usesSelection && !affectAllTargets,
            "affectAllTargets" => usesSelection,
            "contextTargetKey" => targetSource == TargetSource.ContextTarget,
            "targetTag" => targetSource == TargetSource.Tag,
            "targetName" => targetSource == TargetSource.Name,
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
    /// Reads the selected target source used by CancelNode.
    /// </summary>
    private TargetSource GetCancelTargetSource(SerializedProperty nodeProperty)
    {
        SerializedProperty targetSource = nodeProperty.FindPropertyRelative("targetSource");
        return targetSource != null
            ? (TargetSource)targetSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Reads the selected target source used by DeathNode.
    /// </summary>
    private TargetSource GetDeathTargetSource(SerializedProperty nodeProperty)
    {
        SerializedProperty targetSource = nodeProperty.FindPropertyRelative("targetSource");
        return targetSource != null
            ? (TargetSource)targetSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Reads the selected condition used by CancelNode.
    /// </summary>
    private CancelMode GetCancelMode(SerializedProperty nodeProperty)
    {
        SerializedProperty cancelMode = nodeProperty.FindPropertyRelative("cancelMode");
        return cancelMode != null
            ? (CancelMode)cancelMode.enumValueIndex
            : CancelMode.Always;
    }

    /// <summary>
    /// Reads the selected target source used by WeightNode.
    /// </summary>
    private TargetSource GetWeightTargetSource(SerializedProperty nodeProperty)
    {
        SerializedProperty targetSource = nodeProperty.FindPropertyRelative("targetSource");
        return targetSource != null
            ? (TargetSource)targetSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Reads the selected target source used by ScaleNode.
    /// </summary>
    private TargetSource GetScaleTargetSource(SerializedProperty nodeProperty)
    {
        SerializedProperty targetSource = nodeProperty.FindPropertyRelative("targetSource");
        return targetSource != null
            ? (TargetSource)targetSource.enumValueIndex
            : TargetSource.Self;
    }

    /// <summary>
    /// Reads the selected target source used by ReadWeightNode.
    /// </summary>
    private TargetSource GetReadWeightTargetSource(SerializedProperty nodeProperty)
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

    private bool GetBool(SerializedProperty nodeProperty, string propertyName)
    {
        SerializedProperty property = nodeProperty.FindPropertyRelative(propertyName);
        return property != null && property.boolValue;
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
