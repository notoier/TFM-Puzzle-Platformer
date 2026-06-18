using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class AbilityManager : MonoBehaviour
{
    [Serializable]
    public class AbilityInputBinding
    {
        public Ability ability;
        public Key key = Key.None;
    }

    [Header("Abilities")]
    public List<AbilityInputBinding> Abilities = new();

    private readonly HashSet<Ability> activeAbilities = new();

    private void Start()
    {
        EndAllAbilities();
    }

    private void Update()
    {
        if (Keyboard.current == null || Abilities == null)
            return;

        for (int i = 0; i < Abilities.Count; i++)
        {
            AbilityInputBinding binding = Abilities[i];
            if (binding == null || binding.ability == null || binding.key == Key.None)
                continue;

            if (Keyboard.current[binding.key].wasPressedThisFrame)
                ActivateAbility(i);
        }
    }

    public void ActivateAbility(int index)
    {
        if (Abilities == null) return;
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index]?.ability;
        if (ability == null) return;

        AbilityValidationResult validation = ability.Validate();
        if (validation.BlocksUse)
        {
            Debug.LogWarning($"Ability '{ability.name}' cannot activate: {validation.Message}");
            return;
        }

        if (activeAbilities.Contains(ability)) return;

        activeAbilities.Add(ability);
        AbilityContext context = ability.Activate(gameObject, this, _ => FinishAbility(ability));

        if (!context.keepActive && !context.finished)
            FinishAbility(ability);
    }

    public void EndAbility(int index)
    {
        if (Abilities == null) return;
        if (index < 0 || index >= Abilities.Count) return;

        Ability ability = Abilities[index]?.ability;
        if (ability == null) return;

        FinishAbility(ability);
    }

    protected void EndAllAbilities()
    {
        if (Abilities == null) return;

        foreach (var binding in Abilities)
        {
            if (binding?.ability != null)
            {
                FinishAbility(binding.ability);
            }
        }
    }

    private void FinishAbility(Ability ability)
    {
        activeAbilities.Remove(ability);
        ability.End(gameObject);
    }
}
