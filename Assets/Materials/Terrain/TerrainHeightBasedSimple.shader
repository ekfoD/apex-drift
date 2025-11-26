Shader "Custom/URP_TwoColorWavy"
{
    Properties
    {
        _BottomColor ("Bottom Color (Grass)", Color) = (0.3, 0.6, 0.2, 1)
        _TopColor ("Top Color (Stone)", Color) = (0.4, 0.4, 0.4, 1)
        
        _TransitionHeight ("Transition Height", Float) = 50
        _TransitionSmoothness ("Transition Smoothness", Range(0.1, 50)) = 10
        
        _WaveScale ("Wave Scale", Range(0.001, 1)) = 0.05
        _WaveStrength ("Wave Strength", Range(0, 50)) = 15
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
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _TransitionHeight;
                float _TransitionSmoothness;
                float _WaveScale;
                float _WaveStrength;
                float4 _BottomColor;
                float4 _TopColor;
            CBUFFER_END

            // Simple noise function for waves
            float noise(float2 pos)
            {
                float2 p = floor(pos);
                float2 f = frac(pos);
                f = f * f * (3.0 - 2.0 * f);
                
                float n = p.x + p.y * 157.0;
                float a = frac(sin(n + 0.0) * 43758.5453);
                float b = frac(sin(n + 1.0) * 43758.5453);
                float c = frac(sin(n + 157.0) * 43758.5453);
                float d = frac(sin(n + 158.0) * 43758.5453);
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.normalWS = normalInputs.normalWS;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Get world height
                float height = IN.positionWS.y;
                
                // Generate wave noise based on XZ position
                float wave = noise(IN.positionWS.xz * _WaveScale) * 2.0 - 1.0;
                
                // Add wave to height
                float wavyHeight = height + (wave * _WaveStrength);
                
                // Calculate blend between bottom and top color
                float blend = saturate((wavyHeight - _TransitionHeight) / _TransitionSmoothness);
                
                // Mix colors
                half4 color = lerp(_BottomColor, _TopColor, blend);
                
                // Simple lighting
                Light mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float NdotL = saturate(dot(normal, mainLight.direction));
                
                // Apply lighting with ambient
                half3 ambient = half3(0.4, 0.4, 0.45);
                color.rgb *= mainLight.color * NdotL + ambient;
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
