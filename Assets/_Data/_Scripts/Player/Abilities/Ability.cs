using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject
{
    public List<AbilityVariableDefinition> variables = new();

    // This list builds the logic for the ability via nodes.
    [SerializeReference]
    public List<AbilityNode> nodes = new();

    public AbilityValidationResult Validate()
    {
        AbilityValidationResult variableValidation = ValidateVariables();
        if (variableValidation.BlocksUse)
            return variableValidation;

        if (nodes == null || nodes.Count == 0)
            return AbilityValidationResult.Incomplete("Ability has no nodes.");

        AbilityValidationContext validationContext = new AbilityValidationContext(variables);

        foreach (var node in nodes)
        {
            if (node == null)
                return AbilityValidationResult.Incomplete("Ability contains an empty node.");

            AbilityValidationResult validation = node.ValidateAsRoot(validationContext);
            if (validation.BlocksUse)
                return validation;
        }

        return AbilityValidationResult.Ready("Ability is ready.");
    }

    // When this function is called, it passes the user to get their AbilityContext. This context is passed to every node if events are not cancelled.
    public AbilityContext Activate(GameObject actor, MonoBehaviour coroutineRunner = null, System.Action<AbilityContext> finishCallback = null)
    {
        AbilityContext context = new AbilityContext
        {
            actor = actor,
            coroutineRunner = coroutineRunner
        };
        context.SetFinishCallback(finishCallback);
        ExecuteNodes(context, 0);

        return context;
    }

    private void ExecuteNodes(AbilityContext context, int startIndex)
    {
        for (int i = startIndex; i < nodes.Count; i++)
        {
            AbilityNode node = nodes[i];
            if (node == null)
            {
                Debug.LogWarning($"Ability '{name}' contains an empty node. It was skipped.");
                continue;
            }

            if (context.cancelled)
                break;
            if (!context.success)
            {
                node.DefaultBehavior(context);
                continue;
            }
            node.Execute(context);

            if (context.keepActive)
            {
                int nextIndex = i + 1;
                context.SetResumeCallback(_ => ExecuteNodes(context, nextIndex));
                break;
            }
        }

        if (!context.keepActive && !context.finished)
            context.FinishAbility();
    }

    public void End(GameObject actor)
    {
    }

    private AbilityValidationResult ValidateVariables()
    {
        if (variables == null)
            return AbilityValidationResult.Complete();

        HashSet<string> keys = new HashSet<string>();

        foreach (var variable in variables)
        {
            if (variable == null)
                return AbilityValidationResult.Incomplete("Ability contains an empty variable.");

            if (string.IsNullOrWhiteSpace(variable.key))
                return AbilityValidationResult.Incomplete("Ability contains a variable without a key.");

            if (!keys.Add(variable.key))
                return AbilityValidationResult.Invalid($"Ability contains duplicated variable '{variable.key}'.");
        }

        return AbilityValidationResult.Complete();
    }
}
