// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Terrain" 
{
	Properties
	{
		// ***
		// *** These parameters are modified/set via the TerrainShader.cs script!
		// ***
		//_LightDir("Light Direction", vector) = (0,0,0)
		//_LightColor("Light Color", Color) = (1,1,1,1)
		//_Ka("Ambient Strength", float) = 1
		//_Kd("Diffuse Strength", float) = 1
		//_SpecColor("Specular Color", Color) = (1,1,1,1)
		//_Ks("Specular Strength", float) = 1
		//_Shininess("Specular Shininess", float) = 1
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
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
					float3 normal : NORMAL;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 normal : TEXCOORD1;
					float linear_depth : TEXCOORD2;
					float3 world_pos : TEXCOORD3;
				};

				//Lighting parameters
				float _Ka;
				float _Kd;
				float _Ks;
				float _Shininess;

				float3 _LightDir;
				float3 _LightColor;
				float3 _SpecColor;


				//Terrain parameters
				float _MaxHeight;
				int _DataCount;
				UNITY_DECLARE_TEX2DARRAY(_DataTextures);
				float3 _DataColours[8];
				float _DataColourStrengths[8];
				float _DataStartHeights[8];
				float _DataBlendStrengths[8];
				float _DataTextureScales[8];

				//Distance Desaturation
				float _MaxDesaturation;
				float _MaxDistance;


				float inverseLerp(float a, float b, float value)
				{
					return saturate((value - a) / (b - a));
				}


				float4 depthDesaturate(float4 color, float strength)
				{
					float r = color.x;
					float g = color.y;
					float b = color.z;
					float L = 0.3 * r + 0.6 * g + 0.1 * b;
					return float4(r + strength * (L - r), g + strength * (L - g), b + strength * (L - b), color.w);
				}

				fixed4 blinnPhong(float3 worldPos, float3 normal, float3 albedo)
				{
					//computing direction vectors for vertex normal, light direction, and the half direction (half-between light and view directions)
					float3 N = normalize(normal);
					float3 V = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
					float3 L = normalize(_LightDir.xyz);
					float3 H = normalize(V + L);

					//ambient lighting
					float3 Ambient = _Ka * albedo;

					//diffuse lighting
					float3 Diffuse = _Kd * _LightColor * albedo * max(0.0, dot(N, L));

					//specular lighting
					float3 Specular;
					if (dot(N, L) < 0.0)
					{
						Specular = float3(0.0, 0.0, 0.0);
					}
					else
					{
						Specular = _Ks * _SpecColor * pow(max(0.0, dot(reflect(-L, N), V)), _Shininess);
					}

					return float4(Ambient + Diffuse + Specular, 1.0);
				}


				float3 triplanarMap(float3 worldPos, float scale, float3 blendAxes, int index)
				{
					//tri-planar mapping for texture application
					float3 scaledWorldPos = worldPos / scale;

					float3 projX = UNITY_SAMPLE_TEX2DARRAY(_DataTextures, float3(scaledWorldPos.yz, index)) * blendAxes.x;
					float3 projY = UNITY_SAMPLE_TEX2DARRAY(_DataTextures, float3(scaledWorldPos.xz, index)) * blendAxes.y;
					float3 projZ = UNITY_SAMPLE_TEX2DARRAY(_DataTextures, float3(scaledWorldPos.xy, index)) * blendAxes.z;

					return projX + projY + projZ;
				}



				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.linear_depth = o.vertex.w;
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.uv = v.uv;
					o.world_pos = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float3 color = float3(0,0,0);

					float3 blendAxes = abs(i.normal);
					blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
					
					float normHeight = inverseLerp(0, _MaxHeight, i.world_pos.y);
					for (int k = 0; k < _DataCount; k++)
					{
						float strength = inverseLerp(-_DataBlendStrengths[k]/2 - 0.0001, _DataBlendStrengths[k]/2, normHeight - _DataStartHeights[k]);
						float3 tint = _DataColours[k] * _DataColourStrengths[k];
						float3 texColour = triplanarMap(i.world_pos, _DataTextureScales[k], blendAxes, k) * (1 - _DataColourStrengths[k]);
						color.xyz = color.xyz * (1 - strength) + (tint + texColour) * strength;
					}

					fixed4 pb = blinnPhong(i.world_pos, i.normal, color);
					pb = depthDesaturate(pb, saturate(i.linear_depth / _MaxDistance) * _MaxDesaturation);
					return pb;
				}
				ENDCG
			}
		}
}
