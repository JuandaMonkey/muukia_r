Shader "Universal Render Pipeline/Custom/PS2Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0.55, 0.12, 0.12, 1.0)
        _OutlineWidth("Outline Width", Range(0.001, 0.05)) = 0.015
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry+10" 
        }

        Pass
        {
            Name "OutlinePass"
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 normal = normalize(input.normalOS);
                float3 extrudedPos = input.positionOS.xyz + normal * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(extrudedPos);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, _OutlineColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
