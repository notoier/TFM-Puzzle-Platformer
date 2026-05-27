using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PlatformVisualBuilder : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite centerSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Size")]
    [Min(1)]
    [SerializeField] private int centerPieces = 3;

    [SerializeField] private float pieceWidth = 1f;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 0;

    [Header("References")]
    [SerializeField] private Transform visualRoot;
    
    
#if UNITY_EDITOR
    [Header("Piece Size")]
    [SerializeField] private Vector2 targetPieceSize = Vector2.one;
    [SerializeField] private bool scaleSpritesToPieceSize = true;
    
    private bool _buildQueued;

    private void OnValidate()
    {
        QueueBuildPlatform();
    }

    private void QueueBuildPlatform()
    {
        if (_buildQueued)
            return;

        _buildQueued = true;

        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            _buildQueued = false;

            if (EditorUtility.IsPersistent(gameObject))
                return;

            BuildPlatform();
        };
    }

    private float GetSpriteWidth(Sprite sprite)
    {
        if (!sprite)
            return 1f;

        return sprite.bounds.size.x;
    }
    
    [ContextMenu("Build Platform Visuals")]
    private void BuildPlatform()
    {
        if (EditorUtility.IsPersistent(gameObject))
            return;

        if (!leftSprite || !centerSprite || !rightSprite)
            return;

        if (!visualRoot)
        {
            Transform existingRoot = transform.Find("Visuals");

            if (existingRoot)
            {
                visualRoot = existingRoot;
            }
            else
            {
                GameObject rootObject = new GameObject("Visuals");
                rootObject.transform.SetParent(transform);
                rootObject.transform.localPosition = Vector3.zero;
                rootObject.transform.localRotation = Quaternion.identity;
                rootObject.transform.localScale = Vector3.one;

                visualRoot = rootObject.transform;
            }
        }

        ClearVisuals();
        
        float pieceWidth = targetPieceSize.x;

        int totalPieces = centerPieces + 2;
        float totalWidth = totalPieces * pieceWidth;
        float startX = -totalWidth * 0.5f + pieceWidth * 0.5f;

        CreatePiece("Left", leftSprite, startX);

        for (int i = 0; i < centerPieces; i++)
        {
            float x = startX + pieceWidth * (i + 1);
            CreatePiece($"Center_{i + 1}", centerSprite, x);
        }

        float rightX = startX + pieceWidth * (totalPieces - 1);
        CreatePiece("Right", rightSprite, rightX);
        
        UpdateWeightedPlatformColliders();
    }

    private void CreatePiece(string pieceName, Sprite sprite, float localX)
    {
        GameObject pieceObject = new GameObject(pieceName);
        pieceObject.transform.SetParent(visualRoot);
        pieceObject.transform.localPosition = new Vector3(localX, 0f, 0f);
        pieceObject.transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;

        pieceObject.transform.localScale = scaleSpritesToPieceSize
            ? CalculateScaleForSprite(sprite, targetPieceSize)
            : Vector3.one;
    }
    
    private Vector3 CalculateScaleForSprite(Sprite sprite, Vector2 targetSize)
    {
        if (!sprite)
            return Vector3.one;

        Vector2 spriteSize = sprite.bounds.size;

        float safeSpriteWidth = Mathf.Approximately(spriteSize.x, 0f) ? 1f : spriteSize.x;
        float safeSpriteHeight = Mathf.Approximately(spriteSize.y, 0f) ? 1f : spriteSize.y;

        return new Vector3(
            targetSize.x / safeSpriteWidth,
            targetSize.y / safeSpriteHeight,
            1f
        );
    }

    private void ClearVisuals()
    {
        if (!visualRoot)
            return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
    
    private void UpdateWeightedPlatformColliders()
    {
        WeightedPlatform weightedPlatform = GetComponent<WeightedPlatform>();

        if (!weightedPlatform)
            return;

        Vector2 platformSize = GetPlatformSize();
        weightedPlatform.UpdateColliders(platformSize);
    }

    private Vector2 GetPlatformSize()
    {
        int totalPieces = centerPieces + 2;

        return new Vector2(
            totalPieces * targetPieceSize.x,
            targetPieceSize.y
        );
    }
    
#endif


}
