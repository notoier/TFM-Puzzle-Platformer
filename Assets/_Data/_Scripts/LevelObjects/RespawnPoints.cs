using System;
using UnityEngine;

public class RespawnPoints : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform respawnPoint;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            player.transform.position = respawnPoint.position;
    }
}
