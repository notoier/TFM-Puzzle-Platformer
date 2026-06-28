using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    public CharacterMovement characterMovement;
    public AbilityManager abilityManager;
    public PlayerInput playerInput;

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
    }

    public void canMoveEnable()
    {
        playerInput.enabled = true;
    }
}
