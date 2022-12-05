Shader "Hidden/SelectionShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 0.5)
        _Size("Size", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            float4 _Color;
            float4 _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float pos = (i.worldPos.x + i.worldPos.y + i.worldPos.z) / 2;
                float value = floor(frac(pos) + 0.5) * 0.5 + 0.25;
                float3 vertexReal = i.uv * _Size.xyz;
                float borderSize = 0.125;
                int borders =   (vertexReal.x < borderSize ? 1 : 0) +
                                (vertexReal.y < borderSize ? 1 : 0) + 
                                (vertexReal.z < borderSize ? 1 : 0) + 
                                (vertexReal.x > _Size.x - borderSize ? 1 : 0) +
                                (vertexReal.y > _Size.y - borderSize ? 1 : 0) + 
                                (vertexReal.z > _Size.z - borderSize ? 1 : 0);
                if (borders > 1)
                {
                    return float4(1 - _Color.rgb, _Color.a);
                }
                else return float4(1, 1, 1, value) * _Color;
            }
            ENDCG
        }
    }
}
