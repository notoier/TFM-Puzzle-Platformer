using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityManager : StaticInstance<AbilityManager>
{
    [Header("Abilities")]
    [Tooltip("List of abilities the player can use.")]
    public List<Ability> Abilities;

    //private Dictionary<Ability, bool> abilityAvailability = 
    //    new Dictionary<Ability, bool>();
    private void Start()
    {
        EndAllAbilities();
    }

    public void ActivateAbility(int index)
    {
        if (index < 0 || index > Abilities.Count) return;
        
        Ability ability = Abilities[index];

        if(!ability.canActivate) return;
        else
        {

            ability.Activate(gameObject);

        }
    }

    public void EndAbility(int index) 
    {
        if (index < 0 || index > Abilities.Count) return;
        Abilities[index].End(gameObject);
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
