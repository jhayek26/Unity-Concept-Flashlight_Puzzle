//Include access to URP shader library functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//Attributes of the current mesh
struct Attributes {
	float3 positionOS : POSITION; //Object space position
	float3 normalOS : NORMAL; //Object space normal vector
};

//Data that is transferred from the vertex stage to the fragment stage
struct Interpolators {
	float4 positionCS : SV_POSITION; //Clip space position (the on-screen position of the vertex)
};

float3 _LightDirection;

//Slightly adjusts the shadow caster positions to prevent ugly artifacts produced by float point imprecision (aka "shadow acne")
float4 FixShadowCasterPositionCS(float3 positionWS, float3 normalWS)
{
	float3 lightDirectionWS = _LightDirection;
	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

	//Depending on the graphics API being used, the clip space z-axis may be reversed. This will correct the result.
	#if UNITY_REVERSED_Z
		positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
	#else
		positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
	#endif
	
	return positionCS;
}

Interpolators vert(Attributes IN)
{
	Interpolators OUT;

	OUT.positionCS = FixShadowCasterPositionCS(GetVertexPositionInputs(IN.positionOS).positionWS, GetVertexNormalInputs(IN.normalOS).normalWS);

	return OUT;
}

half4 frag(Interpolators IN) : SV_TARGET {
	return 0;
}