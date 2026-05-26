using System;
using UnityEngine;

[Serializable]
public abstract class AbilityNode
{
    /// <summary>
    /// Executes the specific behavior of this ability node using the provided context. This method must be implemented by all concrete subclasses of <see cref="AbilityNode"/>
    /// to define their unique functionality when activated within an ability sequence.
    /// </summary>
    /// <param name="context">The ability context to use for execution. Cannot be null.</param>
    public abstract void Execute(AbilityContext context);

    /// <summary>
    /// An optional implementation of the default behavior for an ability node. This method is called when the node is executed without any specific behavior defined.
    /// </summary>
    /// <remarks>This method sets the <c>cancelled</c> property of the provided <paramref name="context"/> to
    /// <see langword="true"/> as it's default implementation to trigger the end of the ability automatically. Override this method to customize the default behaviour
    /// the node should execute after the <c>success</c> boolean of the provided <paramref name="context"/> is set to false in the node execution.
    /// </remarks>
    /// <param name="context">The ability context to update. Cannot be null.</param>
    public virtual void DefaultBehavior(AbilityContext context)
    {
       context.cancelled = true;
    }
}

