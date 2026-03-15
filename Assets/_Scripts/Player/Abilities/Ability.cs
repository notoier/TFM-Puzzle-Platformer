using System;
using Unity.VersionControl.Git;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public bool canActivate;

    public AudioClip abilitySound;

    public event Action<Ability, GameObject> OnAbilityActivated;

    public void Activate(GameObject abilityUser)
    {
        if (!canActivate) return;

        Execute(abilityUser);
        AudioManager.Instance.PlayEffect(abilitySound);
        canActivate = false;
        OnAbilityActivated?.Invoke(this, abilityUser);
    }

    protected abstract void Execute(GameObject abilityUser);

    public virtual void End(GameObject abilityUser)
    {
        canActivate = true;
    }
}
