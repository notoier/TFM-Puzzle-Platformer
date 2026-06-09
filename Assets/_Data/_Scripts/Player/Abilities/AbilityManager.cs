using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityManager : MonoBehaviour
{
    [Header("Abilities")]
    public List<Ability> Abilities;

    private readonly HashSet<Ability> activeAbilities = new();

    private void Start()
    {
        EndAllAbilities();
    }

    public void ActivateAbility(int index)
    {
        if (Abilities == null) return;
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index];
        if (ability == null) return;

        AbilityValidationResult validation = ability.Validate();
        if (validation.BlocksUse)
        {
            Debug.LogWarning($"Ability '{ability.name}' cannot activate: {validation.Message}");
            return;
        }

        if (activeAbilities.Contains(ability)) return;

        activeAbilities.Add(ability);
        AbilityContext context = ability.Activate(gameObject);

        if (!context.keepActive)
            FinishAbility(ability);
    }

    public void EndAbility(int index)
    {
        if (Abilities == null) return;
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index];
        if (ability == null) return;

        FinishAbility(ability);
    }

    protected void EndAllAbilities()
    {
        if (Abilities == null) return;

        foreach (var ability in Abilities)
        {
            if (ability != null)
            {
                FinishAbility(ability);
            }
        }
    }

    private void FinishAbility(Ability ability)
    {
        activeAbilities.Remove(ability);
        ability.End(gameObject);
    }
}