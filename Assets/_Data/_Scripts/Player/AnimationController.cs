using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public CharacterMovement characterMovement;

    public void CreateSplash()
    {
        characterMovement.SpawnParticles();
    }
}
