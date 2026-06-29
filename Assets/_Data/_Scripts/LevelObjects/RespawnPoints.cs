using System.Collections.Generic;
using UnityEngine;

public class RespawnPoints : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform[] respawnPoints;
    [SerializeField] private Collider2D[] triggers;
    [SerializeField] private Collider2D pitTrigger;

    private readonly Dictionary<Collider2D, Transform> triggerToRespawnPoint = new();

    private Transform currentRespawnPoint;

    private void Awake()
    {
        BuildRespawnMap();

        if (respawnPoints.Length > 0)
            currentRespawnPoint = respawnPoints[0];
    }

    private void BuildRespawnMap()
    {
        triggerToRespawnPoint.Clear();

        int count = Mathf.Min(triggers.Length, respawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            if (triggers[i] == null || respawnPoints[i] == null)
                continue;

            triggers[i].isTrigger = true;

            triggerToRespawnPoint[triggers[i]] = respawnPoints[i];

            RespawnTrigger triggerDetector = triggers[i].GetComponent<RespawnTrigger>();

            if (triggerDetector == null)
                triggerDetector = triggers[i].gameObject.AddComponent<RespawnTrigger>();

            triggerDetector.Initialize(this, triggers[i]);
        }

        if (pitTrigger != null)
        {
            pitTrigger.isTrigger = true;

            RespawnTrigger pitDetector = pitTrigger.GetComponent<RespawnTrigger>();

            if (pitDetector == null)
                pitDetector = pitTrigger.gameObject.AddComponent<RespawnTrigger>();

            pitDetector.Initialize(this, pitTrigger);
        }
    }

    public void OnTriggerDetected(Collider2D ownTrigger, Collider2D other)
    {
        if (player == null || other.gameObject != player)
            return;

        if (ownTrigger == pitTrigger)
        {
            RespawnPlayer();
            return;
        }

        if (triggerToRespawnPoint.TryGetValue(ownTrigger, out Transform respawnPoint))
        {
            currentRespawnPoint = respawnPoint;
            //Debug.Log($"Nuevo respawn point: {respawnPoint.name}");
        }
    }

    private void RespawnPlayer()
    {
        if (player == null || currentRespawnPoint == null)
            return;

        player.transform.position = currentRespawnPoint.position;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }
}