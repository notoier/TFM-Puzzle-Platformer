using System;
using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    private CharacterMovement player;
    private WeightProvider weightProvider;
    
    private void Awake()
    {
        player = GameController.Instance?.GetPlayer();
        weightProvider = GetComponent<WeightProvider>();
    }

    private void Start()
    {
        player.SetCanSplit(false);
        this.transform.localScale = player.transform.localScale;
        weightProvider.Weight = player.Weight;
    }

    private void OnDestroy()
    {
        player.SetCanSplit(true);
    }
}
