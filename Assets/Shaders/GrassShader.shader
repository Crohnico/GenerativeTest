Shader "Custom/GrassShader"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Color("Color", Color) = (1,1,1,1)
        _DetailStrength("Detail Strength", Range(0, 1)) = 0.5
        _DetailSpeed("Detail Speed", Range(0, 10)) = 1
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_BumpMap;
            };

            sampler2D _MainTex;
            sampler2D _BumpMap;
            fixed4 _Color;
            float _DetailStrength;
            float _DetailSpeed;

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                // Agrega detalle dinámico utilizando la textura de bump map
                float detail = tex2D(_BumpMap, IN.uv_BumpMap + _Time.y * _DetailSpeed).r * _DetailStrength;
                baseColor.rgb += detail;

                o.Albedo = baseColor.rgb;
                o.Alpha = baseColor.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}
