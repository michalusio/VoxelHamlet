Shader "Hidden/JobRenderShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back
        ZTest Always
        Pass
        {
            /*Stencil {
                Ref 1
                Comp Greater
                Pass replace
            }*/

            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float pos = (i.worldPos.x + i.worldPos.y + i.worldPos.z) / 2;
                float value = floor(frac(pos) + 0.5) * 0.5 + 0.25;
                float3 vertexReal = i.uv;
                float borderSize = 0.0625;
                int borders =   (vertexReal.x < borderSize ? 1 : 0) +
                                (vertexReal.y < borderSize ? 1 : 0) + 
                                (vertexReal.z < borderSize ? 1 : 0) + 
                                (vertexReal.x > 1 - borderSize ? 1 : 0) +
                                (vertexReal.y > 1 - borderSize ? 1 : 0) + 
                                (vertexReal.z > 1 - borderSize ? 1 : 0);
                float4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                if (borders > 1)
                {
                    //return float4(0, 0, 0, 1);
                }
                return float4(1, 1, 1, value) * col;
            }
            ENDCG
        }
    }
}
