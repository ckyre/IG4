Shader "Clouds"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        
        Pass
        {
            Name "CloudsRenderPass"

            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            
            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            
            Texture2D<float4> _ShapeNoise;
            SamplerState sampler_ShapeNoise;

            float4 _CloudColor;
            
            float _NumSteps;
            float _CloudScale;
            float _CloudSmooth;
            
            float3 _ContainerBoundsMin;
            float3 _ContainerBoundsMax;
            float _ContainerEdgeFadeDst;

            float3 _Wind;
            float _DensityThreshold;
            float _DensityMultiplier;

            float sampleDensity(float3 pos)
            {
                float3 uvw = pos * _CloudScale * 0.001 + _Wind.xyz * 0.1 * _Time.y * _CloudScale;
                float3 size = _ContainerBoundsMax - _ContainerBoundsMin;
                float3 center = (_ContainerBoundsMin + _ContainerBoundsMax) * 0.5f;
                
                float dstFromEdgeX = min(_ContainerEdgeFadeDst, min(pos.x - _ContainerBoundsMin.x, _ContainerBoundsMax.x - pos.x));
                float dstFromEdgeY = min(_CloudSmooth, min(pos.y - _ContainerBoundsMin.y, _ContainerBoundsMax.y - pos.y));
                float dstFromEdgeZ = min(_ContainerEdgeFadeDst, min(pos.z - _ContainerBoundsMin.z, _ContainerBoundsMax.z - pos.z));
                float edgeWeight = min(dstFromEdgeZ, dstFromEdgeX) / _ContainerEdgeFadeDst;

                float4 shape = _ShapeNoise.SampleLevel(sampler_ShapeNoise, uvw.xz, 0);
                float density = max(0, shape.x - _DensityThreshold) * _DensityMultiplier;
                return density * edgeWeight * (dstFromEdgeY / _CloudSmooth);
            }
            
            // Returns (distance to box, distance inside box).
            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir)
            {
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }
            
            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample previous render texture.
                float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.texcoord);

                // Ray settings.
                float3 viewVector = mul(unity_CameraInvProjection, float4(input.texcoord * 2 - 1, 0, -1));
                float3 viewDir = mul(unity_CameraToWorld, float4(viewVector, 0));
                float viewLength = length(viewDir);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = viewDir / viewLength;

                // Sample camera depth texture.
                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
                float4 zBufferParams = float4((100-0.1)/0.1, 1, (100-0.1)/(0.1*100), 1/100);
                float depth = LinearEyeDepth(nonlin_depth, zBufferParams) * viewLength;
                
                // Box distance.
                float2 rayToContainerInfo = rayBoxDst(_ContainerBoundsMin, _ContainerBoundsMax, rayOrigin, 1 / rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                float dstTravelled = 0.0;
                float stepSize = dstInsideBox / _NumSteps;
                float dstLimit = min(depth - dstToBox, dstInsideBox);
                
                float totalDensity = 0.0;
                while(dstTravelled < dstLimit)
                {
                    float3 rayPos = rayOrigin + rayDir * (dstToBox + dstTravelled);
                    totalDensity += sampleDensity(rayPos) * stepSize;
                    dstTravelled += stepSize;
                }
                float transmittance = exp(-totalDensity);

                return lerp(_CloudColor, color, transmittance);
            }
            
            ENDHLSL
        }
    }
}