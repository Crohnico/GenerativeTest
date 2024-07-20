Shader "Custom/Sh_GTWater"
{
    Properties
    {
        _WaveSpeed("Wave Speed", Range(0.1, 10.0)) = 1.0
        _WaveHeight("Wave Height", Range(0.1, 2.0)) = 1.0
        _MainTex("Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _ReflectionTex("Reflection Texture", 2D) = "white" {}
        _RefractionTex("Refraction Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert alpha:blend

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _ReflectionTex;
        sampler2D _RefractionTex;
        float _WaveSpeed;
        float _WaveHeight;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            float wave = sin(IN.worldPos.x * _WaveSpeed + _Time.y) * _WaveHeight;
            wave += sin(IN.worldPos.z * _WaveSpeed + _Time.y * 0.5) * _WaveHeight * 0.5;

            float3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex)).rgb;
            float3 reflection = tex2D(_ReflectionTex, IN.uv_MainTex).rgb;
            float3 refraction = tex2D(_RefractionTex, IN.uv_MainTex).rgb;

            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * lerp(reflection, refraction, wave);
            o.Normal = normal;
            o.Alpha = saturate(1 - wave);
        }
        ENDCG
    }
        FallBack "Diffuse"
}