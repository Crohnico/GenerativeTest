// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Water"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_Smoothness("Smoothness", Float) = 0.5
		_FresnelOpacityPower("Fresnel Opacity Power", Float) = 0
		[HDR][Header(FOG)]_FogColor("Fog Color", Color) = (1,1,1,1)
		_FogThreshold("Fog Threshold", Float) = 0
		[HDR]_IntersectionColor("Intersection Color", Color) = (1,1,1,1)
		_IntersectionThreshold("Intersection Threshold", Float) = 0
		[Header(FOAM)]_FoamThreshold("Foam Threshold", Float) = 0
		_FoamTextureSpeed("Foam Texture Speed", Vector) = (0,0,0,0)
		_FoamNoiseScale("Foam Noise Scale", Float) = 8
		_FoamLinesSpeed("Foam Lines Speed", Float) = 0
		_HardFoam("Hard Foam", Range( 0 , 1)) = 0.2
		_SoftFoam("Soft Foam", Range( 0 , 1)) = 1
		[HDR]_FoamColor("Foam Color", Color) = (1,1,1,1)
		[Header(WAVES)]_WavesA("Waves A", 2D) = "bump" {}
		_WavesB("Waves B", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Float) = 1
		[Toggle]_Radial("Radial", Float) = 0
		_NormalPanningSpeeds("Normal Panning Speeds", Vector) = (0,0,0,0)
		[Header(RADIAL)]_PolarCenter("Polar Center", Vector) = (0.5,0.5,0,0)
		_PolarRadialScale("Polar Radial Scale", Float) = 0
		_PolarLenghtScale("Polar Lenght Scale", Float) = 0
		[Header(DISPLACEMENT)]_HeightAmplitude("HeightAmplitude", Float) = 0.5
		_HeighSpeed("HeighSpeed", Float) = 1
		_WaveDisplacementFactor("Wave Displacement Factor", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:premul keepalpha exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float _HeightAmplitude;
		uniform float _HeighSpeed;
		uniform sampler2D _WavesA;
		uniform float4 _NormalPanningSpeeds;
		uniform float _Radial;
		uniform float4 _WavesA_ST;
		uniform float2 _PolarCenter;
		uniform float _PolarRadialScale;
		uniform float _PolarLenghtScale;
		uniform float _NormalStrength;
		uniform sampler2D _WavesB;
		uniform float4 _WavesB_ST;
		uniform float _WaveDisplacementFactor;
		uniform float4 _IntersectionColor;
		uniform float4 _FogColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FogThreshold;
		uniform float4 _Color;
		uniform float _IntersectionThreshold;
		uniform float2 _FoamTextureSpeed;
		uniform float _FoamNoiseScale;
		uniform float _FoamThreshold;
		uniform float _FoamLinesSpeed;
		uniform float _HardFoam;
		uniform float _SoftFoam;
		uniform float4 _FoamColor;
		uniform float _Smoothness;
		uniform float _FresnelOpacityPower;


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime128 = _Time.y * _HeighSpeed;
			float2 appendResult99 = (float2(_NormalPanningSpeeds.x , _NormalPanningSpeeds.y));
			float2 uv_WavesA = v.texcoord.xy * _WavesA_ST.xy + _WavesA_ST.zw;
			float2 CenteredUV15_g3 = ( uv_WavesA - _PolarCenter );
			float2 break17_g3 = CenteredUV15_g3;
			float2 appendResult23_g3 = (float2(( length( CenteredUV15_g3 ) * _PolarRadialScale * 2.0 ) , ( atan2( break17_g3.x , break17_g3.y ) * ( 1.0 / 6.28318548202515 ) * _PolarLenghtScale )));
			float2 panner98 = ( 1.0 * _Time.y * appendResult99 + (( _Radial )?( appendResult23_g3 ):( uv_WavesA )));
			float2 appendResult100 = (float2(_NormalPanningSpeeds.z , _NormalPanningSpeeds.w));
			float2 uv_WavesB = v.texcoord.xy * _WavesB_ST.xy + _WavesB_ST.zw;
			float2 CenteredUV15_g4 = ( uv_WavesB - _PolarCenter );
			float2 break17_g4 = CenteredUV15_g4;
			float2 appendResult23_g4 = (float2(( length( CenteredUV15_g4 ) * _PolarRadialScale * 2.0 ) , ( atan2( break17_g4.x , break17_g4.y ) * ( 1.0 / 6.28318548202515 ) * _PolarLenghtScale )));
			float2 panner97 = ( 1.0 * _Time.y * appendResult100 + (( _Radial )?( appendResult23_g4 ):( uv_WavesB )));
			float3 WavesNormal146 = ( UnpackScaleNormal( tex2Dlod( _WavesA, float4( panner98, 0, 0.0) ), _NormalStrength ) + UnpackScaleNormal( tex2Dlod( _WavesB, float4( panner97, 0, 0.0) ), _NormalStrength ) );
			float3 appendResult138 = (float3(0.0 , ( ( _HeightAmplitude * sin( mulTime128 ) ) + ( WavesNormal146.y * _WaveDisplacementFactor ) ) , 0.0));
			float3 VertexOffset149 = appendResult138;
			v.vertex.xyz += VertexOffset149;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult99 = (float2(_NormalPanningSpeeds.x , _NormalPanningSpeeds.y));
			float2 uv_WavesA = i.uv_texcoord * _WavesA_ST.xy + _WavesA_ST.zw;
			float2 CenteredUV15_g3 = ( uv_WavesA - _PolarCenter );
			float2 break17_g3 = CenteredUV15_g3;
			float2 appendResult23_g3 = (float2(( length( CenteredUV15_g3 ) * _PolarRadialScale * 2.0 ) , ( atan2( break17_g3.x , break17_g3.y ) * ( 1.0 / 6.28318548202515 ) * _PolarLenghtScale )));
			float2 panner98 = ( 1.0 * _Time.y * appendResult99 + (( _Radial )?( appendResult23_g3 ):( uv_WavesA )));
			float2 appendResult100 = (float2(_NormalPanningSpeeds.z , _NormalPanningSpeeds.w));
			float2 uv_WavesB = i.uv_texcoord * _WavesB_ST.xy + _WavesB_ST.zw;
			float2 CenteredUV15_g4 = ( uv_WavesB - _PolarCenter );
			float2 break17_g4 = CenteredUV15_g4;
			float2 appendResult23_g4 = (float2(( length( CenteredUV15_g4 ) * _PolarRadialScale * 2.0 ) , ( atan2( break17_g4.x , break17_g4.y ) * ( 1.0 / 6.28318548202515 ) * _PolarLenghtScale )));
			float2 panner97 = ( 1.0 * _Time.y * appendResult100 + (( _Radial )?( appendResult23_g4 ):( uv_WavesB )));
			float3 WavesNormal146 = ( UnpackScaleNormal( tex2D( _WavesA, panner98 ), _NormalStrength ) + UnpackScaleNormal( tex2D( _WavesB, panner97 ), _NormalStrength ) );
			o.Normal = WavesNormal146;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth93 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth93 = saturate( abs( ( screenDepth93 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FogThreshold ) ) );
			float4 lerpResult71 = lerp( _IntersectionColor , _FogColor , distanceDepth93);
			float screenDepth94 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth94 = saturate( abs( ( screenDepth94 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _IntersectionThreshold ) ) );
			float4 lerpResult70 = lerp( lerpResult71 , _Color , distanceDepth94);
			float4 Albedo154 = lerpResult70;
			o.Albedo = Albedo154.rgb;
			float2 panner111 = ( 1.0 * _Time.y * _FoamTextureSpeed + i.uv_texcoord);
			float simpleNoise173 = SimpleNoise( panner111*_FoamNoiseScale );
			float screenDepth109 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth109 = saturate( abs( ( screenDepth109 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FoamThreshold ) ) );
			float FoamDepth118 = distanceDepth109;
			float temp_output_120_0 = ( FoamDepth118 - ( saturate( sin( ( ( 4.0 * UNITY_PI ) * ( FoamDepth118 - ( _Time.y * _FoamLinesSpeed ) ) ) ) ) * ( 1.0 - FoamDepth118 ) ) );
			float lerpResult209 = lerp( step( simpleNoise173 , temp_output_120_0 ) , 1.0 , ( 1.0 - _HardFoam ));
			float lerpResult211 = lerp( temp_output_120_0 , 1.0 , ( 1.0 - _SoftFoam ));
			float lerpResult193 = lerp( simpleNoise173 , 0.0 , ( lerpResult209 * lerpResult211 ));
			float4 Emission144 = ( lerpResult193 * _FoamColor );
			o.Emission = Emission144.rgb;
			o.Smoothness = _Smoothness;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float fresnelNdotV95 = dot( ase_normWorldNormal, ase_worldViewDir );
			float fresnelNode95 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV95, _FresnelOpacityPower ) );
			float lerpResult198 = lerp( ( Albedo154.a * fresnelNode95 ) , 0.0 , lerpResult193);
			float lerpResult79 = lerp( lerpResult198 , _Color.a , distanceDepth94);
			float Opacity166 = lerpResult79;
			o.Alpha = Opacity166;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1536;0;1920;1019;2387.95;-321.6704;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;165;327.0965,510.1439;Inherit;False;841;279.4;;3;118;109;51;FOAM DEPTH;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;51;405.3873,637.4705;Inherit;False;Property;_FoamThreshold;Foam Threshold;7;1;[Header];Create;True;1;FOAM;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;109;616.8162,618.7278;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;160;-2766.213,656.1439;Inherit;False;2799.123;937.4301;;33;206;144;207;203;126;178;200;193;201;120;173;111;157;179;122;159;110;121;114;115;158;117;116;125;123;119;61;124;208;209;210;211;212;EMISSION;1,0.903649,0.6462264,0.3137255;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;893.4674,614.285;Inherit;False;FoamDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;124;-2670.293,1319.367;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-2691.447,1410.467;Inherit;False;Property;_FoamLinesSpeed;Foam Lines Speed;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-2457.542,1345.625;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2493.317,1216.617;Inherit;False;118;FoamDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;116;-2357.131,1123.615;Inherit;False;1;0;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;125;-2313.61,1274.828;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-2146.999,1198.616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;164;-2360.574,1671.94;Inherit;False;2400.829;860.4846;;19;146;69;55;56;98;99;57;100;106;107;163;162;161;105;108;102;54;97;180;WAVES NORMAL;1,0.5424528,0.5424528,1;0;0
Node;AmplifyShaderEditor.SinOpNode;115;-1987.844,1199.111;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-2046.86,1331.033;Inherit;False;118;FoamDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-2268.584,2159.752;Inherit;False;Property;_PolarLenghtScale;Polar Lenght Scale;20;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;161;-2149.845,2312.648;Inherit;False;0;55;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;180;-2234.929,1933.546;Inherit;False;Property;_PolarCenter;Polar Center;18;1;[Header];Create;True;1;RADIAL;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;102;-2163.721,1760.804;Inherit;False;0;54;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;106;-2260.992,2069.799;Inherit;False;Property;_PolarRadialScale;Polar Radial Scale;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;110;-2113.796,779.602;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;159;-2114.57,923.209;Inherit;False;Property;_FoamTextureSpeed;Foam Texture Speed;8;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SaturateNode;114;-1836.564,1198.65;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;121;-1850.486,1335.818;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;57;-1546.22,1953.104;Inherit;False;Property;_NormalPanningSpeeds;Normal Panning Speeds;17;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;156;-2194.882,-975.8456;Inherit;False;1843.526;822.4945;;11;154;70;94;143;71;48;47;45;93;50;49;ALBEDO;0.5093005,0.990566,0.589829,1;0;0
Node;AmplifyShaderEditor.FunctionNode;105;-1898.201,1912.843;Inherit;False;Polar Coordinates;-1;;3;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;163;-1889.586,2152.766;Inherit;False;Polar Coordinates;-1;;4;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;99;-1260.921,1945.605;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;162;-1624.905,2313.929;Inherit;False;Property;_Radial;Radial;23;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;108;-1643.92,1756.804;Inherit;False;Property;_Radial;Radial;17;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;100;-1260.921,2048.704;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2030.681,-484.2359;Inherit;False;Property;_FogThreshold;Fog Threshold;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;111;-1870.995,825.462;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1872.398,980.5507;Inherit;False;Property;_FoamNoiseScale;Foam Noise Scale;9;0;Create;True;0;0;0;False;0;False;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;157;-1711.054,1069.227;Inherit;False;118;FoamDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;-1665.218,1261.112;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;97;-1040.221,2102.504;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DepthFade;93;-1809.273,-504.1567;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1036.821,1996.705;Inherit;False;Property;_NormalStrength;Normal Strength;16;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;98;-1048.521,1858.304;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1341.366,-495.9882;Inherit;False;Property;_IntersectionThreshold;Intersection Threshold;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;48;-1827.969,-885.1098;Inherit;False;Property;_IntersectionColor;Intersection Color;5;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;207;-1316.388,886.0518;Inherit;False;Property;_HardFoam;Hard Foam;11;0;Create;True;0;0;0;False;0;False;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;47;-1594.444,-384.6756;Inherit;False;Property;_Color;Color;0;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;120;-1512.151,1156.685;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;206;-1493.4,1287.38;Inherit;False;Property;_SoftFoam;Soft Foam;12;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;173;-1658.471,819.567;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;45;-1821.719,-689.7307;Inherit;False;Property;_FogColor;Fog Color;3;2;[HDR];[Header];Create;True;1;FOG;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;55;-768.6207,2035.505;Inherit;True;Property;_WavesB;Waves B;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;54;-768.2207,1814.504;Inherit;True;Property;_WavesA;Waves A;14;1;[Header];Create;True;1;WAVES;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;201;-1362.136,989.8199;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;143;-1374.768,-539.0413;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;212;-1205.601,1292.121;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;71;-1482.338,-710.9242;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;210;-1198.45,972.9704;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;94;-1101.094,-514.9689;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-406.021,1942.004;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;211;-1028.8,1212.82;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;209;-1030.75,1039.27;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;151;-2039.565,2692.251;Inherit;False;1396.1;534.2;;12;149;138;137;135;134;136;148;130;131;129;128;127;VERTEX OFFSET;0.7371854,0.6084906,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;70;-815.3878,-702.7285;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-586.965,-706.6156;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;-837.2983,1121.166;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;208;-766.6582,886.6625;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;168;-2029.25,-10.92709;Inherit;False;1494.793;481.7027;;11;166;152;80;65;95;79;96;169;170;198;199;OPACITY;0.7216981,0.9911932,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;127;-1991.061,2830.408;Inherit;False;Property;_HeighSpeed;HeighSpeed;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;-238.6209,1938.105;Inherit;False;WavesNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;128;-1788.061,2835.408;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-1802.093,52.31717;Inherit;False;154;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-1967.472,340.3475;Inherit;False;Property;_FresnelOpacityPower;Fresnel Opacity Power;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;193;-631.1999,1023.966;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;-1798.729,2980.219;Inherit;False;146;WavesNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinOpNode;129;-1575.061,2835.408;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;200;-489.6171,991.2463;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-1642.061,2745.407;Inherit;False;Property;_HeightAmplitude;HeightAmplitude;21;1;[Header];Create;True;1;DISPLACEMENT;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;80;-1616.791,59.95282;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FresnelNode;95;-1711.437,247.0146;Inherit;False;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;-1697.341,3119.809;Inherit;False;Property;_WaveDisplacementFactor;Wave Displacement Factor;24;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;136;-1569.096,2985.74;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;169;-660.6804,307.7314;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-1453.15,188.7728;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;199;-1339.261,328.0267;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-1398.061,2779.408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-1394.791,3052.553;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;170;-1074.409,379.4616;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;137;-1192.03,2912.406;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;198;-1278.923,192.0087;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;178;-623.6397,1191.395;Inherit;False;Property;_FoamColor;Foam Color;13;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;138;-1039.919,2888.92;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;79;-1074.605,193.4014;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-379.9056,1023.431;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;144;-203.7187,1016.083;Inherit;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-851.0512,187.5034;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-863.9705,2886.007;Inherit;False;VertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;562.5264,1353.538;Inherit;False;149;VertexOffset;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;147;555.0278,1018.886;Inherit;False;146;WavesNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;145;583.6043,1100.006;Inherit;False;144;Emission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;167;582.4002,1269.068;Inherit;False;166;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;576.1065,932.9052;Inherit;False;154;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;63;586.6277,1183.725;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;881.9326,1003.144;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Mameshiba Games/Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Premultiply;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;3;1;False;-1;10;False;-1;3;1;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;109;0;51;0
WireConnection;118;0;109;0
WireConnection;123;0;124;0
WireConnection;123;1;61;0
WireConnection;125;0;119;0
WireConnection;125;1;123;0
WireConnection;117;0;116;0
WireConnection;117;1;125;0
WireConnection;115;0;117;0
WireConnection;114;0;115;0
WireConnection;121;0;158;0
WireConnection;105;1;102;0
WireConnection;105;2;180;0
WireConnection;105;3;106;0
WireConnection;105;4;107;0
WireConnection;163;1;161;0
WireConnection;163;2;180;0
WireConnection;163;3;106;0
WireConnection;163;4;107;0
WireConnection;99;0;57;1
WireConnection;99;1;57;2
WireConnection;162;0;161;0
WireConnection;162;1;163;0
WireConnection;108;0;102;0
WireConnection;108;1;105;0
WireConnection;100;0;57;3
WireConnection;100;1;57;4
WireConnection;111;0;110;0
WireConnection;111;2;159;0
WireConnection;122;0;114;0
WireConnection;122;1;121;0
WireConnection;97;0;162;0
WireConnection;97;2;100;0
WireConnection;93;0;50;0
WireConnection;98;0;108;0
WireConnection;98;2;99;0
WireConnection;120;0;157;0
WireConnection;120;1;122;0
WireConnection;173;0;111;0
WireConnection;173;1;179;0
WireConnection;55;1;97;0
WireConnection;55;5;56;0
WireConnection;54;1;98;0
WireConnection;54;5;56;0
WireConnection;201;0;173;0
WireConnection;201;1;120;0
WireConnection;143;0;47;0
WireConnection;212;0;206;0
WireConnection;71;0;48;0
WireConnection;71;1;45;0
WireConnection;71;2;93;0
WireConnection;210;0;207;0
WireConnection;94;0;49;0
WireConnection;69;0;54;0
WireConnection;69;1;55;0
WireConnection;211;0;120;0
WireConnection;211;2;212;0
WireConnection;209;0;201;0
WireConnection;209;2;210;0
WireConnection;70;0;71;0
WireConnection;70;1;143;0
WireConnection;70;2;94;0
WireConnection;154;0;70;0
WireConnection;203;0;209;0
WireConnection;203;1;211;0
WireConnection;208;0;173;0
WireConnection;146;0;69;0
WireConnection;128;0;127;0
WireConnection;193;0;208;0
WireConnection;193;2;203;0
WireConnection;129;0;128;0
WireConnection;200;0;193;0
WireConnection;80;0;152;0
WireConnection;95;3;65;0
WireConnection;136;0;148;0
WireConnection;169;0;94;0
WireConnection;96;0;80;3
WireConnection;96;1;95;0
WireConnection;199;0;200;0
WireConnection;131;0;130;0
WireConnection;131;1;129;0
WireConnection;135;0;136;1
WireConnection;135;1;134;0
WireConnection;170;0;169;0
WireConnection;137;0;131;0
WireConnection;137;1;135;0
WireConnection;198;0;96;0
WireConnection;198;2;199;0
WireConnection;138;1;137;0
WireConnection;79;0;198;0
WireConnection;79;1;47;4
WireConnection;79;2;170;0
WireConnection;126;0;193;0
WireConnection;126;1;178;0
WireConnection;144;0;126;0
WireConnection;166;0;79;0
WireConnection;149;0;138;0
WireConnection;0;0;155;0
WireConnection;0;1;147;0
WireConnection;0;2;145;0
WireConnection;0;4;63;0
WireConnection;0;9;167;0
WireConnection;0;11;150;0
ASEEND*/
//CHKSM=DB4D800E05D9702083134773D79C05E7A7F9821D