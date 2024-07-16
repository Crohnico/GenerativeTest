// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CartoonWater"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_WaveSpeedZ("Wave Speed on Z", float) = 1.0
		_WaveSpeedX("Wave Speed on X", float) = 1.0


	    _WaveScaleZ("Wave Scale on Z", float) = 0.1
		_WaveScaleX("Wave Scale on X", float) = 0.1

		_WaveHeight("Wave Height", float) = 0.2
		_TextureScrollSpeed("Surf Speed", float) = 0.2
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert

			sampler2D _MainTex;
			float4 _Color;

			float _WaveSpeedZ;
			float _WaveSpeedX;

			float _WaveScaleX;
			float _WaveScaleZ;

			float _WaveHeight;
			float _TextureScrollSpeed;

			struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
			};

			void vert(inout appdata_full v, out Input o)
			{
				float timeFactorX = _Time.y * _WaveSpeedX;
				float wavePhaseX = timeFactorX * _WaveScaleX;

				float timeFactorZ = _Time.y * _WaveSpeedZ;
				float wavePhaseZ = timeFactorZ * _WaveScaleZ;

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				float roundedX = floor(worldPos.x + 0.5);
				float roundedZ = floor(worldPos.z + 0.5);

				float waveHeight = _WaveHeight * sin(wavePhaseX + roundedX) * sin(wavePhaseZ + roundedZ);

				v.vertex.z += waveHeight;

				o.uv_MainTex = v.texcoord;
				o.worldPos = v.vertex.xyz;
			}


			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				float2 offset = _Time.y * _TextureScrollSpeed;
				float2 uv = IN.uv_MainTex + offset;

				// Mantiene las coordenadas de textura en el rango [0, 1]
				uv = frac(uv);

				// Obtiene el color de la textura desplazada
				fixed4 c = tex2D(_MainTex, uv) * _Color;

				// Asigna el color al canal albedo de salida
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
