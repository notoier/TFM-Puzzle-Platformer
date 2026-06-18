Shader "Custom/FlowingWater"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Water Texture", 2D) = "white" {}
        _WaterSpeed ("Water Speed", Float) = 1
        _Length ("Length", Float) = 1
        _TilesPerUnit ("Tiles Per Unit", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        LOD 200

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;

        fixed4 _Color;
        float _Length;
        float _WaterSpeed;
        float _TilesPerUnit;

        struct Input
        {
            float2 uv_MainTex;
        };

        float2 TileAndOffset(float2 uv)
        {
            float repetitions = max(_Length * _TilesPerUnit, 0.01);

            uv.x *= repetitions;
            uv.x -= _Time.y * _WaterSpeed;

            return uv;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = TileAndOffset(IN.uv_MainTex);
            fixed4 color = tex2D(_MainTex, uv) * _Color;

            o.Albedo = color.rgb;
            o.Emission = color.rgb;
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = color.a;
        }

        ENDCG
    }

    FallBack "Transparent/Diffuse"
}