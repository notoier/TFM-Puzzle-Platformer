using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cainos.InteractivePixelWater
{
    /// <summary>
    /// Cascada vertical compatible con el sistema Interactive Pixel Water.
    ///
    /// Genera una malla subdividida hacia abajo, aplica una deformación suave
    /// a sus bordes y puede interactuar físicamente con Rigidbody2D.
    ///
    /// También puede conectar su punto de impacto con un PixelWater horizontal
    /// para generar ondas y salpicaduras.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PixelWaterfall : MonoBehaviour
    {
        private const int MIN_VERTEX_COUNT_X = 2;
        private const int MIN_VERTEX_COUNT_Y = 2;

        private static readonly int WaterColorShallowId =
            Shader.PropertyToID("_WaterColorShallow");

        private static readonly int WaterColorDeepId =
            Shader.PropertyToID("_WaterColorDeep");

        private static readonly int UnderwaterTintShallowId =
            Shader.PropertyToID("_UnderwaterTintShallow");

        private static readonly int UnderwaterTintDeepId =
            Shader.PropertyToID("_UnderwaterTintDeep");

        private static readonly int DistortionScaleId =
            Shader.PropertyToID("_DistortionScale");

        private static readonly int DistortionSpeedId =
            Shader.PropertyToID("_DistortionSpeed");

        private static readonly int DistortionStrengthId =
            Shader.PropertyToID("_DistortionStrength");

        private static readonly int BlurAmountShallowId =
            Shader.PropertyToID("_BlurAmountShallow");

        private static readonly int BlurAmountDeepId =
            Shader.PropertyToID("_BlurAmountDeep");

        private static readonly int LightShaftColorId =
            Shader.PropertyToID("_LightShaftColor");

        private static readonly int LightShaftScaleId =
            Shader.PropertyToID("_LightShaftScale");

        private static readonly int LightShaftPowerId =
            Shader.PropertyToID("_LightShaftPower");

        private static readonly int LightShaftTiltId =
            Shader.PropertyToID("_LightShaftTilt");

        private static readonly int LightShaftDepthId =
            Shader.PropertyToID("_LightShaftDepth");

        private static readonly int LightShaftSpeedId =
            Shader.PropertyToID("_LightShaftSpeed");

        /*
         * Estas propiedades están pensadas para una futura variante
         * del shader específicamente adaptada a cascadas.
         *
         * Si el shader no las contiene, simplemente se ignoran.
         */
        private static readonly int FlowSpeedId =
            Shader.PropertyToID("_FlowSpeed");

        private static readonly int FlowScaleId =
            Shader.PropertyToID("_FlowScale");

        private static readonly int EdgeFadeId =
            Shader.PropertyToID("_EdgeFade");

        private static readonly int VerticalDarkeningId =
            Shader.PropertyToID("_VerticalDarkening");

        private static readonly int TimeOffsetId =
            Shader.PropertyToID("_TimeOffset");
        
        private static readonly int SurfaceThicknessUpperId =
            Shader.PropertyToID("_SurfaceThicknessUpper");

        private static readonly int SurfaceThicknessLowerId =
            Shader.PropertyToID("_SurfaceThicknessLower");

        private static readonly int SurfaceDistortionMulId =
            Shader.PropertyToID("_SurfaceDistortionMul");

        private static readonly int AmbientWaveMulId =
            Shader.PropertyToID("_AmbientWaveMul");

        [Header("Basic")]

        [SerializeField]
        [Min(0.03125f)]
        private float width = 4f;

        [SerializeField]
        [Min(0.03125f)]
        private float height = 8f;

        [Tooltip("Número de columnas de vértices por unidad.")]
        [SerializeField]
        [Range(1, 16)]
        private int verticesPerUnitX = 4;

        [Tooltip("Número de filas de vértices por unidad.")]
        [SerializeField]
        [Range(1, 16)]
        private int verticesPerUnitY = 4;

        public enum WaterfallOrigin
        {
            Top,
            Center,
            Bottom
        }

        [Tooltip("Punto local que representa la posición del Transform.")]
        [SerializeField]
        private WaterfallOrigin origin = WaterfallOrigin.Top;

        [Header("Interaction")]

        [SerializeField]
        private LayerMask interactionLayerMask;

        [SerializeField]
        private LayerMask interactionTriggerLayerMask;

        [Header("Rendering")]

        [Tooltip(
            "Material opcional. Lo ideal es asignar una copia del material de agua " +
            "que utilice un shader adaptado a flujo vertical."
        )]
        [SerializeField]
        private Material waterfallMaterial;

        [SerializeField]
        private bool createMaterialInstance = true;

        [SerializeField]
        private Color waterColorShallow =
            new Color(0.56f, 0.68f, 0.19f, 0.28f);

        [SerializeField]
        private Color waterColorDeep =
            new Color(0.18f, 0.29f, 0.03f, 0.62f);

        [SerializeField]
        private Color underwaterTintShallow =
            new Color(0.82f, 1f, 0.55f, 1f);

        [SerializeField]
        private Color underwaterTintDeep =
            new Color(0.33f, 0.48f, 0.08f, 1f);

        [Header("Flow")]

        [SerializeField]
        [Min(0f)]
        private float flowSpeed = 1.25f;

        [SerializeField]
        [Min(0.01f)]
        private float flowScale = 1.5f;

        [SerializeField]
        [Range(0f, 0.5f)]
        private float edgeFade = 0.12f;

        [SerializeField]
        [Range(0f, 1f)]
        private float verticalDarkening = 0.2f;

        [Header("Distortion")]

        [SerializeField]
        private bool distortionEnabled = true;

        [SerializeField]
        private float distortionSpeed = 0.5f;

        [SerializeField]
        private float distortionScale = 1f;

        [SerializeField]
        private float distortionStrength = 2f;

        [Header("Blur")]

        [SerializeField]
        private bool blurEnabled = true;

        [SerializeField]
        [Min(0f)]
        private float blurAmountTop = 4f;

        [SerializeField]
        [Min(0f)]
        private float blurAmountBottom = 12f;

        [Header("Light Shafts")]

        [SerializeField]
        private bool lightShaftEnabled = true;

        [SerializeField]
        private Color lightShaftColor =
            new Color(0.16f, 0.24f, 0.13f, 0.25f);

        [SerializeField]
        private float lightShaftScale = 1.4f;

        [SerializeField]
        private float lightShaftPower = 1.25f;

        [SerializeField]
        private float lightShaftTilt = 0f;

        [SerializeField]
        private float lightShaftDepth = 2f;

        [SerializeField]
        private float lightShaftSpeed = 1f;

        [Header("Mesh Movement")]

        [Tooltip(
            "Movimiento lateral general de la cortina. " +
            "Debe mantenerse bajo para no deformar demasiado el collider."
        )]
        [SerializeField]
        [Min(0f)]
        private float horizontalWobble = 0.04f;

        [SerializeField]
        [Min(0f)]
        private float horizontalWobbleSpeed = 1.2f;

        [SerializeField]
        [Min(0.01f)]
        private float horizontalWobbleFrequency = 1.4f;

        [Tooltip(
            "Movimiento adicional aplicado únicamente cerca de los bordes."
        )]
        [SerializeField]
        [Min(0f)]
        private float edgeWobble = 0.06f;

        [SerializeField]
        [Min(0f)]
        private float edgeWobbleSpeed = 1.7f;

        [Tooltip(
            "Reducción de anchura en la parte inferior. " +
            "0 mantiene la misma anchura y 0.5 reduce la base a la mitad."
        )]
        [SerializeField]
        [Range(-0.5f, 0.9f)]
        private float bottomTaper = 0.08f;

        [Header("Physics")]

        [SerializeField]
        private bool applyDownwardForce = true;

        [SerializeField]
        [Min(0f)]
        private float downwardForce = 24f;

        [Tooltip(
            "Reduce únicamente la velocidad horizontal para evitar que la " +
            "cascada frene excesivamente la caída."
        )]
        [SerializeField]
        [Min(0f)]
        private float horizontalDrag = 6f;

        [SerializeField]
        private float constantHorizontalForce;

        [SerializeField]
        private bool limitMaximumFallSpeed = true;

        [SerializeField]
        [Min(0f)]
        private float maximumFallSpeed = 15f;

        [Header("Entry FX")]

        [SerializeField]
        private ParticleSystem entryParticleSystem;

        [SerializeField]
        [Min(0)]
        private int entryParticleCount = 8;

        [SerializeField]
        private ParticleSystem exitParticleSystem;

        [SerializeField]
        [Min(0)]
        private int exitParticleCount = 4;

        [Header("Continuous Particles")]

        [SerializeField]
        private List<ParticleConfig> waterfallParticleConfigs = new();

        [Header("Impact")]

        [Tooltip("Agua horizontal sobre la que cae la cascada.")]
        [SerializeField]
        private PixelWater targetWater;

        [Tooltip(
            "Punto donde la cascada impacta contra el agua. " +
            "Si está vacío se usa el centro inferior de la cascada."
        )]
        [SerializeField]
        private Transform impactPoint;

        [SerializeField]
        private bool generateImpactWaves = true;

        [SerializeField]
        [Min(0.02f)]
        private float impactWaveInterval = 0.12f;

        [SerializeField]
        [Min(0f)]
        private float impactWaveRadius = 0.45f;

        [SerializeField]
        private float impactWaveStrength = -0.1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float impactHorizontalRandomness = 0.4f;

        [SerializeField]
        private bool generateImpactSplashes = true;

        [SerializeField]
        [Min(0.05f)]
        private float impactSplashInterval = 0.75f;

        [SerializeField]
        [Min(0f)]
        private float impactSplashSize = 0.4f;

        [SerializeField]
        [Min(0f)]
        private float impactSplashSpeed = 4f;

        [SerializeField]
        private ParticleSystem continuousImpactParticles;

        [Header("Sorting")]

        [SerializeField]
        private string sortingLayerName = "Water";

        [SerializeField]
        private int orderInLayer;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private BoxCollider2D boxCollider;

        private Material runtimeMaterial;

        private Vector3[] baseVertices;
        private Vector3[] animatedVertices;

        private int vertexCountX;
        private int vertexCountY;

        private float impactWaveTimer;
        private float impactSplashTimer;
        private float timeOffset;

        private string GeneratedMeshName =>
            $"[Waterfall Mesh] {GetInstanceID()}";

        public float Width => width;
        public float Height => height;

        public Vector3 TopCenterWorld =>
            transform.TransformPoint(GetLocalTopCenter());

        public Vector3 BottomCenterWorld =>
            transform.TransformPoint(GetLocalBottomCenter());

        private Vector3 GetLocalTopCenter()
        {
            return origin switch
            {
                WaterfallOrigin.Top => Vector3.zero,
                WaterfallOrigin.Center => new Vector3(0f, height * 0.5f, 0f),
                WaterfallOrigin.Bottom => new Vector3(0f, height, 0f),
                _ => Vector3.zero
            };
        }

        private Vector3 GetLocalBottomCenter()
        {
            return origin switch
            {
                WaterfallOrigin.Top => new Vector3(0f, -height, 0f),
                WaterfallOrigin.Center => new Vector3(0f, -height * 0.5f, 0f),
                WaterfallOrigin.Bottom => Vector3.zero,
                _ => new Vector3(0f, -height, 0f)
            };
        }

        private float GetLocalYFromNormalizedDepth(float normalizedY)
        {
            return origin switch
            {
                WaterfallOrigin.Top => -normalizedY * height,
                WaterfallOrigin.Center => Mathf.Lerp(height * 0.5f, -height * 0.5f, normalizedY),
                WaterfallOrigin.Bottom => (1f - normalizedY) * height,
                _ => -normalizedY * height
            };
        }

        private Vector2 GetColliderOffset()
        {
            return origin switch
            {
                WaterfallOrigin.Top => new Vector2(0f, -height * 0.5f),
                WaterfallOrigin.Center => Vector2.zero,
                WaterfallOrigin.Bottom => new Vector2(0f, height * 0.5f),
                _ => new Vector2(0f, -height * 0.5f)
            };
        }

        private void Awake()
        {
            CacheComponents();
            CreateOrAssignMaterial();
            Refresh();

            timeOffset = UnityEngine.Random.Range(0f, 1000f);
            ResetImpactTimers();
        }
        
        public void SetHeight(float newHeight)
        {
            height = Mathf.Max(0.03125f, newHeight);

            Refresh();
        }

        private void OnEnable()
        {
            CacheComponents();
            CreateOrAssignMaterial();
            Refresh();

            if (Application.isPlaying)
            {
                if (continuousImpactParticles)
                    continuousImpactParticles.Play();

                SetContinuousParticlesEnabled(true);
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Reset()
        {
            CacheComponents();

            interactionLayerMask = LayerMask.GetMask("Player");
            interactionTriggerLayerMask = 0;

            Refresh();
        }

        private void OnValidate()
        {
            width = Mathf.Max(0.03125f, width);
            height = Mathf.Max(0.03125f, height);

            verticesPerUnitX = Mathf.Max(1, verticesPerUnitX);
            verticesPerUnitY = Mathf.Max(1, verticesPerUnitY);

            CacheComponents();

            if (!Application.isPlaying)
                CreateOrAssignMaterial();

            Refresh();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                SetContinuousParticlesEnabled(false);
        }

        private void OnDestroy()
        {
            DestroyGeneratedMesh();

            if (runtimeMaterial)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(runtimeMaterial);
                else
#endif
                    Destroy(runtimeMaterial);

                runtimeMaterial = null;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            AnimateMesh();
            UpdateShaderTime();
            UpdateImpact();
        }

        private void CacheComponents()
        {
            if (!meshFilter)
                meshFilter = GetComponent<MeshFilter>();

            if (!meshRenderer)
                meshRenderer = GetComponent<MeshRenderer>();

            if (!boxCollider)
                boxCollider = GetComponent<BoxCollider2D>();

            if (boxCollider)
                boxCollider.isTrigger = true;
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            CacheComponents();
            CreateOrAssignMaterial();
            GenerateMesh();
            ResetCollider();
            UpdateMaterial();
            UpdateSorting();
            UpdateParticleSystems();
        }

        private void CreateOrAssignMaterial()
        {
            if (!meshRenderer)
                return;

            if (!waterfallMaterial)
            {
                Shader shader = Shader.Find(
                    "Cainos/Interactive Pixel Water/Pixel Waterfall"
                );

                if (!shader)
                {
                    /*
                     * Fallback temporal: permite comprobar la malla y la física
                     * aunque todavía no exista el shader de cascada.
                     */
                    shader = Shader.Find(
                        "Cainos/Interactive Pixel Water/Pixel Water"
                    );
                }

                if (!shader)
                {
                    Debug.LogWarning(
                        "PixelWaterfall: no se encontró el shader de cascada " +
                        "ni el shader Pixel Water original.",
                        this
                    );

                    return;
                }

                if (!runtimeMaterial || runtimeMaterial.shader != shader)
                {
                    DestroyRuntimeMaterial();

                    runtimeMaterial = new Material(shader)
                    {
                        name = $"[Waterfall Material] {GetInstanceID()}",
                        renderQueue = 3000
                    };
                }

                meshRenderer.sharedMaterial = runtimeMaterial;
                return;
            }

            if (createMaterialInstance)
            {
                if (!runtimeMaterial ||
                    runtimeMaterial.shader != waterfallMaterial.shader)
                {
                    DestroyRuntimeMaterial();

                    runtimeMaterial = new Material(waterfallMaterial)
                    {
                        name = $"[Waterfall Material] {GetInstanceID()}"
                    };
                }

                meshRenderer.sharedMaterial = runtimeMaterial;
            }
            else
            {
                DestroyRuntimeMaterial();
                meshRenderer.sharedMaterial = waterfallMaterial;
            }
        }

        private void DestroyRuntimeMaterial()
        {
            if (!runtimeMaterial)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(runtimeMaterial);
            else
#endif
                Destroy(runtimeMaterial);

            runtimeMaterial = null;
        }

        private Material ActiveMaterial
        {
            get
            {
                if (runtimeMaterial)
                    return runtimeMaterial;

                if (meshRenderer)
                    return meshRenderer.sharedMaterial;

                return null;
            }
        }

        private void GenerateMesh()
        {
            if (!meshFilter)
                return;

            vertexCountX = Mathf.Max(
                MIN_VERTEX_COUNT_X,
                Mathf.CeilToInt(width * verticesPerUnitX) + 1
            );

            vertexCountY = Mathf.Max(
                MIN_VERTEX_COUNT_Y,
                Mathf.CeilToInt(height * verticesPerUnitY) + 1
            );

            int totalVertexCount = vertexCountX * vertexCountY;

            baseVertices = new Vector3[totalVertexCount];
            animatedVertices = new Vector3[totalVertexCount];

            Vector2[] uv = new Vector2[totalVertexCount];
            Vector2[] uv2 = new Vector2[totalVertexCount];

            for (int y = 0; y < vertexCountY; y++)
            {
                float normalizedY = y / (float)(vertexCountY - 1);

                float localY = GetLocalYFromNormalizedDepth(normalizedY);

                float taperMultiplier =
                    1f - bottomTaper * normalizedY;

                for (int x = 0; x < vertexCountX; x++)
                {
                    float normalizedX = x / (float)(vertexCountX - 1);

                    float localX =
                        Mathf.Lerp(-width * 0.5f, width * 0.5f, normalizedX);

                    localX *= taperMultiplier;

                    int index = GetVertexIndex(x, y);

                    baseVertices[index] = new Vector3(
                        localX,
                        localY,
                        0f
                    );

                    animatedVertices[index] = baseVertices[index];

                    // El shader original espera la profundidad en unidades,
                    // no normalizada entre 0 y 1.
                    float depth = normalizedY * height;

                    // Distancia al borde lateral más cercano.
                    // Replica aproximadamente el formato usado por PixelWater.
                    float distanceFromLeft = normalizedX * width;
                    float distanceFromRight = (1f - normalizedX) * width;
                    float edgeDistance = Mathf.Min(
                        1f,
                        Mathf.Min(distanceFromLeft, distanceFromRight)
                    );

                    uv[index] = new Vector2(
                        edgeDistance,
                        depth
                    );

                    // Replica la estructura de UV2 usada por PixelWater.
                    uv2[index] = new Vector2(
                        edgeDistance / width,
                        depth / height
                    );
                }
            }

            int quadCount =
                (vertexCountX - 1) * (vertexCountY - 1);

            int[] triangles = new int[quadCount * 6];
            int triangleIndex = 0;

            for (int y = 0; y < vertexCountY - 1; y++)
            {
                for (int x = 0; x < vertexCountX - 1; x++)
                {
                    int topLeft = GetVertexIndex(x, y);
                    int topRight = GetVertexIndex(x + 1, y);
                    int bottomLeft = GetVertexIndex(x, y + 1);
                    int bottomRight = GetVertexIndex(x + 1, y + 1);

                    // Primer triángulo
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;

                    // Segundo triángulo
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = bottomLeft;
                }
            }

            DestroyGeneratedMesh();

            mesh = new Mesh
            {
                name = GeneratedMeshName,
                vertices = animatedVertices,
                triangles = triangles,
                uv = uv,
                uv2 = uv2
            };

            mesh.MarkDynamic();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.sharedMesh = mesh;
        }

        private int GetVertexIndex(int x, int y)
        {
            return y * vertexCountX + x;
        }

        private void AnimateMesh()
        {
            if (!mesh ||
                baseVertices == null ||
                animatedVertices == null)
            {
                return;
            }

            float currentTime = Time.time + timeOffset;

            for (int y = 0; y < vertexCountY; y++)
            {
                float normalizedY = y / (float)(vertexCountY - 1);

                /*
                 * La parte superior se mantiene más estable para que la
                 * cascada permanezca unida visualmente a su salida.
                 */
                float verticalInfluence =
                    Mathf.SmoothStep(0f, 1f, normalizedY);

                for (int x = 0; x < vertexCountX; x++)
                {
                    float normalizedX = x / (float)(vertexCountX - 1);

                    int index = GetVertexIndex(x, y);
                    Vector3 vertex = baseVertices[index];

                    float generalWave = Mathf.Sin(
                        currentTime * horizontalWobbleSpeed +
                        normalizedY * horizontalWobbleFrequency * Mathf.PI * 2f
                    );

                    /*
                     * 0 en el centro y 1 en los bordes.
                     */
                    float edgeInfluence =
                        Mathf.Abs(normalizedX * 2f - 1f);

                    edgeInfluence = Mathf.SmoothStep(
                        0.35f,
                        1f,
                        edgeInfluence
                    );

                    float edgeWave = Mathf.Sin(
                        currentTime * edgeWobbleSpeed +
                        normalizedY * 8f +
                        normalizedX * 3f
                    );

                    float xOffset =
                        generalWave *
                        horizontalWobble *
                        verticalInfluence;

                    xOffset +=
                        edgeWave *
                        edgeWobble *
                        edgeInfluence *
                        verticalInfluence;

                    vertex.x += xOffset;

                    animatedVertices[index] = vertex;
                }
            }

            mesh.vertices = animatedVertices;
            mesh.RecalculateBounds();
        }

        private void ResetCollider()
        {
            if (!boxCollider)
                return;

            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(width, height);
            boxCollider.offset = GetColliderOffset();
        }

        private void UpdateMaterial()
        {
            Material material = ActiveMaterial;

            if (!material)
                return;

            SetColorIfPresent(
                material,
                WaterColorShallowId,
                waterColorShallow
            );

            SetColorIfPresent(
                material,
                WaterColorDeepId,
                waterColorDeep
            );

            SetColorIfPresent(
                material,
                UnderwaterTintShallowId,
                underwaterTintShallow
            );

            SetColorIfPresent(
                material,
                UnderwaterTintDeepId,
                underwaterTintDeep
            );

            SetFloatIfPresent(
                material,
                DistortionScaleId,
                distortionScale
            );

            SetFloatIfPresent(
                material,
                DistortionSpeedId,
                distortionSpeed
            );

            SetFloatIfPresent(
                material,
                DistortionStrengthId,
                distortionEnabled ? distortionStrength : 0f
            );

            SetFloatIfPresent(
                material,
                BlurAmountShallowId,
                blurEnabled ? blurAmountTop : 0f
            );

            SetFloatIfPresent(
                material,
                BlurAmountDeepId,
                blurEnabled ? blurAmountBottom : 0f
            );

            SetColorIfPresent(
                material,
                LightShaftColorId,
                lightShaftEnabled ? lightShaftColor : Color.clear
            );

            SetFloatIfPresent(
                material,
                LightShaftScaleId,
                lightShaftScale
            );

            SetFloatIfPresent(
                material,
                LightShaftPowerId,
                lightShaftPower
            );

            SetFloatIfPresent(
                material,
                LightShaftTiltId,
                lightShaftTilt
            );

            SetFloatIfPresent(
                material,
                LightShaftDepthId,
                lightShaftDepth
            );

            SetFloatIfPresent(
                material,
                LightShaftSpeedId,
                lightShaftSpeed
            );

            SetFloatIfPresent(
                material,
                FlowSpeedId,
                flowSpeed
            );

            SetFloatIfPresent(
                material,
                FlowScaleId,
                flowScale
            );

            SetFloatIfPresent(
                material,
                EdgeFadeId,
                edgeFade
            );

            SetFloatIfPresent(
                material,
                VerticalDarkeningId,
                verticalDarkening
            );
            
            // El shader original representa una masa horizontal.
            // En una cascada no queremos su línea de superficie superior.
            SetFloatIfPresent(
                material,
                SurfaceThicknessUpperId,
                0f
            );

            SetFloatIfPresent(
                material,
                SurfaceThicknessLowerId,
                0f
            );

            SetFloatIfPresent(
                material,
                SurfaceDistortionMulId,
                0f
            );

            // Las ondas ambientales están pensadas para la superficie horizontal.
            SetFloatIfPresent(
                material,
                AmbientWaveMulId,
                0f
            );
        }

        private void UpdateShaderTime()
        {
            Material material = ActiveMaterial;

            if (!material ||
                !material.HasProperty(TimeOffsetId))
            {
                return;
            }

            material.SetFloat(
                TimeOffsetId,
                Time.time + timeOffset
            );
        }

        private static void SetFloatIfPresent(
            Material material,
            int propertyId,
            float value
        )
        {
            if (material.HasProperty(propertyId))
                material.SetFloat(propertyId, value);
        }

        private static void SetColorIfPresent(
            Material material,
            int propertyId,
            Color value
        )
        {
            if (material.HasProperty(propertyId))
                material.SetColor(propertyId, value);
        }

        private void UpdateSorting()
        {
            if (!meshRenderer)
                return;

            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = orderInLayer;
        }

        private void UpdateParticleSystems()
        {
            if (waterfallParticleConfigs != null)
            {
                foreach (ParticleConfig config in waterfallParticleConfigs)
                {
                    if (config == null || !config.particleSystem)
                        continue;

                    ParticleSystem particleSystem =
                        config.particleSystem;

                    ParticleSystem.EmissionModule emission =
                        particleSystem.emission;

                    emission.enabled = config.enabled;

                    emission.rateOverTimeMultiplier =
                        width *
                        height *
                        config.emissionPerSquareUnit;

                    ParticleSystem.ShapeModule shape =
                        particleSystem.shape;

                    shape.shapeType =
                        ParticleSystemShapeType.Rectangle;

                    shape.scale = new Vector3(
                        width,
                        height,
                        0.05f
                    );

                    shape.position = GetColliderOffset();
                }
            }

            if (continuousImpactParticles)
            {
                continuousImpactParticles.transform.position =
                    GetImpactPosition();
            }
        }

        private void SetContinuousParticlesEnabled(bool enabled)
        {
            if (waterfallParticleConfigs == null)
                return;

            foreach (ParticleConfig config in waterfallParticleConfigs)
            {
                if (config == null || !config.particleSystem)
                    continue;

                if (enabled && config.enabled)
                    config.particleSystem.Play();
                else
                    config.particleSystem.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmitting
                    );
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract(other))
                return;

            if (entryParticleSystem && entryParticleCount > 0)
            {
                entryParticleSystem.transform.position =
                    other.bounds.center;

                entryParticleSystem.Emit(entryParticleCount);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!CanInteract(other))
                return;

            Rigidbody2D rb = other.attachedRigidbody;

            if (!rb)
                return;

            if (applyDownwardForce)
            {
                rb.AddForce(
                    Vector2.down * downwardForce,
                    ForceMode2D.Force
                );
            }

            if (horizontalDrag > 0f)
            {
                float dragForce =
                    -rb.linearVelocity.x * horizontalDrag;

                rb.AddForce(
                    new Vector2(dragForce, 0f),
                    ForceMode2D.Force
                );
            }

            if (!Mathf.Approximately(constantHorizontalForce, 0f))
            {
                rb.AddForce(
                    Vector2.right * constantHorizontalForce,
                    ForceMode2D.Force
                );
            }

            if (limitMaximumFallSpeed &&
                rb.linearVelocity.y < -maximumFallSpeed)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.y = -maximumFallSpeed;
                rb.linearVelocity = velocity;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!CanInteract(other))
                return;

            if (exitParticleSystem && exitParticleCount > 0)
            {
                exitParticleSystem.transform.position =
                    other.bounds.center;

                exitParticleSystem.Emit(exitParticleCount);
            }
        }

        private bool CanInteract(Collider2D collider)
        {
            if (!collider)
                return false;

            int layer = collider.gameObject.layer;
            int layerBit = 1 << layer;

            if (collider.isTrigger)
            {
                return (
                    interactionTriggerLayerMask.value &
                    layerBit
                ) != 0;
            }

            return (
                interactionLayerMask.value &
                layerBit
            ) != 0;
        }

        private void UpdateImpact()
        {
            if (!targetWater)
                return;

            impactWaveTimer -= Time.deltaTime;
            impactSplashTimer -= Time.deltaTime;

            if (generateImpactWaves &&
                impactWaveTimer <= 0f)
            {
                GenerateImpactWave();
                impactWaveTimer = impactWaveInterval;
            }

            if (generateImpactSplashes &&
                impactSplashTimer <= 0f)
            {
                GenerateImpactSplash();
                impactSplashTimer = impactSplashInterval;
            }

            if (continuousImpactParticles)
            {
                continuousImpactParticles.transform.position =
                    GetImpactPosition();
            }
        }

        private void GenerateImpactWave()
        {
            Vector3 position = GetRandomImpactPosition();

            float randomStrength =
                impactWaveStrength *
                UnityEngine.Random.Range(0.8f, 1.2f);

            float randomRadius =
                impactWaveRadius *
                UnityEngine.Random.Range(0.85f, 1.15f);

            targetWater.AddWave(
                position,
                randomRadius,
                randomStrength
            );
        }

        private void GenerateImpactSplash()
        {
            Vector3 position = GetRandomImpactPosition();

            float randomSize =
                impactSplashSize *
                UnityEngine.Random.Range(0.85f, 1.15f);

            float randomSpeed =
                impactSplashSpeed *
                UnityEngine.Random.Range(0.85f, 1.2f);

            targetWater.AddSplash(
                position,
                randomSize,
                randomSpeed
            );
        }

        private Vector3 GetImpactPosition()
        {
            if (impactPoint)
                return impactPoint.position;

            return BottomCenterWorld;
        }

        private Vector3 GetRandomImpactPosition()
        {
            Vector3 position = GetImpactPosition();

            float horizontalRange =
                width *
                0.5f *
                impactHorizontalRandomness;

            Vector3 localOffset = new Vector3(
                UnityEngine.Random.Range(
                    -horizontalRange,
                    horizontalRange
                ),
                0f,
                0f
            );

            return position +
                   transform.TransformVector(localOffset);
        }

        private void ResetImpactTimers()
        {
            impactWaveTimer =
                UnityEngine.Random.Range(
                    0f,
                    Mathf.Max(0.02f, impactWaveInterval)
                );

            impactSplashTimer =
                UnityEngine.Random.Range(
                    0f,
                    Mathf.Max(0.05f, impactSplashInterval)
                );
        }

        private void DestroyGeneratedMesh()
        {
            Mesh meshToDestroy = mesh;

            if (!meshToDestroy)
            {
                mesh = null;
                return;
            }

            if (meshToDestroy.name != GeneratedMeshName)
            {
                mesh = null;
                return;
            }

            if (meshFilter &&
                meshFilter.sharedMesh == meshToDestroy)
            {
                meshFilter.sharedMesh = null;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!AssetDatabase.Contains(meshToDestroy))
                    DestroyImmediate(meshToDestroy);

                mesh = null;
                return;
            }
#endif

            Destroy(meshToDestroy);
            mesh = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector2 centerOffset = GetColliderOffset();
            Vector3 center = new Vector3(centerOffset.x, centerOffset.y, 0f);

            Gizmos.DrawWireCube(
                center,
                new Vector3(width, height, 0f)
            );

            Gizmos.matrix = Matrix4x4.identity;

            Vector3 impactPosition = GetImpactPosition();

            Gizmos.DrawWireSphere(
                impactPosition,
                impactWaveRadius
            );
        }

        [Serializable]
        public class ParticleConfig
        {
            public ParticleSystem particleSystem;

            public bool enabled = true;

            [Min(0f)]
            public float emissionPerSquareUnit = 1f;
        }
    }
}