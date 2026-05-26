using UnityEngine;

public interface IProvidesWeight
{ 
    float Weight { get; set; }

    public void AddWeight(float  mass) { Weight += mass; }
    
}
