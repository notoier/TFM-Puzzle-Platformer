using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityManager : StaticInstance<AbilityManager>
{
    [Header("Abilities")]
    public List<Ability> Abilities;

    private void Start()
    {
        EndAllAbilities();
    }

    public void ActivateAbility(int index)
    {
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index];
        if (ability == null) return;

        if (!ability.canActivate) return;

        ability.Activate(gameObject);
    }

    public void EndAbility(int index)
    {
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index];
        if (ability == null) return;

        ability.End(gameObject);
    }

    protected void EndAllAbilities()
    {
        foreach (var ability in Abilities)
        {
            if (ability != null)
            {
                ability.End(gameObject);
            }
        }
    }
}