using System;
using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    private CharacterMovement player;
    
    private void Awake()
    {
        player = GameController.Instance?.GetPlayer();
    }

    private void Start()
    {
        player.SetCanSplit(false);
    }

    private void OnDestroy()
    {
        player.SetCanSplit(true);
    }
}
