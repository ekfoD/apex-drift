Shader "Custom/BasicRoadShader"
{
    Properties
    {
        _RoadColor ("Road Color", Color) = (0.2, 0.2, 0.2, 1)
        _LineColor ("Line Color", Color) = (1, 1, 1, 1)
        _DashLength ("Dash Length", Float) = 3.0
        _GapLength ("Gap Length", Float) = 2.0
        _LineWidth ("Line Width", Range(0, 0.5)) = 0.1
        _LineOffset ("Line Offset from Center", Range(-0.5, 0.5)) = 0
        
        [Header(F1 Kerbs)]
        _KerbWidth ("Kerb Width", Range(0, 1)) = 0.15
        _KerbColor1 ("Kerb Color 1", Color) = (1, 1, 1, 1)
        _KerbColor2 ("Kerb Color 2", Color) = (1, 0, 0, 1)
        _KerbSquareWidth ("Kerb Square Width", Float) = 0.1
        _KerbSquareLength ("Kerb Square Length", Float) = 2.0
        _EdgeLineWidth ("Edge Line Width", Range(0, 0.1)) = 0.02
        _EdgeLineColor ("Edge Line Color", Color) = (1, 1, 1, 1)
        
        [Header(Lighting)]
        _AmbientLight ("Ambient Light", Range(0, 1)) = 0.3
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
                float4 _RoadColor;
                float4 _LineColor;
                float _DashLength;
                float _GapLength;
                float _LineWidth;
                float _LineOffset;
                
                float _KerbWidth;
                float4 _KerbColor1;
                float4 _KerbColor2;
                float _KerbSquareWidth;
                float _KerbSquareLength;
                float _EdgeLineWidth;
                float4 _EdgeLineColor;
                float _AmbientLight;
            CBUFFER_END

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
                
                half4 baseColor;
                
                // LEFT KERB (checkered pattern)
                if (widthPos < _KerbWidth)
                {
                    float xSquare = floor(widthPos / _KerbSquareWidth);
                    float ySquare = floor(distanceAlongRoad / _KerbSquareLength);
                    float checker = fmod(xSquare + ySquare, 2.0);
                    baseColor = checker < 0.5 ? _KerbColor1 : _KerbColor2;
                }
                // RIGHT KERB (checkered pattern)
                else if (widthPos > (1.0 - _KerbWidth))
                {
                    float xSquare = floor((1.0 - widthPos) / _KerbSquareWidth);
                    float ySquare = floor(distanceAlongRoad / _KerbSquareLength);
                    float checker = fmod(xSquare + ySquare, 2.0);
                    baseColor = checker < 0.5 ? _KerbColor1 : _KerbColor2;
                }
                // LEFT EDGE LINE
                else if (widthPos > _KerbWidth && widthPos < (_KerbWidth + _EdgeLineWidth))
                {
                    baseColor = _EdgeLineColor;
                }
                // RIGHT EDGE LINE
                else if (widthPos < (1.0 - _KerbWidth) && widthPos > (1.0 - _KerbWidth - _EdgeLineWidth))
                {
                    baseColor = _EdgeLineColor;
                }
                else
                {
                    // CENTER DASHED LINE
                    float centerLine = 0.5 + _LineOffset;
                    bool inCenterLine = abs(widthPos - centerLine) < _LineWidth;
                    
                    float dashCycle = _DashLength + _GapLength;
                    float posInCycle = fmod(distanceAlongRoad, dashCycle);
                    bool isDash = posInCycle < _DashLength;
                    
                    if (inCenterLine && isDash)
                    {
                        baseColor = _LineColor;
                    }
                    else
                    {
                        baseColor = _RoadColor;
                    }
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
