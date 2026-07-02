using UnityEngine;

public interface IProvidesWeight
{ 
    float Weight { get; set; }

    public virtual void AddWeight(float  mass, bool instant, bool fuse) { Weight += mass; }

    public virtual void AdaptWeight(bool instant, bool fuse){}
}
