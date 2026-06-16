using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class MeshSortingLayer : MonoBehaviour
{
    [SerializeField] private string sortingLayerName = "Water";
    [SerializeField] private int orderInLayer;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        ApplySorting();
    }

    private void OnValidate()
    {
        ApplySorting();
    }

    private void ApplySorting()
    {
        if (!_meshRenderer)
            _meshRenderer = GetComponent<MeshRenderer>();

        _meshRenderer.sortingLayerName = sortingLayerName;
        _meshRenderer.sortingOrder = orderInLayer;
    }
}