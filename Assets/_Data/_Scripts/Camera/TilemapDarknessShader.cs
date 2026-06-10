using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDarknessShader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;

    [Header("Depth")] 
    [SerializeField] private int maxDepth = 6;
    [SerializeField, Range(0f, 1f)] private float maxDarkness = 0.9f;
    [SerializeField] private AnimationCurve darknessCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    private int _width;
    private int _height;
    private int _minX;
    private int _minY;
    
    private bool[,] _tilemapData;
    private int[,] _depthMap;
    
    private void Awake()
    {
        if (!tilemap) tilemap = GetComponent<Tilemap>();
        
        _width = tilemap.cellBounds.size.x;
        _height = tilemap.cellBounds.size.y;
        _minX = tilemap.cellBounds.min.x;
        _minY = tilemap.cellBounds.min.y;

        _tilemapData = new bool[_width, _height];
        _depthMap = new int[_width, _height];

        InitTilemapData();
        CalculateDepthMap();
        //DebugDepthMapValues();
    }

    private void InitTilemapData()
    {
        for (int matrixX = 0; matrixX < _width; matrixX++)
        {
            for (int matrixY = 0; matrixY < _height; matrixY++)
            {
                Vector3Int cellPosition = new Vector3Int(matrixX + _minX, matrixY + _minY, 0);
                
                bool hasTile = tilemap.HasTile(cellPosition);

                _tilemapData[matrixX, matrixY] = hasTile;

                _depthMap[matrixX, matrixY] = hasTile ? 0 : -1;

            }
        }
    }

    private void CalculateDepthMap()
    {
        PropagateDepthBFS();
        FillRemainingSolidTiles();
        ApplyDepthDarkness();
    }
    
    private void PropagateDepthBFS()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_tilemapData[x, y])
                    continue;

                if (HasAirNeighbour(x, y))
                {
                    _depthMap[x, y] = 1;
                    queue.Enqueue(new Vector2Int(x, y));
                }
            }
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            int currentDepth = _depthMap[current.x, current.y];

            if (currentDepth >= maxDepth)
                continue;

            TrySetDepth(current.x + 1, current.y, currentDepth + 1, queue);
            TrySetDepth(current.x - 1, current.y, currentDepth + 1, queue);
            TrySetDepth(current.x, current.y + 1, currentDepth + 1, queue);
            TrySetDepth(current.x, current.y - 1, currentDepth + 1, queue);
        }
    }
    
    private void TrySetDepth(int x, int y, int depth, Queue<Vector2Int> queue)
    {
        if (!IsInsideMap(x, y))
            return;

        if (!_tilemapData[x, y])
            return;

        if (_depthMap[x, y] != 0)
            return;

        _depthMap[x, y] = depth;
        queue.Enqueue(new Vector2Int(x, y));
    }
    
    private void MarkBorderTiles()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_depthMap[x, y] != 0)
                    continue;

                if (HasAirNeighbour(x, y))
                {
                    _depthMap[x, y] = 1;
                }
            }
        }
    }

    private void PropagateDepth()
    {
        for (int currentDepth = 1; currentDepth < maxDepth; currentDepth++)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_depthMap[x, y] != 0)
                        continue;

                    if (HasNeighbourWithDepth(x, y))
                    {
                        _depthMap[x, y] = currentDepth + 1;
                    }
                }
            }
        }
    }
    
    private void FillRemainingSolidTiles()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_depthMap[x, y] == 0)
                {
                    _depthMap[x, y] = maxDepth;
                }
            }
        }
    }
    
    private void ApplyDepthDarkness()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int depth = _depthMap[x, y];

                if (depth == -1)
                    continue;

                Vector3Int cellPosition = new Vector3Int(
                    x + _minX,
                    y + _minY,
                    0
                );

                float normalizedDepth = Mathf.InverseLerp(1, maxDepth, depth);
                float curvedDepth = darknessCurve.Evaluate(normalizedDepth);
                float lightMultiplier = Mathf.Lerp(1f, 1f - maxDarkness, curvedDepth);

                Color color = new Color(
                    lightMultiplier,
                    lightMultiplier,
                    lightMultiplier,
                    1f
                );

                tilemap.SetTileFlags(cellPosition, TileFlags.None);
                tilemap.SetColor(cellPosition, color);
            }
        }
    }
    
    private bool HasNeighbourWithDepth(int x, int y)
    {
        return HasDepth(x + 1, y) ||
               HasDepth(x - 1, y) ||
               HasDepth(x, y + 1) ||
               HasDepth(x, y - 1);
    }
    
    private bool HasDepth(int x, int y)
    {
        if (!IsInsideMap(x, y))
            return false;

        return !_tilemapData[x, y];
    }

    private bool IsInsideMap(int x, int y)
    {
        return x >= 0 && x < _width &&
               y >= 0 && y < _height;
    }
    
    private bool IsAir(int x, int y)
    {
        if (!IsInsideMap(x, y))
            return true;

        return _depthMap[x, y] == -1;
    }
    
    private bool HasAirNeighbour(int x, int y)
    {
        return IsAir(x + 1, y) ||
               IsAir(x - 1, y) ||
               IsAir(x, y + 1) ||
               IsAir(x, y - 1);
    }
    
    
    /* DEBUG 
    
    private void DebugDepthMapValues()
    {
        int[] counts = new int[maxDepth + 1];
        int airCount = 0;
        int pendingCount = 0;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int depth = _depthMap[x, y];

                if (depth == -1)
                {
                    airCount++;
                }
                else if (depth == 0)
                {
                    pendingCount++;
                }
                else if (depth >= 1 && depth <= maxDepth)
                {
                    counts[depth]++;
                }
            }
        }

        Debug.Log($"Air: {airCount}");
        Debug.Log($"Pending 0: {pendingCount}");

        for (int i = 1; i <= maxDepth; i++)
        {
            Debug.Log($"Depth {i}: {counts[i]}");
        }
    }
    
    */
}
