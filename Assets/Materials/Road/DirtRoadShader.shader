Shader "Custom/TMNFDirt"
{
    Properties
    {
        [Header(Dirt Colors)]
        _DirtColor ("Dirt Color", Color) = (0.35, 0.3, 0.25, 1)
        _DirtVariation ("Dirt Variation", Color) = (0.4, 0.35, 0.28, 1)
        _EdgeColor ("Edge Color (Darker)", Color) = (0.25, 0.22, 0.18, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.3)) = 0.15
        
        [Header(Tire Tracks)]
        _TireTrackColor ("Tire Track Color (Dark)", Color) = (0.2, 0.18, 0.15, 1)
        _TireTrackWidth ("Tire Track Width", Range(0, 0.2)) = 0.14
        _TireTrackOffset ("Tire Track Position", Range(0.15, 0.45)) = 0.28
        _TireTrackDepth ("Tire Track Depth", Range(0, 1)) = 0.85
        _TreadFrequency ("Tire Tread Frequency", Float) = 6.0
        
        [Header(Muddy Details)]
        _MudStrength ("Mud Variation", Range(0, 1)) = 0.4
        _MudScale ("Mud Scale", Float) = 15.0
        
        [Header(Lighting)]
        _AmbientLight ("Ambient Light", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DirtColor;
                float4 _DirtVariation;
                float4 _EdgeColor;
                float _EdgeWidth;
                
                float4 _TireTrackColor;
                float _TireTrackWidth;
                float _TireTrackOffset;
                float _TireTrackDepth;
                float _TreadFrequency;
                
                float _MudStrength;
                float _MudScale;
                float _AmbientLight;
            CBUFFER_END

            // Simple hash for noise
            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float widthPos = input.uv.x;
                float distanceAlongRoad = input.uv.y;
                
                // Base muddy dirt with variation
                float mudNoise = noise(input.uv * _MudScale);
                half4 baseColor = lerp(_DirtColor, _DirtVariation, mudNoise * _MudStrength);
                
                // Darker, rougher edges
                if (widthPos < _EdgeWidth)
                {
                    float edgeFade = smoothstep(0.0, 1.0, widthPos / _EdgeWidth);
                    baseColor = lerp(_EdgeColor, baseColor, edgeFade);
                }
                else if (widthPos > (1.0 - _EdgeWidth))
                {
                    float edgeFade = smoothstep(0.0, 1.0, (1.0 - widthPos) / _EdgeWidth);
                    baseColor = lerp(_EdgeColor, baseColor, edgeFade);
                }
                
                // LEFT TIRE TRACK - Deep and pronounced
                float leftTrackCenter = _TireTrackOffset;
                float leftTrackDist = abs(widthPos - leftTrackCenter);
                if (leftTrackDist < _TireTrackWidth * 0.5)
                {
                    float trackFade = 1.0 - (leftTrackDist / (_TireTrackWidth * 0.5));
                    trackFade = pow(trackFade, 1.5); // Sharper edges
                    
                    // Tire tread pattern (TMNF style - subtle grooves)
                    float treadPattern = sin(distanceAlongRoad * _TreadFrequency * 6.28318);
                    treadPattern = (treadPattern + 1.0) * 0.5;
                    treadPattern = smoothstep(0.4, 0.6, treadPattern);
                    
                    // Mix tread into track
                    float finalDepth = trackFade * _TireTrackDepth;
                    finalDepth = lerp(finalDepth, finalDepth * 0.8, treadPattern * 0.3);
                    
                    baseColor = lerp(baseColor, _TireTrackColor, finalDepth);
                }
                
                // RIGHT TIRE TRACK - Deep and pronounced
                float rightTrackCenter = 1.0 - _TireTrackOffset;
                float rightTrackDist = abs(widthPos - rightTrackCenter);
                if (rightTrackDist < _TireTrackWidth * 0.5)
                {
                    float trackFade = 1.0 - (rightTrackDist / (_TireTrackWidth * 0.5));
                    trackFade = pow(trackFade, 1.5); // Sharper edges
                    
                    // Tire tread pattern (slightly offset)
                    float treadPattern = sin((distanceAlongRoad + 0.3) * _TreadFrequency * 6.28318);
                    treadPattern = (treadPattern + 1.0) * 0.5;
                    treadPattern = smoothstep(0.4, 0.6, treadPattern);
                    
                    // Mix tread into track
                    float finalDepth = trackFade * _TireTrackDepth;
                    finalDepth = lerp(finalDepth, finalDepth * 0.8, treadPattern * 0.3);
                    
                    baseColor = lerp(baseColor, _TireTrackColor, finalDepth);
                }
                
                // Apply shadows
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half shadow = mainLight.shadowAttenuation;
                half lighting = max(shadow, _AmbientLight);
                
                return half4(baseColor.rgb * lighting, baseColor.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
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
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                Light mainLight = GetMainLight();
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, mainLight.direction));
                
                output.positionCS = positionCS;
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
