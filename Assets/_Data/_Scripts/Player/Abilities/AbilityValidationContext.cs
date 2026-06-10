using System.Collections.Generic;

public class AbilityValidationContext
{
    private readonly IReadOnlyList<AbilityVariableDefinition> variables;

    public AbilityValidationContext(IReadOnlyList<AbilityVariableDefinition> variables)
    {
        this.variables = variables;
    }

    public bool TryGetVariable(string key, out AbilityVariableDefinition variable)
    {
        variable = null;

        if (string.IsNullOrWhiteSpace(key) || variables == null)
            return false;

        foreach (var candidate in variables)
        {
            if (candidate == null)
                continue;

            if (candidate.key == key)
            {
                variable = candidate;
                return true;
            }
        }

        return false;
    }

    public bool HasVariable(string key, AbilityValueType expectedType)
    {
        return TryGetVariable(key, out AbilityVariableDefinition variable) && variable.type == expectedType;
    }
}
