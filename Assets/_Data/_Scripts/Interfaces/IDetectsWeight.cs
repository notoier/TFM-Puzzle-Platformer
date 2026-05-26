using UnityEngine;
public interface IDetectsWeight
{
    float CurrentWeight { get; set; }

    void RegisterWeight(IProvidesWeight weightProvider);
    void RegisterWeight(float weight);
    void UnregisterWeight(IProvidesWeight weightProvider);
    void UnregisterWeight(float weight);
    bool HasEnoughWeight();
}