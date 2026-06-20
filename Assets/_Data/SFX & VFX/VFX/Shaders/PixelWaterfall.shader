Shader "Cainos/Interactive Pixel Water/Pixel Waterfall"
{
    Properties
    {
        _WaterColorShallow("Water Color Shallow", Color) = (0.56, 0.68, 0.19, 0.28)
        _WaterColorDeep("Water Color Deep", Color) = (0.18, 0.29, 0.03, 0.62)

        _UnderwaterTintShallow("Underwater Tint Shallow", Color) = (0.82, 1.0, 0.55, 1)
        _UnderwaterTintDeep("Underwater Tint Deep", Color) = (0.33, 0.48, 0.08, 1)

        _DistortionSpeed("Distortion Speed", Float) = 0.5
        _DistortionScale("Distortion Scale", Float) = 1
        _DistortionStrength("Distortion Strength", Float) = 0.5

        _BlurAmountShallow("Blur Amount Top", Float) = 2
        _BlurAmountDeep("Blur Amount Bottom", Float) = 6

        _LightShaftColor("Flow Highlight Color", Color) = (0.45, 0.62, 0.20, 0.18)
        _LightShaftScale("Flow Highlight Scale", Float) = 1.4
        _LightShaftPower("Flow Highlight Power", Float) = 2
        _LightShaftTilt("Flow Highlight Tilt", Float) = 0
        _LightShaftDepth("Flow Highlight Depth", Float) = 2
        _LightShaftSpeed("Flow Highlight Speed", Float) = 0.7

        _FlowSpeed("Flow Speed", Float) = 1.25
        _FlowScale("Flow Scale", Float) = 1.5
        _EdgeFade("Edge Fade", Range(0.001, 1)) = 0.12
        _VerticalDarkening("Vertical Darkening", Range(0, 1)) = 0.2
        _Opacity("Opacity", Range(0, 1)) = 0.9
        _TimeOffset("Time Offset", Float) = 0

        _StencilReference("Stencil Reference", Int) = 123
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 100

        Stencil
        {
            Ref [_StencilReference]
            Pass Replace
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "WaterfallUnlit"

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _CameraSortingLayerTexture;

            float4 _WaterColorShallow;
            float4 _WaterColorDeep;
            float4 _UnderwaterTintShallow;
            float4 _UnderwaterTintDeep;

            float _DistortionSpeed;
            float _DistortionScale;
            float _DistortionStrength;

            float _BlurAmountShallow;
            float _BlurAmountDeep;

            float4 _LightShaftColor;
            float _LightShaftScale;
            float _LightShaftPower;
            float _LightShaftTilt;
            float _LightShaftDepth;
            float _LightShaftSpeed;

            float _FlowSpeed;
            float _FlowScale;
            float _EdgeFade;
            float _VerticalDarkening;
            float _Opacity;
            float _TimeOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 mod289(float2 x)
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float3 mod289(float3 x)
            {
                return x - floor(x * (1.0 / 289.0)) * 289.0;
            }

            float3 permute(float3 x)
            {
                return mod289(((x * 34.0) + 1.0) * x);
            }

            float snoise(float2 v)
            {
                const float4 C = float4(
                    0.211324865405187,
                    0.366025403784439,
                    -0.577350269189626,
                    0.024390243902439
                );

                float2 i = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);

                float2 i1 = x0.x > x0.y
                    ? float2(1.0, 0.0)
                    : float2(0.0, 1.0);

                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;

                i = mod289(i);

                float3 p = permute(
                    permute(i.y + float3(0.0, i1.y, 1.0)) +
                    i.x + float3(0.0, i1.x, 1.0)
                );

                float3 m = max(
                    0.5 - float3(
                        dot(x0, x0),
                        dot(x12.xy, x12.xy),
                        dot(x12.zw, x12.zw)
                    ),
                    0.0
                );

                m = m * m;
                m = m * m;

                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;

                m *= 1.79284291400159 -
                     0.85373472095314 * (a0 * a0 + h * h);

                float3 g;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;

                return 130.0 * dot(m, g);
            }

            float4 SampleBlurredBackground(float2 uv, float blur)
            {
                float2 texel = 1.0 / _ScreenParams.xy;

                // Muestreo fijo de 9 puntos: bastante más barato y estable
                // que un kernel dinámico grande.
                float2 offset = texel * max(0.0, blur);

                float4 col = 0;
                col += tex2D(_CameraSortingLayerTexture, uv);
                col += tex2D(_CameraSortingLayerTexture, uv + float2( offset.x, 0));
                col += tex2D(_CameraSortingLayerTexture, uv + float2(-offset.x, 0));
                col += tex2D(_CameraSortingLayerTexture, uv + float2(0,  offset.y));
                col += tex2D(_CameraSortingLayerTexture, uv + float2(0, -offset.y));
                col += tex2D(_CameraSortingLayerTexture, uv + float2( offset.x,  offset.y));
                col += tex2D(_CameraSortingLayerTexture, uv + float2(-offset.x,  offset.y));
                col += tex2D(_CameraSortingLayerTexture, uv + float2( offset.x, -offset.y));
                col += tex2D(_CameraSortingLayerTexture, uv + float2(-offset.x, -offset.y));

                return col / 9.0;
            }

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = v.uv;
                o.uv2 = v.uv2;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // En el script de la cascada:
                // uv.x  = distancia al borde lateral más cercano.
                // uv.y  = profundidad en unidades.
                // uv2.y = profundidad normalizada de 0 a 1.
                float depth01 = saturate(i.uv2.y);
                float time = _Time.y + _TimeOffset;

                float safeScale = max(0.001, _FlowScale);
                float safeDistortionScale = max(0.001, _DistortionScale);

                // Ruido vertical descendente.
                float2 flowCoords = float2(
                    i.worldPos.x / safeScale,
                    (i.worldPos.y / safeScale) + time * _FlowSpeed
                );

                float flowNoiseA = snoise(flowCoords);
                float flowNoiseB = snoise(
                    flowCoords * float2(2.15, 0.65) +
                    float2(7.3, time * _DistortionSpeed)
                );

                // Distorsión principalmente horizontal, con una fracción vertical.
                float2 distortion = float2(
                    flowNoiseA,
                    flowNoiseB * 0.25
                );

                distortion *= 0.01 * _DistortionStrength /
                              safeDistortionScale;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                screenUV += distortion;

                float blur = lerp(
                    _BlurAmountShallow,
                    _BlurAmountDeep,
                    depth01
                );

                float4 background = SampleBlurredBackground(screenUV, blur);

                float4 tint = lerp(
                    _UnderwaterTintShallow,
                    _UnderwaterTintDeep,
                    depth01
                );

                float darkening = lerp(
                    1.0,
                    1.0 - _VerticalDarkening,
                    depth01
                );

                float4 waterColor = lerp(
                    _WaterColorShallow,
                    _WaterColorDeep,
                    depth01
                );

                float3 processedBackground =
                    background.rgb * tint.rgb * darkening;

                float3 finalRgb = lerp(
                    processedBackground,
                    waterColor.rgb,
                    saturate(waterColor.a)
                );

                // Vetas verticales claras desplazándose hacia abajo.
                float shaftScale = max(0.001, _LightShaftScale);
                float shaftNoise = snoise(float2(
                    (i.worldPos.x + i.worldPos.y * _LightShaftTilt) / shaftScale,
                    (i.worldPos.y / shaftScale) + time * _LightShaftSpeed
                ));

                shaftNoise = saturate(shaftNoise * 0.5 + 0.5);
                shaftNoise = pow(shaftNoise, max(0.01, _LightShaftPower));

                float shaftDepthFade = 1.0 - saturate(
                    i.uv.y / max(0.001, _LightShaftDepth)
                );

                finalRgb += _LightShaftColor.rgb *
                            _LightShaftColor.a *
                            shaftNoise *
                            shaftDepthFade;

                // uv.x vale 0 en ambos laterales y se aproxima a 1
                // en el interior, por lo que sirve directamente como máscara.
                float edgeMask = smoothstep(
                    0.0,
                    max(0.001, _EdgeFade),
                    i.uv.x
                );

                // Variación suave de opacidad para romper el bloque uniforme.
                float bodyVariation = lerp(
                    0.82,
                    1.0,
                    saturate(flowNoiseB * 0.5 + 0.5)
                );

                float alpha = edgeMask * _Opacity * bodyVariation;

                return half4(finalRgb, saturate(alpha));
            }
            ENDCG
        }
    }

    Fallback Off
}
