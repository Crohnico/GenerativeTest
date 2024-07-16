Shader "Custom/WindOscillationShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Frequency("Frequency", Range(0, 5)) = 1
        _Amplitude("_Amplitude", float) = 1
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            LOD 100

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
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _Frequency;
                float _Amplitude;

                float _MinAmplitude = -0.4;
                float _MaxAmplitude = 0.4;
                float _CurrentTargetAmplitude;
                float _TargetAmplitude;
                float _ChangeSpeed = 0.5;

                // Función para generar un número aleatorio basado en la posición Y del vértice
                float Rand(float seed)
                {
                    return frac(sin(seed) * 43758.5453);
                }

                v2f vert(appdata v)
                {
                    v2f o;

                    _Amplitude = lerp(_Amplitude, _CurrentTargetAmplitude, _ChangeSpeed * _Frequency * _Time.y);

                    /*if (abs(_Amplitude - _CurrentTargetAmplitude) < 0.001)
                    {
                        // Generar un nuevo objetivo aleatorio
                        float seed = v.vertex.y * _Time.y; // Usar la posición Y del vértice como semilla
                        _TargetAmplitude = lerp(_MinAmplitude, _MaxAmplitude, Rand(seed));
                        if (_TargetAmplitude < 0)
                        {
                            _TargetAmplitude = -_TargetAmplitude;
                        }
                        _CurrentTargetAmplitude = _TargetAmplitude;
                    }*/

                    float adjustedAmplitude = _Amplitude / _Frequency;

                    // Aplicar el desplazamiento al vértice en el eje Z basado en su posición Y
                    float displacement = adjustedAmplitude * sin(_Frequency * v.vertex.y);
                    v.vertex.z += displacement;

                    // Transformar el vértice al espacio de clip
                    o.vertex = UnityObjectToClipPos(v.vertex);

                    // Pasar las coordenadas UV al fragment shader
                    o.uv = v.uv;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                    return col;
                }
                ENDCG
            }
        }
}