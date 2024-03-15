Shader "Unlit/SecretTextureShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        _ShadowTex ("Shadow Texture", 2D) = "black" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0
    }
    SubShader
    {
        Tags{"RenderPipeline" = "UniversalPipeline"}

        Pass {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM

            #define _SPECULAR_COLOR
            #define _ADDITIONAL_LIGHTS

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE //Compile variants of the shader with and without main light shadows (single underscore denotes no keyword for the first variant, hence no main light shadows)
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            //Add custom HLSL pass
            #include "SecretTextureForwardLitPass.hlsl"
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            //Add custom HLSL pass
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
