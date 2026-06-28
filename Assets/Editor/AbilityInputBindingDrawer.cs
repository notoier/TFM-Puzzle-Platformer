using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AbilityManager.AbilityInputBinding))]
public class AbilityInputBindingDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty ability = property.FindPropertyRelative("ability");
        SerializedProperty key = property.FindPropertyRelative("key");
        SerializedProperty useGamepad = property.FindPropertyRelative("useGamepad");
        SerializedProperty gamepadButton = property.FindPropertyRelative("gamepadButton");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect line = new Rect(position.x, position.y, position.width, lineHeight);

        EditorGUI.PropertyField(line, ability);
        line.y += lineHeight + Spacing;

        EditorGUI.PropertyField(line, key);
        line.y += lineHeight + Spacing;

        EditorGUI.PropertyField(line, useGamepad);

        if (useGamepad.boolValue)
        {
            line.y += lineHeight + Spacing;
            EditorGUI.PropertyField(line, gamepadButton);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        SerializedProperty useGamepad = property.FindPropertyRelative("useGamepad");
        int visibleLines = useGamepad.boolValue ? 4 : 3;

        return visibleLines * EditorGUIUtility.singleLineHeight
               + (visibleLines - 1) * Spacing;
    }
}
