Shader "CloudsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
            };

            // Rendering texture.
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Depth texture.
            sampler2D _CameraDepthTexture;

            // Ray marcing settings.
            int _NumSteps;
            int _NumLightSteps;
            float _RenderDistance;

            // Clouds settings.
            half4 _CloudsColor;
            float _CloudsAlpha;
            Texture2D<float4> _CloudsShapeNoiseTexture;
            SamplerState sampler_CloudsShapeNoiseTexture;
            float _CloudsScale;
            float _CloudsSmooth;
            float3 _Wind;

            // Clouds lightning settings.
            float _DensityThreshold;
            float _DensityMultiplier;
            float _LightAbsorptionThroughCloud;
            float _LightAbsorptionTowardSun;
            float _DarknessThreshold;
            float4 _PhaseParams;

            // Container settings.
            float3 _ContainerBoundsMin;
            float3 _ContainerBoundsMax;
            float _ContainerEdgeFadeDst;

            v2f vert (appdata v)
            {
                // Prepare output (vertex poisition, uv).
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // This calculate the ray direction for each pixels of the screen.
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewDir = mul(unity_CameraToWorld, float4(viewVector,0));
                
                return o;
            }
            
            // Calculates the distance of the ray from the box and it oposite face.
            // Returns two floats: distanceToBox and distanceInsideBox.
            // If the ray don't collide with the container, distanceInsideBox will be zeo.
            // This function comes from Sebastien Lague video. Adapted from: http://jcgt.org/published/0007/03/04/
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

            // Samples the density of the clouds at one point inside the container.
            // Uses the noise function to calculate the density.
            float sampleDensity(float3 pos)
            {
                float3 uvw = pos * _CloudsScale * 0.001 + _Wind.xyz * 0.1 * _Time.y * _CloudsScale;

                float dstFromEdgeX = min(_ContainerEdgeFadeDst, min(pos.x - _ContainerBoundsMin.x, _ContainerBoundsMax.x - pos.x));
                float dstFromEdgeY = min(_CloudsSmooth, min(pos.y - _ContainerBoundsMin.y, _ContainerBoundsMax.y - pos.y));
                float dstFromEdgeZ = min(_ContainerEdgeFadeDst, min(pos.z - _ContainerBoundsMin.z, _ContainerBoundsMax.z - pos.z));
                float edgeWeight = min(dstFromEdgeZ,dstFromEdgeX)/_ContainerEdgeFadeDst;

                float4 shape = _CloudsShapeNoiseTexture.SampleLevel(sampler_CloudsShapeNoiseTexture, uvw.xz, 0);
                float density = max(0, shape.x - _DensityThreshold) * _DensityMultiplier;
                return density * edgeWeight * (dstFromEdgeY/_CloudsSmooth);
            }

            // Henyey-Greenstein.
            float hg(float angle, float g)
            {
                float g2 = g * g;
                return (1 - g2) / (4 * 3.1415 * pow( 1 + g2 - 2 * g * (angle), 1.5));
            }

            // Phase.
            // This function makes clouds brighter around sun.
            float phase(float3 rayDir, float3 lightPos)
            {
                float4 params = float4(0.1f, 0.25f, 0.5f, 0.0f);
                float blend = 0.5f;

                float angle = dot(rayDir, lightPos);
                float hgBlend = hg(angle,params.x) * (1-blend) + hg(angle,-params.y) * blend;
                return params.z + hgBlend * params.w;
            }

            // Calculates the lightning at a point.
            // This function raymarch from the given point to the light source
            // and calculates the density regulary.
            float lightmarch(float3 position)
            {
                float3 dirToLight = _WorldSpaceLightPos0.xyz;
                float dstInsideBox = rayBoxDst(_ContainerBoundsMin, _ContainerBoundsMax, position, 1/dirToLight).y;
                
                float stepSize = dstInsideBox/_NumLightSteps;
                float totalDensity = 0;

                for (int step = 0; step < _NumLightSteps; step ++)
                {
                    position += dirToLight * stepSize;
                    totalDensity += max(0, sampleDensity(position) * stepSize);
                }

                float transmittance = exp(-totalDensity * _LightAbsorptionTowardSun);
                return _DarknessThreshold + transmittance * (1 - _DarknessThreshold);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the render texture.
                fixed4 color = tex2D(_MainTex, i.uv);

                // Ray settings.
                float viewLength = length(i.viewDir);
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = i.viewDir / viewLength;
                
                // Sample the depth texture.
                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float depth = LinearEyeDepth(nonlin_depth) * viewLength;

                // First ray compute.
                float2 rayToContainerInfo = rayBoxDst(_ContainerBoundsMin, _ContainerBoundsMax, rayOrigin, 1/rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                // If the container is out of render distance, do nothing.
                if(dstToBox + dstInsideBox > _RenderDistance)
                {
                    return color;
                }

                // Compute density a each sample points inside the clouds.
                float dstTravelled = 0.0f;
                float stepSize = dstInsideBox / _NumSteps;
                float dstLimit = min(depth - dstToBox, dstInsideBox);

                float3 entryPoint = rayOrigin + rayDir * dstToBox;
                float transmittance = 1;
                float3 lightEnergy = 0;
                
                float phaseVal = phase(rayDir, _WorldSpaceLightPos0.xyz);

                while (dstTravelled < dstLimit)
                {
                    rayOrigin = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayOrigin);
                    
                    if (density > 0)
                    {
                        // If the point is inside a cloud, calculate lightning on this point.
                        float lightTransmittance = lightmarch(rayOrigin);
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseVal;
                        transmittance *= exp(-density * stepSize * _LightAbsorptionThroughCloud);
                    
                        if (transmittance < 0.1)
                        {
                            break;
                        }
                    }
                    dstTravelled += stepSize;
                }

                // Calculates final color of the pixel.
                float3 cloudColor = lightEnergy * _CloudsColor;
                float3 finalColor = color * transmittance + cloudColor;
                return float4(lerp(color, finalColor, _CloudsAlpha), 1.0);
            }
            ENDCG
        }
    }
}
