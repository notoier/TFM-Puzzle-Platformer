using UnityEngine;

public class WaterController : MonoBehaviour
{
    public float maxLength = 10f;
    public float fallSpeed = 1f;

    public LineRenderer lineRenderer;
    public LayerMask obstacleLayerMask;
    public BoxCollider2D boxCollider2D;
    public GameObject splashEffectObject;

    private float currentLength;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int LengthId =
        Shader.PropertyToID("_Length");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
    }

    private void Update()
    {
        bool hitObstacle = GetCurrentLength(out float targetLength);

        currentLength = Mathf.MoveTowards(
            currentLength,
            targetLength,
            fallSpeed * Time.deltaTime
        );

        ResizeLine(currentLength);
        RescaleCollider(currentLength);
        CheckForSplashEffect(
            currentLength,
            targetLength,
            hitObstacle
        );
    }

    private bool GetCurrentLength(out float targetLength)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            maxLength,
            obstacleLayerMask
        );

        if (hit.collider != null)
        {
            targetLength = hit.distance;
            return true;
        }

        targetLength = maxLength;
        return false;
    }

    private void ResizeLine(float length)
    {
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(
            1,
            Vector3.down * length
        );

        lineRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(LengthId, length);
        lineRenderer.SetPropertyBlock(propertyBlock);
    }

    private void RescaleCollider(float length)
    {
        boxCollider2D.size = new Vector2(
            lineRenderer.startWidth,
            length
        );

        boxCollider2D.offset = new Vector2(
            0f,
            -length * 0.5f
        );
    }

    private void CheckForSplashEffect(
        float length,
        float targetLength,
        bool hitObstacle)
    {
        bool active =
            hitObstacle &&
            Mathf.Approximately(length, targetLength);

        splashEffectObject.SetActive(active);
        splashEffectObject.transform.localPosition =
            Vector3.down * length;
    }
}