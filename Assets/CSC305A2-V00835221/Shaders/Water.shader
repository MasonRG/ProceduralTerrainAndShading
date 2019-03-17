Shader "Custom/Water" 
{
	Properties
	{
	}
		SubShader
		{
			Tags { "Queue"="Transparent" "RenderType" = "Transparent" }
			LOD 100
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

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
					float4 vertex : SV_POSITION;
					float3 normal : TEXCOORD1;
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


				//Water parameters
				sampler2D _MainTex;
				float _TextureScale;
				float3 _TintColour;
				float _TintStrength;
				float _Transparency;

				float _MoveSpeed;
				float _RippleSpeed;
				float _RippleHeight;
				float _TimeSinceStart;

				float inverseLerp(float a, float b, float value)
				{
					return saturate((value - a) / (b - a));
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
					
					return float4(Ambient + Diffuse + Specular, _Transparency);
				}



				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.world_pos = mul(unity_ObjectToWorld, v.vertex);

					///Demo 2 - Time based vertex movement
					float coordinate_sum = o.world_pos.x + o.world_pos.y + o.world_pos.z;
					float sin_v = sin(coordinate_sum + _Time * _MoveSpeed * _RippleSpeed);

					o.world_pos.x += _Time * _MoveSpeed;
					o.world_pos.z += _Time * _MoveSpeed;
					o.vertex.y += sin_v * _RippleHeight;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//project on the y axis -- water is flat in the xz plane so this will work out fine
					float3 tint = _TintStrength * _TintColour;
					float2 samplePos = i.world_pos.xz / _TextureScale;
					float3 color =  (1 - _TintStrength) * tex2D(_MainTex, samplePos);
					float3 albedo = tint + color;

					fixed4 pb = blinnPhong(i.world_pos, i.normal, albedo);
					return pb;
				}
				ENDCG
			}
		}
}
