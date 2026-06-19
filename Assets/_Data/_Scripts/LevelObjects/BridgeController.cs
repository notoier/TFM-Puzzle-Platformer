using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BridgeController : MonoBehaviour
{
    [Header("Break")] 
    [SerializeField] private HingeJoint2D breakingPoint;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private float breakVolume;
    [SerializeField] private bool breaksOnWalk;
    private bool isBroken;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out CharacterMovement characterMovement) || !breaksOnWalk || isBroken) return;
        StartCoroutine(Break());
    }

    private IEnumerator Break()
    {
        isBroken =  true;
        if (breakSound) AudioManager.Instance?.PlayEffect(breakSound, breakingPoint.transform, breakVolume, 1.2f, 1.3f);
        yield return new WaitForSeconds(breakSound.length * 0.5f);
        if (breakingPoint) breakingPoint.breakForce = -1;
    }

}
