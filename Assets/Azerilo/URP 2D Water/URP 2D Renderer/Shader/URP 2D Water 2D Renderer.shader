Shader "Azerilo/URP 2D Water 2D Renderer"
{
    Properties
    {
        Water_Top_Color("Water Top Color", Color) = (1, 1, 1, 0)
        Water_Top_Width("Water Top Width", Float) = 5
        Water_Color("Water Color", Color) = (0.8156863, 0.9568627, 1, 0)
        Water_Level("Water Level", Range(0, 1)) = 0.8
        Wave_Speed("Wave Speed", Float) = 3
        Wave_Frequency("Wave Frequency", Float) = 18
        Wave_Depth("Wave Depth", Range(0, 20)) = 1.4
        Refraction_Speed("Refraction Speed", Range(0, 20)) = 10
        Refraction_Noise("Refraction Noise", Float) = 70
        Vector1_D775EAAC("Refraction Strength", Range(-2, 2)) = 0.7
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"=""
        }
        Pass
        {
            Name "Sprite Lit"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>
        
            // Keywords
            #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
            // GraphKeywords: <None>
        
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define VARYINGS_NEED_SCREENPOSITION
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITELIT
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
             float4 screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyzw =  input.color;
            output.interp3.xyzw =  input.screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.color = input.interp2.xyzw;
            output.screenPosition = input.interp3.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Water_Top_Color;
        float Water_Top_Width;
        float4 Water_Color;
        float Water_Level;
        float Wave_Speed;
        float Wave_Frequency;
        float Wave_Depth;
        float Refraction_Speed;
        float Refraction_Noise;
        float Vector1_D775EAAC;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);
        float4 _CameraSortingLayerTexture_TexelSize;
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif
        
            // Graph Functions
            
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        
        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            float angle = dot(uv, float2(12.9898, 78.233));
            #if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN))
                // 'sin()' has bad precision on Mali GPUs for inputs > 10000
                angle = fmod(angle, TWO_PI); // Avoid large inputs to sin()
            #endif
            return frac(sin(angle)*43758.5453);
        }
        
        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }
        
        
        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
        
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);
        
            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;
        
            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            Out = t;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Less_float(float A, float B, out float Out)
        {
            Out = A < B ? 1 : 0;
        }
        
        void Unity_Branch_float2(float Predicate, float2 True, float2 False, out float2 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
            #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float4 SpriteMask;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0 = UnityBuildTexture2DStructNoScale(_CameraSortingLayerTexture);
            float4 _ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_373d7310208ff486bbf8a3259833d1b8_Out_0 = Refraction_Speed;
            float _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2;
            Unity_Divide_float(_Property_373d7310208ff486bbf8a3259833d1b8_Out_0, 100, _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2);
            float _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2;
            Unity_Multiply_float_float(_Divide_593e951e650bbd80996dccbbe89f53ac_Out_2, IN.TimeParameters.x, _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2);
            float2 _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2.xx), _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3);
            float _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0 = Refraction_Noise;
            float _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2;
            Unity_SimpleNoise_float(_TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3, _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0, _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2);
            float4 Color_5f26627bfa7e45ee92463dfb6a3fad53 = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0 = Vector1_D775EAAC;
            float _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2;
            Unity_Divide_float(_Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0, 100, _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2);
            float4 _Add_9ded394883e31188b2d77d852ecb6540_Out_2;
            Unity_Add_float4(Color_5f26627bfa7e45ee92463dfb6a3fad53, (_Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2);
            float4 _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2;
            Unity_Multiply_float4_float4((_SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2, _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2);
            float2 _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3;
            Unity_TilingAndOffset_float((_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), float2 (1, 1), (_Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3);
            float _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1;
            Unity_SceneDepth_Eye_float((float4(_TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, 0.0, 1.0)), _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1);
            float4 _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_R_1 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[0];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_G_2 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[1];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_B_3 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[2];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[3];
            float _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2;
            Unity_Subtract_float(_SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1, _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4, _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2);
            float _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2;
            Unity_Comparison_Less_float(_Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2, 0, _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2);
            float2 _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3;
            Unity_Branch_float2(_Comparison_739ba6765d539c82bd0510fa86047a34_Out_2, (_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3);
            float4 _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.tex, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.samplerstate, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.GetTransformedUV(_Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3));
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_R_4 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.r;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_G_5 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.g;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_B_6 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.b;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_A_7 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.a;
            float4 _Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0 = Water_Top_Color;
            float4 _UV_cae19cd50907c986bfac8000fcc55360_Out_0 = IN.uv0;
            float4 _Add_4b77fda1a266e5868c852226324046d5_Out_2;
            Unity_Add_float4(_Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0, _UV_cae19cd50907c986bfac8000fcc55360_Out_0, _Add_4b77fda1a266e5868c852226324046d5_Out_2);
            float4 _Property_7e27e38310ef2784a384c60888aa7849_Out_0 = Water_Color;
            float4 _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0 = IN.uv0;
            float _Split_b7f6798c5c3baf81a04496f4af428a21_R_1 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[0];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_G_2 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[1];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_B_3 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[2];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_A_4 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[3];
            float _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0 = Wave_Frequency;
            float _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2;
            Unity_Multiply_float_float(_Split_b7f6798c5c3baf81a04496f4af428a21_R_1, _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0, _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2);
            float _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0 = Wave_Speed;
            float _Multiply_a07084ffbf220388b55751a6362f056b_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2);
            float _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2;
            Unity_Add_float(_Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2, _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2);
            float _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1;
            Unity_Sine_float(_Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2, _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1);
            float _Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0 = Wave_Depth;
            float _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2;
            Unity_Divide_float(_Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0, 100, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2);
            float _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2;
            Unity_Multiply_float_float(_Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2, _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2);
            float _Property_02aa838086edbc889d6afefebe6be70d_Out_0 = Water_Level;
            float _Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0 = Water_Top_Width;
            float _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2;
            Unity_Divide_float(_Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0, 1000, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2);
            float _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2;
            Unity_Subtract_float(_Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2);
            float _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2);
            float _Comparison_40e1062705411a84b8476f128b40d741_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2, _Comparison_40e1062705411a84b8476f128b40d741_Out_2);
            float _Branch_453a802f56019e8ba166f6e33820a826_Out_3;
            Unity_Branch_float(_Comparison_40e1062705411a84b8476f128b40d741_Out_2, 0, 1, _Branch_453a802f56019e8ba166f6e33820a826_Out_3);
            float4 _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3;
            Unity_Lerp_float4(_Add_4b77fda1a266e5868c852226324046d5_Out_2, _Property_7e27e38310ef2784a384c60888aa7849_Out_0, (_Branch_453a802f56019e8ba166f6e33820a826_Out_3.xxxx), _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3);
            float4 _Multiply_702f9ba078084900941398eaa2f44e27_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0, _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3, _Multiply_702f9ba078084900941398eaa2f44e27_Out_2);
            float _Add_1113249fecc4d28da19af79ba352056d_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Add_1113249fecc4d28da19af79ba352056d_Out_2);
            float _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_1113249fecc4d28da19af79ba352056d_Out_2, _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2);
            float _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            Unity_Branch_float(_Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2, 0, 1, _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3);
            surface.BaseColor = (_Multiply_702f9ba078084900941398eaa2f44e27_Out_2.xyz);
            surface.Alpha = _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            surface.SpriteMask = IsGammaSpace() ? float4(1, 1, 1, 1) : float4 (SRGBToLinear(float3(1, 1, 1)), 1);
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.ScreenPosition =                             ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteLitPass.hlsl"
        
            ENDHLSL
        }
        Pass
        {
            Name "Sprite Normal"
            Tags
            {
                "LightMode" = "NormalsRendering"
            }
        
            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
        
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITENORMAL
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Water_Top_Color;
        float Water_Top_Width;
        float4 Water_Color;
        float Water_Level;
        float Wave_Speed;
        float Wave_Frequency;
        float Wave_Depth;
        float Refraction_Speed;
        float Refraction_Noise;
        float Vector1_D775EAAC;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);
        float4 _CameraSortingLayerTexture_TexelSize;
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif
        
            // Graph Functions
            
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        
        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            float angle = dot(uv, float2(12.9898, 78.233));
            #if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN))
                // 'sin()' has bad precision on Mali GPUs for inputs > 10000
                angle = fmod(angle, TWO_PI); // Avoid large inputs to sin()
            #endif
            return frac(sin(angle)*43758.5453);
        }
        
        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }
        
        
        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
        
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);
        
            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;
        
            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            Out = t;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Less_float(float A, float B, out float Out)
        {
            Out = A < B ? 1 : 0;
        }
        
        void Unity_Branch_float2(float Predicate, float2 True, float2 False, out float2 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
            #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0 = UnityBuildTexture2DStructNoScale(_CameraSortingLayerTexture);
            float4 _ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_373d7310208ff486bbf8a3259833d1b8_Out_0 = Refraction_Speed;
            float _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2;
            Unity_Divide_float(_Property_373d7310208ff486bbf8a3259833d1b8_Out_0, 100, _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2);
            float _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2;
            Unity_Multiply_float_float(_Divide_593e951e650bbd80996dccbbe89f53ac_Out_2, IN.TimeParameters.x, _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2);
            float2 _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2.xx), _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3);
            float _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0 = Refraction_Noise;
            float _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2;
            Unity_SimpleNoise_float(_TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3, _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0, _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2);
            float4 Color_5f26627bfa7e45ee92463dfb6a3fad53 = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0 = Vector1_D775EAAC;
            float _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2;
            Unity_Divide_float(_Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0, 100, _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2);
            float4 _Add_9ded394883e31188b2d77d852ecb6540_Out_2;
            Unity_Add_float4(Color_5f26627bfa7e45ee92463dfb6a3fad53, (_Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2);
            float4 _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2;
            Unity_Multiply_float4_float4((_SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2, _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2);
            float2 _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3;
            Unity_TilingAndOffset_float((_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), float2 (1, 1), (_Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3);
            float _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1;
            Unity_SceneDepth_Eye_float((float4(_TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, 0.0, 1.0)), _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1);
            float4 _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_R_1 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[0];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_G_2 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[1];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_B_3 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[2];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[3];
            float _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2;
            Unity_Subtract_float(_SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1, _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4, _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2);
            float _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2;
            Unity_Comparison_Less_float(_Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2, 0, _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2);
            float2 _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3;
            Unity_Branch_float2(_Comparison_739ba6765d539c82bd0510fa86047a34_Out_2, (_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3);
            float4 _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.tex, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.samplerstate, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.GetTransformedUV(_Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3));
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_R_4 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.r;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_G_5 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.g;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_B_6 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.b;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_A_7 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.a;
            float4 _Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0 = Water_Top_Color;
            float4 _UV_cae19cd50907c986bfac8000fcc55360_Out_0 = IN.uv0;
            float4 _Add_4b77fda1a266e5868c852226324046d5_Out_2;
            Unity_Add_float4(_Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0, _UV_cae19cd50907c986bfac8000fcc55360_Out_0, _Add_4b77fda1a266e5868c852226324046d5_Out_2);
            float4 _Property_7e27e38310ef2784a384c60888aa7849_Out_0 = Water_Color;
            float4 _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0 = IN.uv0;
            float _Split_b7f6798c5c3baf81a04496f4af428a21_R_1 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[0];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_G_2 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[1];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_B_3 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[2];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_A_4 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[3];
            float _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0 = Wave_Frequency;
            float _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2;
            Unity_Multiply_float_float(_Split_b7f6798c5c3baf81a04496f4af428a21_R_1, _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0, _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2);
            float _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0 = Wave_Speed;
            float _Multiply_a07084ffbf220388b55751a6362f056b_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2);
            float _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2;
            Unity_Add_float(_Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2, _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2);
            float _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1;
            Unity_Sine_float(_Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2, _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1);
            float _Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0 = Wave_Depth;
            float _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2;
            Unity_Divide_float(_Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0, 100, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2);
            float _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2;
            Unity_Multiply_float_float(_Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2, _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2);
            float _Property_02aa838086edbc889d6afefebe6be70d_Out_0 = Water_Level;
            float _Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0 = Water_Top_Width;
            float _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2;
            Unity_Divide_float(_Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0, 1000, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2);
            float _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2;
            Unity_Subtract_float(_Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2);
            float _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2);
            float _Comparison_40e1062705411a84b8476f128b40d741_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2, _Comparison_40e1062705411a84b8476f128b40d741_Out_2);
            float _Branch_453a802f56019e8ba166f6e33820a826_Out_3;
            Unity_Branch_float(_Comparison_40e1062705411a84b8476f128b40d741_Out_2, 0, 1, _Branch_453a802f56019e8ba166f6e33820a826_Out_3);
            float4 _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3;
            Unity_Lerp_float4(_Add_4b77fda1a266e5868c852226324046d5_Out_2, _Property_7e27e38310ef2784a384c60888aa7849_Out_0, (_Branch_453a802f56019e8ba166f6e33820a826_Out_3.xxxx), _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3);
            float4 _Multiply_702f9ba078084900941398eaa2f44e27_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0, _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3, _Multiply_702f9ba078084900941398eaa2f44e27_Out_2);
            float _Add_1113249fecc4d28da19af79ba352056d_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Add_1113249fecc4d28da19af79ba352056d_Out_2);
            float _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_1113249fecc4d28da19af79ba352056d_Out_2, _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2);
            float _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            Unity_Branch_float(_Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2, 0, 1, _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3);
            surface.BaseColor = (_Multiply_702f9ba078084900941398eaa2f44e27_Out_2.xyz);
            surface.Alpha = _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            surface.NormalTS = IN.TangentSpaceNormal;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.ScreenPosition =                             ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteNormalPass.hlsl"
        
            ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
            // Render State
            Cull Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
        
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Water_Top_Color;
        float Water_Top_Width;
        float4 Water_Color;
        float Water_Level;
        float Wave_Speed;
        float Wave_Frequency;
        float Wave_Depth;
        float Refraction_Speed;
        float Refraction_Noise;
        float Vector1_D775EAAC;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);
        float4 _CameraSortingLayerTexture_TexelSize;
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif
        
            // Graph Functions
            
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
            #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0 = IN.uv0;
            float _Split_b7f6798c5c3baf81a04496f4af428a21_R_1 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[0];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_G_2 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[1];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_B_3 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[2];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_A_4 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[3];
            float _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0 = Wave_Frequency;
            float _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2;
            Unity_Multiply_float_float(_Split_b7f6798c5c3baf81a04496f4af428a21_R_1, _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0, _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2);
            float _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0 = Wave_Speed;
            float _Multiply_a07084ffbf220388b55751a6362f056b_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2);
            float _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2;
            Unity_Add_float(_Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2, _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2);
            float _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1;
            Unity_Sine_float(_Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2, _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1);
            float _Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0 = Wave_Depth;
            float _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2;
            Unity_Divide_float(_Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0, 100, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2);
            float _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2;
            Unity_Multiply_float_float(_Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2, _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2);
            float _Property_02aa838086edbc889d6afefebe6be70d_Out_0 = Water_Level;
            float _Add_1113249fecc4d28da19af79ba352056d_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Add_1113249fecc4d28da19af79ba352056d_Out_2);
            float _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_1113249fecc4d28da19af79ba352056d_Out_2, _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2);
            float _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            Unity_Branch_float(_Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2, 0, 1, _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3);
            surface.Alpha = _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
            ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
            // Render State
            Cull Back
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
        
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 interp0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.interp0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Water_Top_Color;
        float Water_Top_Width;
        float4 Water_Color;
        float Water_Level;
        float Wave_Speed;
        float Wave_Frequency;
        float Wave_Depth;
        float Refraction_Speed;
        float Refraction_Noise;
        float Vector1_D775EAAC;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);
        float4 _CameraSortingLayerTexture_TexelSize;
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif
        
            // Graph Functions
            
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
            #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
            // Graph Pixel
            struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0 = IN.uv0;
            float _Split_b7f6798c5c3baf81a04496f4af428a21_R_1 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[0];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_G_2 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[1];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_B_3 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[2];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_A_4 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[3];
            float _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0 = Wave_Frequency;
            float _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2;
            Unity_Multiply_float_float(_Split_b7f6798c5c3baf81a04496f4af428a21_R_1, _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0, _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2);
            float _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0 = Wave_Speed;
            float _Multiply_a07084ffbf220388b55751a6362f056b_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2);
            float _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2;
            Unity_Add_float(_Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2, _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2);
            float _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1;
            Unity_Sine_float(_Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2, _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1);
            float _Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0 = Wave_Depth;
            float _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2;
            Unity_Divide_float(_Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0, 100, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2);
            float _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2;
            Unity_Multiply_float_float(_Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2, _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2);
            float _Property_02aa838086edbc889d6afefebe6be70d_Out_0 = Water_Level;
            float _Add_1113249fecc4d28da19af79ba352056d_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Add_1113249fecc4d28da19af79ba352056d_Out_2);
            float _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_1113249fecc4d28da19af79ba352056d_Out_2, _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2);
            float _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            Unity_Branch_float(_Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2, 0, 1, _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3);
            surface.Alpha = _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
            ENDHLSL
        }
        Pass
        {
            Name "Sprite Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
            // Render State
            Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>
        
            // Keywords
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            // GraphKeywords: <None>
        
            // Defines
            #define _SURFACE_TYPE_TRANSPARENT 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SPRITEFORWARD
        #define REQUIRE_DEPTH_TEXTURE
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
            // Includes
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 ScreenPosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
             float4 interp2 : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyzw =  input.texCoord0;
            output.interp2.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.texCoord0 = input.interp1.xyzw;
            output.color = input.interp2.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 Water_Top_Color;
        float Water_Top_Width;
        float4 Water_Color;
        float Water_Level;
        float Wave_Speed;
        float Wave_Frequency;
        float Wave_Depth;
        float Refraction_Speed;
        float Refraction_Noise;
        float Vector1_D775EAAC;
        CBUFFER_END
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_CameraSortingLayerTexture);
        SAMPLER(sampler_CameraSortingLayerTexture);
        float4 _CameraSortingLayerTexture_TexelSize;
        
            // Graph Includes
            // GraphIncludes: <None>
        
            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif
        
            // Graph Functions
            
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        
        inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
        {
            float angle = dot(uv, float2(12.9898, 78.233));
            #if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN))
                // 'sin()' has bad precision on Mali GPUs for inputs > 10000
                angle = fmod(angle, TWO_PI); // Avoid large inputs to sin()
            #endif
            return frac(sin(angle)*43758.5453);
        }
        
        inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }
        
        
        inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
        
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = Unity_SimpleNoise_RandomValue_float(c0);
            float r1 = Unity_SimpleNoise_RandomValue_float(c1);
            float r2 = Unity_SimpleNoise_RandomValue_float(c2);
            float r3 = Unity_SimpleNoise_RandomValue_float(c3);
        
            float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
            float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
            float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
        {
            float t = 0.0;
        
            float freq = pow(2.0, float(0));
            float amp = pow(0.5, float(3-0));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
        
            Out = t;
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
        {
            if (unity_OrthoParams.w == 1.0)
            {
                Out = LinearEyeDepth(ComputeWorldSpacePosition(UV.xy, SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), UNITY_MATRIX_I_VP), UNITY_MATRIX_V);
            }
            else
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Less_float(float A, float B, out float Out)
        {
            Out = A < B ? 1 : 0;
        }
        
        void Unity_Branch_float2(float Predicate, float2 True, float2 False, out float2 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
            // Graph Vertex
            struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
            #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0 = UnityBuildTexture2DStructNoScale(_CameraSortingLayerTexture);
            float4 _ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Property_373d7310208ff486bbf8a3259833d1b8_Out_0 = Refraction_Speed;
            float _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2;
            Unity_Divide_float(_Property_373d7310208ff486bbf8a3259833d1b8_Out_0, 100, _Divide_593e951e650bbd80996dccbbe89f53ac_Out_2);
            float _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2;
            Unity_Multiply_float_float(_Divide_593e951e650bbd80996dccbbe89f53ac_Out_2, IN.TimeParameters.x, _Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2);
            float2 _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_2e179bde7dfd688e85aaae302149f8c2_Out_2.xx), _TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3);
            float _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0 = Refraction_Noise;
            float _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2;
            Unity_SimpleNoise_float(_TilingAndOffset_f90ee46f27f3d081b4ddb84578c783f6_Out_3, _Property_ff0baa56ee3d568eb312f3a6f2b2d3af_Out_0, _SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2);
            float4 Color_5f26627bfa7e45ee92463dfb6a3fad53 = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
            float _Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0 = Vector1_D775EAAC;
            float _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2;
            Unity_Divide_float(_Property_1de02d5f49a08b8eab610a90e1911cb1_Out_0, 100, _Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2);
            float4 _Add_9ded394883e31188b2d77d852ecb6540_Out_2;
            Unity_Add_float4(Color_5f26627bfa7e45ee92463dfb6a3fad53, (_Divide_58924c2a5e735c81b2309aa2eb82458b_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2);
            float4 _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2;
            Unity_Multiply_float4_float4((_SimpleNoise_badb01badbf2298ca74526e8b17673ab_Out_2.xxxx), _Add_9ded394883e31188b2d77d852ecb6540_Out_2, _Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2);
            float2 _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3;
            Unity_TilingAndOffset_float((_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), float2 (1, 1), (_Multiply_2d5921574ba3db8e96a47f3ed5a79378_Out_2.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3);
            float _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1;
            Unity_SceneDepth_Eye_float((float4(_TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, 0.0, 1.0)), _SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1);
            float4 _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0 = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_R_1 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[0];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_G_2 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[1];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_B_3 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[2];
            float _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4 = _ScreenPosition_8224cb8e01422e84939a0f447a510ae4_Out_0[3];
            float _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2;
            Unity_Subtract_float(_SceneDepth_cb726e53e537878f8dded5b13dbb196b_Out_1, _Split_de4ec4e893dff3848faba8eddc2c33b8_A_4, _Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2);
            float _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2;
            Unity_Comparison_Less_float(_Subtract_e04a11cdf02ef4809bcac78822696f74_Out_2, 0, _Comparison_739ba6765d539c82bd0510fa86047a34_Out_2);
            float2 _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3;
            Unity_Branch_float2(_Comparison_739ba6765d539c82bd0510fa86047a34_Out_2, (_ScreenPosition_8ed4b323f4f106849eda05f041314f2c_Out_0.xy), _TilingAndOffset_8e76997513c4ad858cdf0148ebee6db8_Out_3, _Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3);
            float4 _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0 = SAMPLE_TEXTURE2D(_Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.tex, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.samplerstate, _Property_5be0163405f8415ea5aa3c3a2ed93d27_Out_0.GetTransformedUV(_Branch_8ec74c3ea8cc4689a1cf8811f1410039_Out_3));
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_R_4 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.r;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_G_5 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.g;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_B_6 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.b;
            float _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_A_7 = _SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0.a;
            float4 _Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0 = Water_Top_Color;
            float4 _UV_cae19cd50907c986bfac8000fcc55360_Out_0 = IN.uv0;
            float4 _Add_4b77fda1a266e5868c852226324046d5_Out_2;
            Unity_Add_float4(_Property_2431f4c9d874f78ebe1bc2867afaa2ec_Out_0, _UV_cae19cd50907c986bfac8000fcc55360_Out_0, _Add_4b77fda1a266e5868c852226324046d5_Out_2);
            float4 _Property_7e27e38310ef2784a384c60888aa7849_Out_0 = Water_Color;
            float4 _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0 = IN.uv0;
            float _Split_b7f6798c5c3baf81a04496f4af428a21_R_1 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[0];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_G_2 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[1];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_B_3 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[2];
            float _Split_b7f6798c5c3baf81a04496f4af428a21_A_4 = _UV_6f1267cd4a4f6b8c9d18af15f7eedb7e_Out_0[3];
            float _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0 = Wave_Frequency;
            float _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2;
            Unity_Multiply_float_float(_Split_b7f6798c5c3baf81a04496f4af428a21_R_1, _Property_4f7aae92b853da85956c0fd7469dc4f0_Out_0, _Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2);
            float _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0 = Wave_Speed;
            float _Multiply_a07084ffbf220388b55751a6362f056b_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_7d1c5f931dfe7f8bb61a1fe5ccd7aca7_Out_0, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2);
            float _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2;
            Unity_Add_float(_Multiply_c48821dac71694859e6d01f7dc8bd6b6_Out_2, _Multiply_a07084ffbf220388b55751a6362f056b_Out_2, _Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2);
            float _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1;
            Unity_Sine_float(_Add_068b73da3bb6c48d9bce70e4e7c9d03e_Out_2, _Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1);
            float _Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0 = Wave_Depth;
            float _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2;
            Unity_Divide_float(_Property_6fe884b2bd79cd8ebcde9902b4feaab3_Out_0, 100, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2);
            float _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2;
            Unity_Multiply_float_float(_Sine_760e7551b6c0c284a416b07723a7c3a1_Out_1, _Divide_c4e337ab38b92b8ea564d7ffa749cce1_Out_2, _Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2);
            float _Property_02aa838086edbc889d6afefebe6be70d_Out_0 = Water_Level;
            float _Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0 = Water_Top_Width;
            float _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2;
            Unity_Divide_float(_Property_ed5ff1176af7ba83b365b4b2dc2e779f_Out_0, 1000, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2);
            float _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2;
            Unity_Subtract_float(_Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Divide_9cbdbc2bc747e38a9cae5fec062cc368_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2);
            float _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Subtract_98378fe44c51a08a88c0dd33d0fcdee2_Out_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2);
            float _Comparison_40e1062705411a84b8476f128b40d741_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_7df99c4416d7c68e8b3a6fff849116c8_Out_2, _Comparison_40e1062705411a84b8476f128b40d741_Out_2);
            float _Branch_453a802f56019e8ba166f6e33820a826_Out_3;
            Unity_Branch_float(_Comparison_40e1062705411a84b8476f128b40d741_Out_2, 0, 1, _Branch_453a802f56019e8ba166f6e33820a826_Out_3);
            float4 _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3;
            Unity_Lerp_float4(_Add_4b77fda1a266e5868c852226324046d5_Out_2, _Property_7e27e38310ef2784a384c60888aa7849_Out_0, (_Branch_453a802f56019e8ba166f6e33820a826_Out_3.xxxx), _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3);
            float4 _Multiply_702f9ba078084900941398eaa2f44e27_Out_2;
            Unity_Multiply_float4_float4(_SampleTexture2D_69897ac780e148ba979c0ae7a9b716a3_RGBA_0, _Lerp_f44b73cbec88f78c828ce3d15cb88ed1_Out_3, _Multiply_702f9ba078084900941398eaa2f44e27_Out_2);
            float _Add_1113249fecc4d28da19af79ba352056d_Out_2;
            Unity_Add_float(_Multiply_fdd6b73180e3858fa5fc065d48e419a4_Out_2, _Property_02aa838086edbc889d6afefebe6be70d_Out_0, _Add_1113249fecc4d28da19af79ba352056d_Out_2);
            float _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2;
            Unity_Comparison_Greater_float(_Split_b7f6798c5c3baf81a04496f4af428a21_G_2, _Add_1113249fecc4d28da19af79ba352056d_Out_2, _Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2);
            float _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            Unity_Branch_float(_Comparison_9e92e65e6497ca8eac41208ea7d9f17e_Out_2, 0, 1, _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3);
            surface.BaseColor = (_Multiply_702f9ba078084900941398eaa2f44e27_Out_2.xyz);
            surface.Alpha = _Branch_9da848ed469ee88b867bbe3abf39a245_Out_3;
            surface.NormalTS = IN.TangentSpaceNormal;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.ScreenPosition =                             ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
            output.uv0 =                                        input.texCoord0;
            output.TimeParameters =                             _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteForwardPass.hlsl"
        
            ENDHLSL
        }
    }
}