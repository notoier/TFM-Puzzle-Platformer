public enum AbilityValidationState
{
    Incomplete,
    Invalid,
    Complete,
    Ready
}

public class AbilityValidationResult
{
    public AbilityValidationState State { get; }
    public string Message { get; }
    public bool BlocksUse => State == AbilityValidationState.Incomplete || State == AbilityValidationState.Invalid;

    private AbilityValidationResult(AbilityValidationState state, string message)
    {
        State = state;
        Message = message;
    }

    public static AbilityValidationResult Incomplete(string message)
    {
        return new AbilityValidationResult(AbilityValidationState.Incomplete, message);
    }

    public static AbilityValidationResult Invalid(string message)
    {
        return new AbilityValidationResult(AbilityValidationState.Invalid, message);
    }

    public static AbilityValidationResult Complete(string message = "")
    {
        return new AbilityValidationResult(AbilityValidationState.Complete, message);
    }

    public static AbilityValidationResult Ready(string message = "")
    {
        return new AbilityValidationResult(AbilityValidationState.Ready, message);
    }
}
