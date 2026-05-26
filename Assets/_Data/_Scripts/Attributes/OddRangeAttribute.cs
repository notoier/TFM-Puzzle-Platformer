using UnityEngine;

public class OddRangeAttribute : PropertyAttribute
{
    public int Min { get; }
    public int Max { get; }

    public OddRangeAttribute(int min, int max)
    {
        Min = min;
        Max = max;
    }
}