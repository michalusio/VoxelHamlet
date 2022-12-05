// Standard geometry shader example
// https://github.com/keijiro/StandardGeometryShader

#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"
#include "noiseSimplex.cginc"

// Cube map shadow caster; Used to render point light shadows on platforms
// that lacks depth cube map support.
#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
#define PASS_CUBE_SHADOWCASTER
#endif

// Shader uniforms
half _Glossiness;
half _Metallic;
float _Grid;
float4 _AddColor;

// Vertex input attributes
struct VertexInput
{
	float4 pos : POSITION;
};

// Vertex to Geometry input
struct Attributes
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 worldPosition : TEXCOORD0;
};

// Fragment varyings
struct Varyings
{
	float4 position : SV_POSITION;

#if defined(PASS_CUBE_SHADOWCASTER)
	// Cube map shadow caster pass
	float3 shadow : TEXCOORD0;

#elif defined(UNITY_PASS_SHADOWCASTER)
	// Default shadow caster pass

#else
	// GBuffer construction pass
	float3 normal : NORMAL;
	float4 tspace0 : TEXCOORD1;
	float4 tspace1 : TEXCOORD2;
	float4 tspace2 : TEXCOORD3;
	half3 ambient : TEXCOORD4;
	float3 worldPosition : TEXCOORD5;
#endif
};

//
// Vertex stage
//

Attributes Vertex(VertexInput input)
{
	Attributes o;
	// Only do object space to world space transform.
	o.position = mul(unity_ObjectToWorld, float4(input.pos.xyz, 1));
	o.worldPosition = o.position.xyz;
	o.normal = 0;
	o.tangent = float4(1, 0, 0, 1);
	return o;
}

//
// Geometry stage
//

Varyings VertexOutput(float3 pos, half3 wnrm, half4 wtan, float3 wpos)
{
	Varyings o;
#if defined(PASS_CUBE_SHADOWCASTER)
	// Cube map shadow caster pass: Transfer the shadow vector.
	o.position = UnityWorldToClipPos(float4(pos, 1));
	o.shadow = pos - _LightPositionRange.xyz;

#elif defined(UNITY_PASS_SHADOWCASTER)
	// Default shadow caster pass: Apply the shadow bias.
	float scos = dot(wnrm, normalize(UnityWorldSpaceLightDir(pos)));
	pos -= wnrm * unity_LightShadowBias.z * sqrt(1 - scos * scos);
	o.position = UnityApplyLinearShadowBias(UnityWorldToClipPos(float4(pos, 1)));

#else
	// GBuffer construction pass
	half3 bi = cross(wnrm, wtan) * wtan.w * unity_WorldTransformParams.w;
	o.position = UnityWorldToClipPos(float4(pos, 1));
	o.worldPosition = wpos;
	o.normal = wnrm;
	o.tspace0 = float4(wtan.x, bi.x, wnrm.x, pos.x);
	o.tspace1 = float4(wtan.y, bi.y, wnrm.y, pos.y);
	o.tspace2 = float4(wtan.z, bi.z, wnrm.z, pos.z);
	o.ambient = ShadeSHPerVertex(wnrm, 0);
#endif
	return o;
}

[maxvertexcount(3)]
void Geometry(
	triangle Attributes input[3],
	inout TriangleStream<Varyings> outStream
)
{
	float3 p0 = input[0].position.xyz;
	float3 p1 = input[1].position.xyz;
	float3 p2 = input[2].position.xyz;
	float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));
	input[0].normal = triangleNormal;
	input[1].normal = triangleNormal;
	input[2].normal = triangleNormal;

	outStream.Append(VertexOutput(input[0].position, input[0].normal, input[0].tangent, input[0].worldPosition));
	outStream.Append(VertexOutput(input[1].position, input[1].normal, input[1].tangent, input[1].worldPosition));
	outStream.Append(VertexOutput(input[2].position, input[2].normal, input[2].tangent, input[2].worldPosition));
}

//
// Fragment phase
//

#if defined(PASS_CUBE_SHADOWCASTER)

// Cube map shadow caster pass
half4 Fragment(Varyings input) : SV_Target
{
	float depth = length(input.shadow) + unity_LightShadowBias.x;
	return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

// Default shadow caster pass
half4 Fragment() : SV_Target{ return 0; }

#else

// GBuffer construction pass
void Fragment(
	Varyings input,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3
)
{

	// Tangent space conversion (tangent space normal -> world space normal)
	float3 wn = input.normal.xyz;

	float2 uv;
	if (abs(wn.x) > 0.1) uv = input.worldPosition.zy;
	else if (abs(wn.y) > 0.1) uv = input.worldPosition.xz;
	else if (abs(wn.z) > 0.1) uv = input.worldPosition.xy;

	// Sample textures
	half3 albedo;
	float y = input.worldPosition.y;
	if (y >= 55) albedo	= half3(0.1411764705882353, 0.5, 0);
	else if (y >= 48) albedo = half3(0.5, 0.4156862745098039, 0);
	else albedo = half3(0.4784313725490196, 0.4784313725490196, 0.4784313725490196);
	
	{
		float2 grid = abs(frac(uv - 0.5) - 0.5) / fwidth(uv);
		float l = min(1, min(grid.x, grid.y));
		if (_Grid > 0.5)
		{
			albedo *= (9 + l) / 10;
		}
	}
	albedo *= _AddColor.rgb * (1 + round(snoise(round(uv - 0.5)) * 2) / 25);

	// PBS workflow conversion (metallic -> specular)
	half3 c_diff, c_spec;
	half refl10;
	c_diff = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, // input
		c_spec, refl10     // output
	);

	// Update the GBuffer.
	UnityStandardData data;
	data.diffuseColor = c_diff;
	data.occlusion = 0;
	data.specularColor = c_spec;
	data.smoothness = _Glossiness;
	data.normalWorld = wn;
	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

	// Calculate ambient lighting and output to the emission buffer.
	float3 wp = float3(input.tspace0.w, input.tspace1.w, input.tspace2.w);
	half3 sh = ShadeSHPerPixel(data.normalWorld, input.ambient, wp);
	outEmission = half4(sh * c_diff, 1);
}

#endif