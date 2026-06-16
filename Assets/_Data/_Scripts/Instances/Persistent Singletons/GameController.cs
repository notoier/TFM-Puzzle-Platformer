using UnityEngine;

public class GameController : PersistentSingleton<GameController>
{
    [Header("References")]
    [SerializeField] private CharacterMovement player;


    protected override void Awake()
    {
        base.Awake();
        if (!player) player = FindAnyObjectByType<CharacterMovement>();   
    }
    
    
    public float GetDistanceToPlayer(Vector3 objectPosition)
    {
        return Vector3.Distance(objectPosition, player.transform.position);
    }
    
    
}
