Shader "Custom/Prop" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard vertex:myvert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float linear_depth;
		};

		void myvert(inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
			float4 tmp = UnityObjectToClipPos(v.vertex);
			data.linear_depth = tmp.w;
		}


		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		//Distance Desaturation
		float _MaxDesaturation;
		float _MaxDistance;

		float3 depthDesaturate(float3 color, float strength)
		{
			float r = color.x;
			float g = color.y;
			float b = color.z;
			float L = 0.3 * r + 0.6 * g + 0.1 * b;
			return float3(r + strength * (L - r), g + strength * (L - g), b + strength * (L - b));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			o.Albedo = depthDesaturate(o.Albedo, saturate(IN.linear_depth / _MaxDistance) * _MaxDesaturation);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
