// gradient night sky with twinkling stars. the night cycle script drives the colours
Shader "Skyguard/NightSky"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.03, 0.05, 0.16, 1)
        _HorizonColor ("Horizon", Color) = (0.28, 0.14, 0.24, 1)
        _GroundColor ("Below horizon", Color) = (0.02, 0.03, 0.06, 1)
        _StarStrength ("Stars", Range(0, 2)) = 1
        _StarDensity ("Star density", Float) = 280
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                half _StarStrength;
                float _StarDensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 d = normalize(input.direction);
                float up = d.y;

                half3 colour = lerp(_HorizonColor.rgb, _ZenithColor.rgb, saturate(pow(max(up, 0.0), 0.45)));
                if (up < 0.0)
                    colour = lerp(_HorizonColor.rgb, _GroundColor.rgb, saturate(-up * 4.0));

                // hash a grid laid over the view direction and only light the rare bright cells
                float seed = Hash(floor(d * _StarDensity));
                float star = step(0.9972, seed) * saturate(up * 3.0) * _StarStrength;
                float twinkle = 0.7 + 0.3 * sin(_Time.y * 2.0 + seed * 100.0);
                colour += star * twinkle;

                return half4(colour, 1.0);
            }
            ENDHLSL
        }
    }
}
