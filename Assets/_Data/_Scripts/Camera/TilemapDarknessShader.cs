using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class TilemapDarknessShader : MonoBehaviour
{
    private const float DiagonalCost = 1.41421356f;
    private const float InfiniteDistance = 999999f;

    [Header("References")]
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private TilemapRenderer tilemapRenderer;

    [SerializeField]
    private Material darknessMaterial;

    [Header("Depth")]
    [Tooltip("Distancia, medida aproximadamente en tiles, a la que se alcanza la oscuridad máxima.")]
    [SerializeField, Min(2f)]
    private float maxDepth = 12f;

    [Tooltip("Oscuridad máxima. Con 0.6, el color conserva como mínimo el 40 % de su luminosidad.")]
    [SerializeField, Range(0f, 1f)]
    private float maxDarkness = 0.6f;

    [Tooltip("Transforma la profundidad normalizada en oscuridad.")]
    [SerializeField]
    private AnimationCurve darknessCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.4f, 0.05f),
        new Keyframe(0.7f, 0.3f),
        new Keyframe(1f, 1f)
    );

    [Header("Mask")]
    [Tooltip("Margen añadido alrededor del Tilemap para representar explícitamente el aire exterior.")]
    [SerializeField, Min(1)]
    private int boundsPadding = 1;
    
    [Header("Mask Resolution")]
    [SerializeField, Range(1, 16)]
    private int pixelsPerTile = 4;

    [SerializeField] private bool highResActive;

    [Header("Edge Lighting")]

    [SerializeField, Range(0f, 1f)]
    private float lightEdgeStart = 0f;

    [SerializeField, Range(0f, 1f)]
    private float lightEdgeEnd = 0.04f;
    
    [Header("Debug")]
    [SerializeField] private bool showRawDistanceMask;
    
    [Tooltip("Bilinear suaviza la máscara. Point permite depurar sus valores por tile.")]
    [SerializeField]
    private FilterMode maskFilterMode = FilterMode.Bilinear;

    private static readonly int DepthTextureId =
        Shader.PropertyToID("_DepthTexture");

    private static readonly int TilemapWorldMinId =
        Shader.PropertyToID("_TilemapWorldMin");

    private static readonly int TilemapWorldSizeId =
        Shader.PropertyToID("_TilemapWorldSize");
    
    private static readonly int LightEdgeStartId =
        Shader.PropertyToID("_LightEdgeStart");

    private static readonly int LightEdgeEndId =
        Shader.PropertyToID("_LightEdgeEnd");

    private int _width;
    private int _height;
    private int _minX;
    private int _minY;

    private bool[,] _tilemapData;
    private float[,] _distanceMap;
    
    private bool[,] _highResolutionSolidMap;
    private float[,] _highResolutionDistanceMap;

    private int _textureWidth;
    private int _textureHeight;
    
    private Texture2D _depthTexture;
    //private MaterialPropertyBlock _propertyBlock;
    private Material _runtimeMaterial;
    
    [SerializeField, Range(0.1f, 2f)]
    private float darknessExponent = 0.55f;

    private float CalculateDarkness(int x, int y)
    {
        if (!_tilemapData[x, y])
            return 0f;

        float distance = _distanceMap[x, y];

        float normalizedDepth = Mathf.InverseLerp(
            1f,
            maxDepth,
            distance
        );

        float shapedDepth = Mathf.Pow(
            normalizedDepth,
            darknessExponent
        );

        return Mathf.Clamp01(
            shapedDepth * maxDarkness
        );
    }
    
    private void Awake()
    {
        InitializeReferences();

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        GenerateDarkness();
    }

    private void InitializeReferences()
    {
        if (!tilemap)
            tilemap = GetComponent<Tilemap>();

        if (!tilemapRenderer)
            tilemapRenderer = GetComponent<TilemapRenderer>();
    }
    
    private bool ValidateReferences()
    {
        if (!tilemap)
        {
            Debug.LogError("No se ha encontrado un componente Tilemap.", this);
            return false;
        }

        if (!tilemapRenderer)
        {
            Debug.LogError("No se ha encontrado un TilemapRenderer.", this);
            return false;
        }

        if (!darknessMaterial)
        {
            Debug.LogError(
                "No se ha asignado el material de oscurecimiento.",
                this
            );

            return false;
        }

        return true;
    }

    public void GenerateDarkness()
    {
        AssignMaterial();
        CalculateBounds();
        CreateDataArrays();

        InitializeTilemapData();
        ResetTileColors();
        
        if (highResActive)
        {
            InitializeHighResolutionMap();

            ForwardHighResolutionPass();
            BackwardHighResolutionPass();
            DebugHighResDistanceRange();
            
            GenerateHighResolutionDepthTexture();
        }
        else
        {

            InitializeDistanceMap();
            CalculateDistanceTransform();
            DebugDistanceRange();

            GenerateDepthTexture();  
        }
        
        ApplyTextureToShader();
    }

    private void AssignMaterial()
    {
        if (_runtimeMaterial != null)
        {
            Destroy(_runtimeMaterial);
        }

        _runtimeMaterial = new Material(darknessMaterial)
        {
            name = $"{darknessMaterial.name} ({name} Instance)"
        };

        tilemapRenderer.material = _runtimeMaterial;
    }

    private void CalculateBounds()
    {
        tilemap.CompressBounds();

        BoundsInt tileBounds = tilemap.cellBounds;

        /*
         * Ampliamos los bounds para que el aire exterior esté representado
         * dentro de la propia matriz. De esta forma, el cálculo no depende
         * de tratar directamente los índices externos como aire.
         */
        _minX = tileBounds.xMin - boundsPadding;
        _minY = tileBounds.yMin - boundsPadding;

        _width = tileBounds.size.x + boundsPadding * 2;
        _height = tileBounds.size.y + boundsPadding * 2;
    }

    private void CreateDataArrays()
    {
        _tilemapData = new bool[_width, _height];
        _distanceMap = new float[_width, _height];
    }

    private void InitializeTilemapData()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3Int cellPosition = MatrixToCell(x, y);

                _tilemapData[x, y] =
                    tilemap.HasTile(cellPosition);
            }
        }
    }

    private void InitializeHighResolutionMap()
    {
        _textureWidth = _width * pixelsPerTile;
        _textureHeight = _height * pixelsPerTile;

        _highResolutionSolidMap =
            new bool[_textureWidth, _textureHeight];

        _highResolutionDistanceMap =
            new float[_textureWidth, _textureHeight];

        for (int tileX = 0; tileX < _width; tileX++)
        {
            for (int tileY = 0; tileY < _height; tileY++)
            {
                bool isSolid = _tilemapData[tileX, tileY];

                int startX = tileX * pixelsPerTile;
                int startY = tileY * pixelsPerTile;

                for (int localX = 0; localX < pixelsPerTile; localX++)
                {
                    for (int localY = 0; localY < pixelsPerTile; localY++)
                    {
                        int pixelX = startX + localX;
                        int pixelY = startY + localY;

                        _highResolutionSolidMap[pixelX, pixelY] =
                            isSolid;

                        _highResolutionDistanceMap[pixelX, pixelY] =
                            isSolid ? InfiniteDistance : 0f;
                    }
                }
            }
        }
    }
    
    private void ResetTileColors()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_tilemapData[x, y])
                    continue;

                Vector3Int cellPosition = MatrixToCell(x, y);

                tilemap.SetTileFlags(
                    cellPosition,
                    TileFlags.None
                );

                tilemap.SetColor(
                    cellPosition,
                    Color.white
                );
            }
        }
    }

    private void InitializeDistanceMap()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                /*
                 * El aire es el origen de la distancia.
                 * Los tiles sólidos empiezan con distancia infinita.
                 */
                _distanceMap[x, y] =
                    _tilemapData[x, y]
                        ? InfiniteDistance
                        : 0f;
            }
        }
    }

    private void CalculateDistanceTransform()
    {
        ForwardDistancePass();
        BackwardDistancePass();
    }

    private void ForwardDistancePass()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (!_tilemapData[x, y])
                    continue;

                float best = _distanceMap[x, y];

                best = Mathf.Min(best, GetDistance(x - 1, y) + 1f);
                best = Mathf.Min(best, GetDistance(x, y - 1) + 1f);

                best = Mathf.Min(
                    best,
                    GetDistance(x - 1, y - 1) + DiagonalCost
                );

                best = Mathf.Min(
                    best,
                    GetDistance(x + 1, y - 1) + DiagonalCost
                );

                _distanceMap[x, y] = best;
            }
        }
    }
    
    private void ForwardHighResolutionPass()
    {
        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                if (!_highResolutionSolidMap[x, y])
                    continue;

                float best = _highResolutionDistanceMap[x, y];

                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x - 1, y) + 1f
                );

                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x, y - 1) + 1f
                );

                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x - 1, y - 1)
                    + DiagonalCost
                );

                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x + 1, y - 1)
                    + DiagonalCost
                );

                _highResolutionDistanceMap[x, y] = best;
            }
        }
    }

    private void BackwardDistancePass()
    {
        for (int y = _height - 1; y >= 0; y--)
        {
            for (int x = _width - 1; x >= 0; x--)
            {
                if (!_tilemapData[x, y])
                    continue;

                float best = _distanceMap[x, y];

                best = Mathf.Min(best, GetDistance(x + 1, y) + 1f);
                best = Mathf.Min(best, GetDistance(x, y + 1) + 1f);

                best = Mathf.Min(
                    best,
                    GetDistance(x + 1, y + 1) + DiagonalCost
                );

                best = Mathf.Min(
                    best,
                    GetDistance(x - 1, y + 1) + DiagonalCost
                );

                _distanceMap[x, y] = best;
            }
        }
    }
    
    private void BackwardHighResolutionPass()
    {
        for (int y = _textureHeight - 1; y >= 0; y--)
        {
            for (int x = _textureWidth - 1; x >= 0; x--)
            {
                if (!_highResolutionSolidMap[x, y])
                    continue;

                float best = _highResolutionDistanceMap[x, y];

                // Derecha
                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x + 1, y) + 1f
                );

                // Arriba
                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x, y + 1) + 1f
                );

                // Diagonal superior derecha
                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x + 1, y + 1)
                    + DiagonalCost
                );

                // Diagonal superior izquierda
                best = Mathf.Min(
                    best,
                    GetHighResolutionDistance(x - 1, y + 1)
                    + DiagonalCost
                );

                _highResolutionDistanceMap[x, y] = best;
            }
        }
    }

    private float GetDistance(int x, int y)
    {
        if (!IsInsideMap(x, y))
            return InfiniteDistance;

        return _distanceMap[x, y];
    }

    private float GetHighResolutionDistance(int x, int y)
    {
        if (x < 0 || x >= _textureWidth ||
            y < 0 || y >= _textureHeight)
        {
            return InfiniteDistance;
        }

        return _highResolutionDistanceMap[x, y];
    }
    
    private float CalculateHighResolutionDarkness(int x, int y)
    {
        if (!_highResolutionSolidMap[x, y])
            return 0f;

        float distance =
            _highResolutionDistanceMap[x, y];

        if (distance >= InfiniteDistance * 0.5f)
            return 0f;

        float maxDepthInPixels =
            maxDepth * pixelsPerTile;

        float normalizedDepth = Mathf.InverseLerp(
            1f,
            maxDepthInPixels,
            distance
        );

        float shapedDepth = Mathf.Pow(
            normalizedDepth,
            darknessExponent
        );

        return Mathf.Clamp01(
            shapedDepth * maxDarkness
        );
    }
    
    private float CalculateNormalizedDepth(int x, int y)
    {
        if (!_tilemapData[x, y])
            return 0f;

        float distance = _distanceMap[x, y];

        if (distance >= InfiniteDistance * 0.5f)
            return 0f;

        return Mathf.InverseLerp(
            1f,
            maxDepth,
            distance
        );
    }
    
    private float CalculateHighResolutionNormalizedDepth(int x, int y)
    {
        if (!_highResolutionSolidMap[x, y])
            return 0f;

        float distance = _highResolutionDistanceMap[x, y];

        if (distance >= InfiniteDistance * 0.5f)
            return 0f;

        float maxDepthInPixels = maxDepth * pixelsPerTile;

        return Mathf.InverseLerp(
            1f,
            maxDepthInPixels,
            distance
        );
    }
    
    private void GenerateDepthTexture()
    {
        DestroyDepthTexture();

        _depthTexture = new Texture2D(
            _width,
            _height,
            TextureFormat.RG16,
            false,
            true
        )
        {
            name = $"{name}_DepthMask",
            filterMode = maskFilterMode,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[_width * _height];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float normalizedDepth =
                    CalculateNormalizedDepth(x, y);

                float darkness =
                    CalculateDarkness(x, y);

                pixels[y * _width + x] =
                    new Color(
                        normalizedDepth,
                        darkness,
                        0f,
                        1f
                    );
            }
        }

        _depthTexture.SetPixels(pixels);
        _depthTexture.Apply(false, false);
    }
    
    private void GenerateHighResolutionDepthTexture()
    {
        DestroyDepthTexture();

        _depthTexture = new Texture2D(
            _textureWidth,
            _textureHeight,
            TextureFormat.RG16,
            false,
            true
        )
        {
            name = $"{name}_HighResolutionDepthMask",
            filterMode = maskFilterMode,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels =
            new Color[_textureWidth * _textureHeight];

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                float darkness =
                    CalculateHighResolutionDarkness(x, y);

                float normalizedDepth =
                    CalculateHighResolutionNormalizedDepth(x, y);

                pixels[y * _textureWidth + x] = new Color(
                    normalizedDepth,        // R: profundidad para la máscara de luz
                    darkness, // G: oscurecimiento que ya funcionaba
                    0f,
                    1f
                );
            }
        }

        _depthTexture.SetPixels(pixels);
        _depthTexture.Apply(false, false);
    }
    
    private void DebugDistanceRange()
    {
        float minDistance = float.MaxValue;
        float maxDistance = float.MinValue;
        int solidTiles = 0;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_tilemapData[x, y])
                    continue;

                float distance = _distanceMap[x, y];

                minDistance = Mathf.Min(minDistance, distance);
                maxDistance = Mathf.Max(maxDistance, distance);
                solidTiles++;
            }
        }

        Debug.Log(
            $"Distance map | Solids: {solidTiles} | " +
            $"Min: {minDistance:F2} | Max: {maxDistance:F2}",
            this
        );
    }

    private void DebugHighResDistanceRange()
    {
        float minDistance = float.MaxValue;
        float maxDistance = float.MinValue;
        int solidCount = 0;
        int infiniteCount = 0;

        for (int y = 0; y < _textureHeight; y++)
        {
            for (int x = 0; x < _textureWidth; x++)
            {
                if (!_highResolutionSolidMap[x, y])
                    continue;

                float distance = _highResolutionDistanceMap[x, y];
                solidCount++;

                if (distance >= InfiniteDistance * 0.5f)
                {
                    infiniteCount++;
                    continue;
                }

                minDistance = Mathf.Min(minDistance, distance);
                maxDistance = Mathf.Max(maxDistance, distance);
            }
        }

        Debug.Log(
            $"HighRes Distance | Solids: {solidCount} | Infinite: {infiniteCount} | Min: {minDistance:F2} | Max: {maxDistance:F2}",
            this
        );
    }
    
    private void ApplyTextureToShader()
    {
        Vector3 worldMin = tilemap.CellToWorld(
            new Vector3Int(_minX, _minY, 0)
        );

        Vector3 worldMax = tilemap.CellToWorld(
            new Vector3Int(
                _minX + _width,
                _minY + _height,
                0
            )
        );

        Vector2 worldSize = new Vector2(
            worldMax.x - worldMin.x,
            worldMax.y - worldMin.y
        );

        if (!_runtimeMaterial)
        {
            Debug.LogError("Runtime material has not been created.", this);
            return;
        }

        _runtimeMaterial.SetTexture(
            DepthTextureId,
            _depthTexture
        );

        _runtimeMaterial.SetVector(
            TilemapWorldMinId,
            new Vector4(worldMin.x, worldMin.y, 0f, 0f)
        );

        _runtimeMaterial.SetVector(
            TilemapWorldSizeId,
            new Vector4(worldSize.x, worldSize.y, 0f, 0f)
        );
        
        _runtimeMaterial.SetFloat(
            LightEdgeStartId,
            lightEdgeStart
        );

        _runtimeMaterial.SetFloat(
            LightEdgeEndId,
            lightEdgeEnd
        );
    }

    private Vector3Int MatrixToCell(int x, int y)
    {
        return new Vector3Int(
            x + _minX,
            y + _minY,
            0
        );
    }

    private bool IsInsideMap(int x, int y)
    {
        return x >= 0 &&
               x < _width &&
               y >= 0 &&
               y < _height;
    }
    
    private void OnValidate()
    {
        maxDepth = Mathf.Max(2f, maxDepth);
        boundsPadding = Mathf.Max(1, boundsPadding);
    }

    private void OnDestroy()
    {
        DestroyDepthTexture();

        if (_runtimeMaterial != null)
        {
            Destroy(_runtimeMaterial);
            _runtimeMaterial = null;
        }
    }

    private void DestroyDepthTexture()
    {
        if (!_depthTexture)
            return;

        Destroy(_depthTexture);
        _depthTexture = null;
    }
}