using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SewerAreaController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameController.Instance?.GameAreaChanged(GameArea.Sewer);
    }
}
