Shader "Unlit/TerrainShader"
{
	Properties
	{
		_TextureMapArray("Texture Map Array", 2DArray) = "" {}
		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_CameraPos("CameraPos", Vector) = (0, 0, 0, 0)
		_Grid("Grid Distance", Range(0, 1)) = 0.95
		_AddColor("Additional Color", Color) = (1, 1, 1, 1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }

			Pass
			{
				Tags { "LightMode" = "Deferred" }
				CGPROGRAM
				#pragma require geometry
				#pragma target 4.5
				#pragma vertex Vertex
				#pragma geometry Geometry
				#pragma fragment Fragment
				#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
				#pragma require 2darray
				#include "StandardGeometry.cginc"
				ENDCG
			}

			Pass
			{
				Tags { "LightMode" = "ShadowCaster" }
				CGPROGRAM
				#pragma require geometry
				#pragma target 4.5
				#pragma vertex Vertex
				#pragma geometry Geometry
				#pragma fragment Fragment
				#pragma multi_compile_shadowcaster noshadowmask nodynlightmap nodirlightmap nolightmap
				#pragma require 2darray
				#include "StandardGeometry.cginc"
				ENDCG
			}
		}
}
