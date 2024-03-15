//Include access to URP shader library functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//Properties

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
float4 _MainTex_ST; //MainTex tiling and offset
TEXTURE2D(_ShadowTex); SAMPLER(sampler_ShadowTex);
float4 _ShadowTex_ST; //ShadowTex tiling and offset
float _Smoothness;

//Structs

//Attributes of the current mesh
struct Attributes {
	float3 positionOS : POSITION; //Object space position
	float3 normalOS : NORMAL; //Object space normal vector
	float2 uv : TEXCOORD0; //Texture uv coordinates
};

//Data that is transferred from the vertex stage to the fragment stage
struct Interpolators {
	float4 positionCS : SV_POSITION; //Clip space position (the on-screen position of the vertex)
	//noperspective float2 uv : TEXCOORD0; //Texture uv coordinates (affine texture mapping)
	float2 uv : TEXCOORD0; //Texture uv coordinates
	float3 positionWS : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
};

//Functions

Interpolators vert(Attributes IN)
{
	Interpolators OUT;

	VertexPositionInputs posIn = GetVertexPositionInputs(IN.positionOS);
	OUT.positionCS = posIn.positionCS;
	OUT.positionWS = posIn.positionWS;
	OUT.normalWS = GetVertexNormalInputs(IN.normalOS).normalWS;
	OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

	return OUT;
}

half4 frag(Interpolators IN) : SV_TARGET {

	half4 mainTexSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
	half4 shadowTexSample = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, IN.uv);

	InputData lightingIn = (InputData)0;
	lightingIn.positionWS = IN.positionWS;
	lightingIn.normalWS = normalize(IN.normalWS); //Normalize the world space normals for smoother specular highlights
	lightingIn.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
	lightingIn.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

	SurfaceData surfaceIn = (SurfaceData)0;
	surfaceIn.albedo = 1;
	surfaceIn.specular = 1;
	//surfaceIn.albedo = mainTexSample.rgb;
	surfaceIn.smoothness = _Smoothness;

	//return UniversalFragmentBlinnPhong(lightingIn, surfaceIn);

	half4 blinnPhong = UniversalFragmentBlinnPhong(lightingIn, surfaceIn);
	//Blend between the unlit shadow texture and the lit main texture using the luminance of the Blinn-Phong lighting
	half3 blendedTextures = lerp(shadowTexSample.rgb, mainTexSample.rgb * blinnPhong.rgb, clamp(dot(blinnPhong.rgb, half3(0.2126729, 0.7151522, 0.0721750)),0,1));

	return half4(blendedTextures,mainTexSample.a);
}