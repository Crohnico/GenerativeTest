Shader "Custom/WhiteReplace" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Threshold1("Threshold1", Range(0, 1)) = 0.6
        _Threshold2("Threshold2", Range(0, 1)) = 0.3
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert

            struct Input {
                float2 uv_MainTex;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Threshold1;
            float _Threshold2;

            void surf(Input IN, inout SurfaceOutput o) {
                fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);

                // Obtener el valor máximo de los componentes RGB
                float maxChannel = max(texColor.r, max(texColor.g, texColor.b));

                // Comparar el valor máximo con los umbrales
                if (maxChannel >= _Threshold1) {
                    o.Albedo = _Color.rgb; // Aplicar el color definido
                }
     else if (maxChannel >= _Threshold2) {
                    // Mezclar el color original de la textura con el color definido
                    o.Albedo = lerp(texColor.rgb, _Color.rgb, (_Threshold1 - maxChannel) / (_Threshold1 - _Threshold2));
                }
     else {
      o.Albedo = texColor.rgb; // Mantener el color original de la textura
  }

  o.Alpha = texColor.a;
}
ENDCG
        }
            FallBack "Diffuse"
}