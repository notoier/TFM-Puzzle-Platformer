using UnityEngine;

public interface IProvidesWeight
{ 
    float Weight { get; set; }

    public virtual void AddWeight(float  mass) { Weight += mass; }
}
