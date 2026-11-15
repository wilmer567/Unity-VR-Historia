Shader "ISTR/PortalFotoMezcla"
{
    Properties
    {
        _FotoReal ("Foto Real (Estatica)", 2D) = "white" {}
        _PortalTex ("Portal Camera (RenderTexture)", 2D) = "black" {}
        _Mezcla ("Mezcla Portal (0=Foto, 1=Portal)", Range(0,1)) = 0
        _BrilloEmision ("Brillo Emision", Range(0,3)) = 0.5
        _TinteFoto ("Tinte Foto", Color) = (1,1,1,1)
        _VignetteIntensidad ("Vignette Borde", Range(0,1)) = 0.3
        _Smoothness ("Smoothness Vidrio", Range(0,1)) = 0.85
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [Normal] _BumpMap ("Normal Map Marco", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0,2)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float fogCoord : TEXCOORD5;
            };

            TEXTURE2D(_FotoReal);       SAMPLER(sampler_FotoReal);
            TEXTURE2D(_PortalTex);      SAMPLER(sampler_PortalTex);
            TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float _Mezcla;
                float _BrilloEmision;
                float4 _TinteFoto;
                float _VignetteIntensidad;
                float _Smoothness;
                float _Metallic;
                float _BumpScale;
                float4 _FotoReal_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.tangentWS = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _FotoReal);
                output.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Muestrear ambas texturas
                half4 foto = SAMPLE_TEXTURE2D(_FotoReal, sampler_FotoReal, input.uv);
                half4 portal = SAMPLE_TEXTURE2D(_PortalTex, sampler_PortalTex, input.uv);

                // Aplicar tinte a la foto
                foto.rgb *= _TinteFoto.rgb;

                // Vignette en los bordes del cuadro
                float2 centrado = input.uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(centrado, centrado) * _VignetteIntensidad;
                vignette = saturate(vignette);

                // Mezcla foto <-> portal controlada por _Mezcla
                half3 colorMezclado = lerp(foto.rgb, portal.rgb, _Mezcla);
                colorMezclado *= vignette;

                // Normal map
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                float3x3 TBN = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, TBN));

                // Iluminacion basica URP
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.fogCoord = input.fogCoord;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = colorMezclado;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = colorMezclado * _BrilloEmision;
                surfaceData.alpha = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
