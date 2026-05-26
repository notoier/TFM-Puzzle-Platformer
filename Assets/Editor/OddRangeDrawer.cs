#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OddRangeAttribute))]
public class OddRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        OddRangeAttribute oddRange = (OddRangeAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "OddRange only works with int.");
            return;
        }

        int oddMin = MakeOdd(oddRange.Min);
        int oddMax = MakeOdd(oddRange.Max);

        int currentValue = Mathf.Clamp(property.intValue, oddMin, oddMax);

        if (currentValue % 2 == 0)
            currentValue += 1;

        int maxStep = (oddMax - oddMin) / 2;
        int step = Mathf.Clamp((currentValue - oddMin) / 2, 0, maxStep);

        int newStep = EditorGUI.IntSlider(position, label, step, 0, maxStep);

        property.intValue = oddMin + newStep * 2;
    }

    private int MakeOdd(int value)
    {
        return value % 2 == 0 ? value + 1 : value;
    }
}
#endif