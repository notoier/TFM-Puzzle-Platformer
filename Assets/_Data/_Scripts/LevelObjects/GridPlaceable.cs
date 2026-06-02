using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class GridPlaceable : MonoBehaviour
{
    public enum SnapMode
    {
        GridLine,
        CellCenter
    }

    [Header("Grid")]
    [SerializeField] private Grid grid;
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private bool snapManualMovementToGrid = true;

    [Header("Snap Settings")]
    [SerializeField] private SnapMode snapMode = SnapMode.GridLine;
    [SerializeField] private Vector2 snapOffset = Vector2.zero;
    [SerializeField] private float snapAfterMoveDelay = 0.25f;

    [Header("References")]
    [SerializeField] private WeightedPlatform platform;
    
#if UNITY_EDITOR
    private Vector3 _lastEditorPosition;
    private double _lastManualMoveTime;
    private bool _snapPending;

    private void OnValidate()
    {
        AutoAssignGrid();
        if (!platform) platform = GetComponent<WeightedPlatform>();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            return;

        if (!snapToGrid || !snapManualMovementToGrid || platform?.GetControlMode() == WeightedPlatform.ControlMode.External)
            return;

        DetectManualMovement();
        TrySnapAfterManualMovement();
    }

    [ContextMenu("Snap To Grid")]
    private void SnapToGridFromInspector()
    {
        if (!grid)
        {
            Debug.LogWarning("No Grid assigned. Cannot snap object to grid.", this);
            return;
        }

        transform.position = SnapPositionToGrid(transform.position);
        _lastEditorPosition = transform.position;
    }

    private void AutoAssignGrid()
    {
        if (grid != null)
            return;

#if UNITY_2023_1_OR_NEWER
        grid = FindFirstObjectByType<Grid>();
#else
        grid = FindObjectOfType<Grid>();
#endif
    }

    private void DetectManualMovement()
    {
        if (transform.position == _lastEditorPosition)
            return;

        _lastManualMoveTime = EditorApplication.timeSinceStartup;
        _snapPending = true;
        _lastEditorPosition = transform.position;
    }

    private void TrySnapAfterManualMovement()
    {
        if (!_snapPending)
            return;

        double elapsedTime = EditorApplication.timeSinceStartup - _lastManualMoveTime;

        if (elapsedTime < snapAfterMoveDelay)
            return;

        SnapToGridFromInspector();
        _snapPending = false;
    }

    private Vector3 SnapPositionToGrid(Vector3 position)
    {
        if (!grid)
            return position;

        Vector3 cellSize = grid.cellSize;

        float safeGridX = Mathf.Approximately(cellSize.x, 0f) ? 1f : cellSize.x;
        float safeGridY = Mathf.Approximately(cellSize.y, 0f) ? 1f : cellSize.y;

        switch (snapMode)
        {
            case SnapMode.CellCenter:
                position.x = Mathf.Floor(position.x / safeGridX) * safeGridX + safeGridX * 0.5f;
                position.y = Mathf.Floor(position.y / safeGridY) * safeGridY + safeGridY * 0.5f;
                break;

            case SnapMode.GridLine:
            default:
                position.x = Mathf.Round(position.x / safeGridX) * safeGridX;
                position.y = Mathf.Round(position.y / safeGridY) * safeGridY;
                break;
        }

        position.x += snapOffset.x * safeGridX;
        position.y += snapOffset.y * safeGridY;

        return position;
    }
#endif
}