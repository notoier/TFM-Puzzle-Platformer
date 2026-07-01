using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    public CharacterMovement characterMovement;
    public AbilityManager abilityManager;
    public PlayerInput playerInput;
    public Rigidbody2D playerRb2D;
    
    public void CreateSplash()
    {
        characterMovement.SpawnParticles();
    }

    public void LittleShake()
    {
        CameraShake.Instance.Shake(CameraShake.ShakeType.Small);
    }

    public void divideStart()
    {
        abilityManager.ActivateAbility(1);
    }

    public void canMoveDisable()
    {
        playerInput.enabled = false;
        playerRb2D.constraints = RigidbodyConstraints2D.FreezePositionX;

    }

    public void canMoveEnable()
    {
        playerInput.enabled = true;
        playerRb2D.linearVelocity  = Vector2.zero;
        playerRb2D.constraints = RigidbodyConstraints2D.None;
        playerRb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
