using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public CharacterMovement characterMovement;

    public void CreateSplash()
    {
        characterMovement.SpawnParticles();
    }

    public void LittleShake()
    {
        CameraShake.Instance.Shake(CameraShake.ShakeType.Small);
    }
}
