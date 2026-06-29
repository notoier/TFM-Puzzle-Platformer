using UnityEngine;

public enum GameArea
{
    Sewer,
    Cave,
    End
}

public class GameController : PersistentSingleton<GameController>
{
    [Header("References")]
    [SerializeField] private CharacterMovement player;

    [Header("Area Managment")]
    [SerializeField] private AudioClip sewerAreaAmbienceMusic;
    [SerializeField] private float sewerAreaAmbienceVolume;
    [SerializeField] private AudioClip caveAreaAmbienceMusic;
    [SerializeField] private float caveAreaAmbienceVolume;
    [SerializeField] private AudioClip boxAreaAmbienceMusic;
    [SerializeField] private float boxAreaAmbienceVolume;

    [SerializeField] private GameArea currentArea;
    
    private AudioClip currentAmbienceMusic;
    private float currentVolume = 1f;

    protected override void Awake()
    {
        base.Awake();
        if (!player) player = FindAnyObjectByType<CharacterMovement>();   
    }

    private void Start()
    {
        UpdateMusic();
    }
    
    public float GetDistanceToPlayer(Vector3 objectPosition)
    {
        return Vector3.Distance(objectPosition, player.transform.position);
    }

    public CharacterMovement GetPlayer()
    {
        return player;
    }

    public void GameAreaChanged(GameArea newArea)
    {
        if (newArea == currentArea) return;
        
        currentArea = newArea;
        UpdateMusic();
    }

    private void UpdateMusic()
    {
        currentAmbienceMusic = currentArea switch
        {
            GameArea.Sewer => sewerAreaAmbienceMusic,
            GameArea.Cave => caveAreaAmbienceMusic,
            GameArea.End => boxAreaAmbienceMusic,
            _ => currentAmbienceMusic
        };
        
        currentVolume = currentArea switch
        {
            GameArea.Sewer => sewerAreaAmbienceVolume,
            GameArea.Cave => caveAreaAmbienceVolume,
            GameArea.End => boxAreaAmbienceVolume,
            _ => currentVolume
        };

        RestartMusic();
    }

    private void RestartMusic()
    {
        if (currentAmbienceMusic) AudioManager.Instance?.PlayMusic(currentAmbienceMusic, currentVolume);
    }
    
}
