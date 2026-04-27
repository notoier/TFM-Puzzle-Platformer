using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AbilityNode), true)]
public class AbilityNodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool isFilled = IsNodeFullyFilled(property);

        if (property.managedReferenceValue == null)
        {
            DrawEmptyState(position, property);
            EditorGUI.EndProperty();
            return;
        }

        DrawNodeState(position, property, isFilled);

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// Draws the UI when the node IS empty.
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
    /// Draws the UI when the node is NOT empty.
    /// </summary>
    private void DrawNodeState(Rect position, SerializedProperty property, bool isFilled)
    {
        string name = property.managedReferenceValue
            .GetType()
            .Name
            .Replace("Node", "");

        Color prevColor = GUI.color;

        GUI.color = isFilled
            ? new Color32(0, 255, 47, 255)   // green
            : new Color32(255, 196, 0, 255); // orange

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            name,
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
            float height = EditorGUI.GetPropertyHeight(iterator, true);

            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, height),
                iterator,
                true
            );

            y += height + 2;
            iterator.NextVisible(false);
        }

        EditorGUI.indentLevel--;
    }


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
            height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
            iterator.NextVisible(false);
        }

        return height;
    }


    private void ShowNodeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        var nodeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(AbilityNode).IsAssignableFrom(t))
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in nodeTypes)
        {
            string name = type.Name.Replace("Node", "");

            menu.AddItem(new GUIContent(name), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }
       
        menu.ShowAsContext();
    }

    /// <summary>
    /// Checks recursively if the nodes are filled. Useful data to handle color codes and allowing the user to complete an ability.
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
}