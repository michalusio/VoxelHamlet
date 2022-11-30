Shader "Custom/GridShaderEdit"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Size("Size", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _Size;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            IN.uv_MainTex *= _Size.xy;

            {
                float Border = 0.25;
                if (!(abs(IN.uv_MainTex.x - _Size.x/2) > _Size.x / 2 - Border || abs(IN.uv_MainTex.y - _Size.y / 2) > _Size.y / 2 - Border))
                {
                    float pos = (IN.uv_MainTex.x + IN.uv_MainTex.y) / 2;
                    float value = floor(frac(pos) + 0.5) * 0.5 + 0.25;
                    c.a *= value / 2;
                }
            }

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
