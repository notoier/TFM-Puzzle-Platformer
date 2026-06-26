using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private SpriteRenderer lightSourceSpriteRenderer;
    [SerializeField] private SpriteRenderer lightFrameSpriteRenderer;


    [Header("Color")] 
    [SerializeField] private Color frameColor;
    [SerializeField] private Color lightColor;
    [SerializeField, Range(0,1)] private float lightColorAlpha = 1.0f;
    [SerializeField, Range(0,1)] private float spriteColorAlpha = 1.0f;

    [Header("Edge Settings")]
    [SerializeField] private Light2D edgeSpotLight2D;
    [SerializeField] private float edgeLightIntensity = 1.2f;
    [SerializeField] private float edgeLightRange = 11f;
    
    [Header("Shadow Settings")]
    [SerializeField] private Light2D shadowSpotLight2D;
    [SerializeField] private float shadowLightIntensity = 3.5f;
    [SerializeField] private float shadowLightRange = 13.5f;
    
    [SerializeField] private float shadowStrength = 1;
    [SerializeField] private float shadowFalloff = 0.25f;
    [SerializeField] private float shadowSoftness = 0.25f;
    
    [Header("Volumetric Settings")]
    [SerializeField] private Light2D volumetricSpotLight2D;
    [SerializeField] private float volumetriclightIntensity = 3.5f;
    [SerializeField] private float volumetricLightRange = 13.5f;
    
    [SerializeField] private float volumetricIntensity = 0.15f;

    private void Awake()
    {
        ConfigLight();
    }

    private void OnValidate()
    {
        ConfigLight();
    }
    
    private void ConfigLight()
    {
        Color lightBeamColor = new(lightColor.r, lightColor.g, lightColor.b, lightColorAlpha);
        Color lightSourceColor = new(lightColor.r, lightColor.g, lightColor.b, spriteColorAlpha);
        Color lightFrameColor = new(frameColor.r, frameColor.g, frameColor.b, spriteColorAlpha);
        
        if (edgeSpotLight2D)
        {
            edgeSpotLight2D.color = lightBeamColor;
            edgeSpotLight2D.intensity = edgeLightIntensity;
            edgeSpotLight2D.pointLightOuterRadius = edgeLightRange;

            shadowSpotLight2D.volumetricEnabled = false;
            shadowSpotLight2D.shadowsEnabled = false;
        }

        if (shadowSpotLight2D)
        {
            shadowSpotLight2D.color = lightBeamColor;
            shadowSpotLight2D.intensity = shadowLightIntensity;
            shadowSpotLight2D.pointLightOuterRadius = shadowLightRange;

            shadowSpotLight2D.shadowsEnabled = true;
            shadowSpotLight2D.volumetricEnabled = false;
            shadowSpotLight2D.shadowIntensity = shadowStrength;
            shadowSpotLight2D.shadowSoftness = shadowSoftness;
            shadowSpotLight2D.shadowSoftnessFalloffIntensity = shadowFalloff;
        }

        if (volumetricSpotLight2D)
        {
            volumetricSpotLight2D.color = lightBeamColor;
            volumetricSpotLight2D.intensity = volumetriclightIntensity;
            volumetricSpotLight2D.pointLightOuterRadius = volumetricLightRange;
            
            volumetricSpotLight2D.volumetricEnabled = true;
            volumetricSpotLight2D.shadowsEnabled = false;
            volumetricSpotLight2D.volumeIntensity = volumetricIntensity;
        }

        if (lightSourceSpriteRenderer)
        {
            lightSourceSpriteRenderer.color = lightSourceColor;
        }

        if (lightFrameSpriteRenderer)
        {
            lightFrameSpriteRenderer.color = frameColor;
        }
    }
}
