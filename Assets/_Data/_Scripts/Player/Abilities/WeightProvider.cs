using UnityEngine;

public class WeightProvider : MonoBehaviour, IProvidesWeight
{
    [SerializeField]
    private float weight = 1f;

    public float Weight
    {
        get => weight;
        set => weight = value;
    }

    public void AddWeight(float mass)
    {
        weight += mass;
    }
}