Shader "CrossWaves/OceanFullscreen"
{
    Properties
    {
        // Phase 1 — ambient water
        _DeepColor       ("Deep Color",       Color)   = (0.01, 0.06, 0.18, 1)
        _ShallowColor    ("Shallow Color",    Color)   = (0.05, 0.22, 0.48, 1)
        _NormalMap       ("Normal Map",       2D)      = "bump" {}
        _NormalScaleA    ("Normal Scale A",   Float)   = 2.0
        _NormalScaleB    ("Normal Scale B",   Float)   = 5.0
        _ScrollSpeedA    ("Scroll Speed A",   Vector)  = (0.05, 0.03, 0, 0)
        _ScrollSpeedB    ("Scroll Speed B",   Vector)  = (-0.03, 0.04, 0, 0)
        [HDR]_SpecColor  ("Spec Color",       Color)   = (2, 2, 2, 1)
        _SpecPower       ("Spec Power",       Float)   = 32.0
        _LightDir        ("Light Direction",  Vector)  = (0.5, 0.7, 0.5, 0)
        _SwellStrength   ("Swell Strength",   Float)   = 0.008

        // Phase 2 — coastline foam
        _SDFTex          ("SDF Texture",      2D)      = "black" {}
        _GridRectUV      ("Grid Rect UV",     Vector)  = (0.2, 0.2, 0.8, 0.8)
        _FoamColor       ("Foam Color",       Color)   = (1.0, 1.0, 1.0, 1)
        _FoamWidth       ("Foam Width",       Float)   = 0.18
        _FoamSoftness    ("Foam Softness",    Float)   = 0.06
        _FoamSpeed       ("Foam Speed",       Float)   = 1.2
        _FoamOn          ("Foam On",          Float)   = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "OceanFullscreen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_NormalMap);   SAMPLER(sampler_NormalMap);
            TEXTURE2D(_SDFTex);      SAMPLER(sampler_SDFTex);

            CBUFFER_START(UnityPerMaterial)
                half4  _DeepColor;
                half4  _ShallowColor;
                half4  _SpecColor;
                half4  _FoamColor;
                half4  _GridRectUV;
                half4  _ScrollSpeedA;
                half4  _ScrollSpeedB;
                half   _NormalScaleA;
                half   _NormalScaleB;
                half   _SpecPower;
                half   _SwellStrength;
                half   _FoamWidth;
                half   _FoamSoftness;
                half   _FoamSpeed;
                half   _FoamOn;
                float4 _LightDir;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                half2 uv = input.texcoord;

                // Subtle large-scale swell
                half swellU = sin(uv.y * 6.283h + _Time.y * 0.2h) * _SwellStrength;
                half swellV = cos(uv.x * 6.283h + _Time.y * 0.15h) * _SwellStrength;
                half2 swelledUV = uv + half2(swellU, swellV);

                // Two normal map samples at different scales and scroll directions
                half2 uvA = swelledUV * _NormalScaleA + _ScrollSpeedA.xy * _Time.y;
                half2 uvB = swelledUV * _NormalScaleB + _ScrollSpeedB.xy * _Time.y;
                half3 nA = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvA));
                half3 nB = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvB));
                half3 surfNormal = normalize(nA + nB);

                // Blinn-Phong specular glint
                half3 lightDir = normalize((half3)_LightDir.xyz);
                half3 viewDir  = half3(0.0h, 0.0h, 1.0h);
                half3 halfVec  = normalize(lightDir + viewDir);
                half  spec     = pow(max(dot(surfNormal, halfVec), 0.0h), _SpecPower);

                // Deep/shallow colour — use X component which swings -1 to 1 across the surface
                half  depthFactor = surfNormal.x * 0.5h + 0.5h;
                half3 waterColor  = lerp(_DeepColor.rgb, _ShallowColor.rgb, depthFactor);
                waterColor += _SpecColor.rgb * spec;

                // --- Phase 2: coastline foam ---
                half foamTerm = 0.0h;
                if (_FoamOn > 0.5h)
                {
                    // Map screen UV into grid rect UV space
                    half2 gridMin = _GridRectUV.xy;
                    half2 gridMax = _GridRectUV.zw;
                    half2 gridUV  = (uv - gridMin) / max(gridMax - gridMin, 0.0001h);

                    if (gridUV.x >= 0.0h && gridUV.x <= 1.0h &&
                        gridUV.y >= 0.0h && gridUV.y <= 1.0h)
                    {
                        half dist = SAMPLE_TEXTURE2D(_SDFTex, sampler_SDFTex, gridUV).r;

                        // Animated foam breath — reuse nA as cheap noise
                        half noise  = nA.x * 0.5h + 0.5h;
                        half breath = sin(_Time.y * _FoamSpeed + noise * 6.283h) * 0.25h + 0.75h;

                        // Foam band: sea only (dist > 0), within FoamWidth of coast
                        half foamMask = (1.0h - smoothstep(_FoamWidth - _FoamSoftness,
                                                            _FoamWidth + _FoamSoftness, dist))
                                      * step(0.001h, dist);
                        foamTerm = saturate(foamMask * 2.0h) * breath;
                    }
                }

                half3 finalColor = lerp(waterColor, _FoamColor.rgb, foamTerm);
                return half4(finalColor, 1.0h);
            }
            ENDHLSL
        }
    }
}
