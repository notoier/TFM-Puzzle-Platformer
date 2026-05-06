using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject
{
    // This list builds the logic for the ability via nodes.
    [SerializeReference]
    public List<AbilityNode> nodes = new();

    public bool canActivate = true;

    // When this function is called, it passes the user to get their AbilityContext. This context is passed to every node if events are not cancelled.
    public void Activate(GameObject actor)
    {
        canActivate = false;

        AbilityContext context = new AbilityContext
        {
            actor = actor
        };

        foreach (var node in nodes)
        {
            if (context.cancelled)
                break;

            node.Execute(context);
        }
    }

    public void End(GameObject actor)
    {

    }
}