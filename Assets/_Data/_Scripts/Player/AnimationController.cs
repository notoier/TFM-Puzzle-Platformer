using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public CharacterMovement characterMovement;
    public AbilityManager abilityManager;

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
        abilityManager.ActivateAbility(0);
    }
}
