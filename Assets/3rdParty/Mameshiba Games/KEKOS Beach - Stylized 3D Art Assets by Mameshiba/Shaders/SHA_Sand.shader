// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Sand"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (0.7254902,0.6509804,0.5294118,1)
		_MinLightIntensity("Min Light Intensity", Float) = 0
		_MaxLightIntensity("Max Light Intensity", Float) = 1
		[SingleLineTexture]_BumpMap1("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Scale", Float) = 1
		[Toggle]_TriplanarNormal("Triplanar Normal", Float) = 1
		_TriplanarNormalSize("Triplanar Normal Size", Vector) = (1,1,0,0)
		[HDR][Header(Rim Light Specular)]_RimLightColor("Rim Light Color", Color) = (23.96863,8.309601,0,1)
		_Bias("Bias", Float) = 0
		_Scale("Scale", Float) = 0.3
		_Power("Power", Float) = 6
		_BaseAndSparkleContribution("Base And Sparkle Contribution", Float) = 3
		[Header(Sparkle)][SingleLineTexture]_SparkleTexture("Sparkle Texture", 2D) = "white" {}
		_SparkleColor("Sparkle Color", Color) = (1,0.9108636,0.7783019,1)
		_SparkleIntensity("Sparkle Intensity", Float) = 1
		_SparkleSize("Sparkle Size", Range( 0 , 10)) = 1
		_SparkleCameraNoise("Sparkle Camera Noise", Float) = 0
		[Header(Shore)]_WetSandColor("Wet Sand Color", Color) = (0.4245283,0.2140283,0.1021271,1)
		_ShoreHeighSpeed("Shore Heigh Speed", Float) = 1
		_ShoreHeightAmplitude("Shore Height Amplitude", Float) = 0.5
		_ShoreHeightAdjustment("Shore Height Adjustment", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform float _TriplanarNormal;
		uniform sampler2D _BumpMap1;
		uniform float4 _BumpMap1_ST;
		uniform float _BumpScale;
		uniform float2 _TriplanarNormalSize;
		uniform float4 _BaseColor;
		uniform float _MinLightIntensity;
		uniform float _MaxLightIntensity;
		uniform float4 _WetSandColor;
		uniform float _ShoreHeightAmplitude;
		uniform float _ShoreHeighSpeed;
		uniform float _ShoreHeightAdjustment;
		uniform sampler2D _SparkleTexture;
		uniform float _SparkleSize;
		uniform float _SparkleCameraNoise;
		uniform float _SparkleIntensity;
		uniform float4 _SparkleColor;
		uniform float _BaseAndSparkleContribution;
		uniform float4 _RimLightColor;
		uniform float _Bias;
		uniform float _Scale;
		uniform float _Power;


		inline float3 TriplanarSampling141( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
		{
			float2 vertex1, vertex2, vertex3;
			// Scaling of the input
			float2 uv = UV * 3.464; // 2 * sqrt (3)
			// Skew input space into simplex triangle grid
			const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
			float2 skewedCoord = mul( gridToSkewedGrid, uv );
			// Compute local triangle vertex IDs and local barycentric coordinates
			int2 baseId = int2( floor( skewedCoord ) );
			float3 temp = float3( frac( skewedCoord ), 0 );
			temp.z = 1.0 - temp.x - temp.y;
			if ( temp.z > 0.0 )
			{
				W1 = temp.z;
				W2 = temp.y;
				W3 = temp.x;
				vertex1 = baseId;
				vertex2 = baseId + int2( 0, 1 );
				vertex3 = baseId + int2( 1, 0 );
			}
			else
			{
				W1 = -temp.z;
				W2 = 1.0 - temp.y;
				W3 = 1.0 - temp.x;
				vertex1 = baseId + int2( 1, 1 );
				vertex2 = baseId + int2( 1, 0 );
				vertex3 = baseId + int2( 0, 1 );
			}
			UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
			UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
			UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
			return;
		}


		void TriplanarWeights( float3 WorldNormal, out float W0, out float W1, out float W2 )
		{
			half3 weights = max( abs( WorldNormal.xyz ), 0.000001 );
			weights /= ( weights.x + weights.y + weights.z ).xxx;
			W0 = weights.x;
			W1 = weights.y;
			W2 = weights.z;
			return;
		}


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_BumpMap1 = i.uv_texcoord * _BumpMap1_ST.xy + _BumpMap1_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 triplanar141 = TriplanarSampling141( _BumpMap1, ase_worldPos, ase_worldNormal, 1.0, _TriplanarNormalSize, _BumpScale, 0 );
			float3 tanTriplanarNormal141 = mul( ase_worldToTangent, triplanar141 );
			float3 NormalOutput145 = (( _TriplanarNormal )?( tanTriplanarNormal141 ):( UnpackScaleNormal( tex2D( _BumpMap1, uv_BumpMap1 ), _BumpScale ) ));
			o.Normal = NormalOutput145;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float mulTime163 = _Time.y * _ShoreHeighSpeed;
			float smoothstepResult170 = smoothstep( 0.0 , 0.5 , ( ( ase_worldPos.y + ( _ShoreHeightAmplitude * sin( ( mulTime163 + -2.0 ) ) ) ) + _ShoreHeightAdjustment ));
			float4 lerpResult153 = lerp( _WetSandColor , float4( 1,1,1,1 ) , saturate( smoothstepResult170 ));
			float4 WaterShore155 = lerpResult153;
			o.Emission = ( ( _BaseColor * saturate( (0.0 + (ase_lightColor.a - _MinLightIntensity) * (1.0 - 0.0) / (_MaxLightIntensity - _MinLightIntensity)) ) ) * WaterShore155 ).rgb;
			float localStochasticTiling53_g1 = ( 0.0 );
			float2 temp_cast_1 = (_SparkleSize).xx;
			float2 temp_output_104_0_g1 = temp_cast_1;
			float3 temp_output_80_0_g1 = ase_worldPos;
			float2 Triplanar_UV050_g1 = ( temp_output_104_0_g1 * (temp_output_80_0_g1).zy );
			float2 UV53_g1 = Triplanar_UV050_g1;
			float2 UV153_g1 = float2( 0,0 );
			float2 UV253_g1 = float2( 0,0 );
			float2 UV353_g1 = float2( 0,0 );
			float W153_g1 = 0.0;
			float W253_g1 = 0.0;
			float W353_g1 = 0.0;
			StochasticTiling( UV53_g1 , UV153_g1 , UV253_g1 , UV353_g1 , W153_g1 , W253_g1 , W353_g1 );
			float2 temp_output_57_0_g1 = ddx( Triplanar_UV050_g1 );
			float2 temp_output_58_0_g1 = ddy( Triplanar_UV050_g1 );
			float localTriplanarWeights108_g1 = ( 0.0 );
			float3 WorldNormal108_g1 = ase_worldNormal;
			float W0108_g1 = 0.0;
			float W1108_g1 = 0.0;
			float W2108_g1 = 0.0;
			TriplanarWeights( WorldNormal108_g1 , W0108_g1 , W1108_g1 , W2108_g1 );
			float localStochasticTiling83_g1 = ( 0.0 );
			float2 Triplanar_UV164_g1 = ( temp_output_104_0_g1 * (temp_output_80_0_g1).zx );
			float2 UV83_g1 = Triplanar_UV164_g1;
			float2 UV183_g1 = float2( 0,0 );
			float2 UV283_g1 = float2( 0,0 );
			float2 UV383_g1 = float2( 0,0 );
			float W183_g1 = 0.0;
			float W283_g1 = 0.0;
			float W383_g1 = 0.0;
			StochasticTiling( UV83_g1 , UV183_g1 , UV283_g1 , UV383_g1 , W183_g1 , W283_g1 , W383_g1 );
			float2 temp_output_86_0_g1 = ddx( Triplanar_UV164_g1 );
			float2 temp_output_92_0_g1 = ddy( Triplanar_UV164_g1 );
			float localStochasticTiling117_g1 = ( 0.0 );
			float2 Triplanar_UV271_g1 = ( temp_output_104_0_g1 * (temp_output_80_0_g1).xy );
			float2 UV117_g1 = Triplanar_UV271_g1;
			float2 UV1117_g1 = float2( 0,0 );
			float2 UV2117_g1 = float2( 0,0 );
			float2 UV3117_g1 = float2( 0,0 );
			float W1117_g1 = 0.0;
			float W2117_g1 = 0.0;
			float W3117_g1 = 0.0;
			StochasticTiling( UV117_g1 , UV1117_g1 , UV2117_g1 , UV3117_g1 , W1117_g1 , W2117_g1 , W3117_g1 );
			float2 temp_output_107_0_g1 = ddx( Triplanar_UV271_g1 );
			float2 temp_output_110_0_g1 = ddy( Triplanar_UV271_g1 );
			float4 Output_Triplanar295_g1 = ( ( ( ( tex2D( _SparkleTexture, UV153_g1, temp_output_57_0_g1, temp_output_58_0_g1 ) * W153_g1 ) + ( tex2D( _SparkleTexture, UV253_g1, temp_output_57_0_g1, temp_output_58_0_g1 ) * W253_g1 ) + ( tex2D( _SparkleTexture, UV353_g1, temp_output_57_0_g1, temp_output_58_0_g1 ) * W353_g1 ) ) * W0108_g1 ) + ( W1108_g1 * ( ( tex2D( _SparkleTexture, UV183_g1, temp_output_86_0_g1, temp_output_92_0_g1 ) * W183_g1 ) + ( tex2D( _SparkleTexture, UV283_g1, temp_output_86_0_g1, temp_output_92_0_g1 ) * W283_g1 ) + ( tex2D( _SparkleTexture, UV383_g1, temp_output_86_0_g1, temp_output_92_0_g1 ) * W383_g1 ) ) ) + ( W2108_g1 * ( ( tex2D( _SparkleTexture, UV1117_g1, temp_output_107_0_g1, temp_output_110_0_g1 ) * W1117_g1 ) + ( tex2D( _SparkleTexture, UV2117_g1, temp_output_107_0_g1, temp_output_110_0_g1 ) * W2117_g1 ) + ( tex2D( _SparkleTexture, UV3117_g1, temp_output_107_0_g1, temp_output_110_0_g1 ) * W3117_g1 ) ) ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float simplePerlin2D69 = snoise( ase_screenPos.xy*_SparkleCameraNoise );
			simplePerlin2D69 = simplePerlin2D69*0.5 + 0.5;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV77 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode77 = ( _Bias + _Scale * pow( 1.0 - fresnelNdotV77, _Power ) );
			float4 lerpResult99 = lerp( float4( 0,0,0,0 ) , _RimLightColor , saturate( fresnelNode77 ));
			o.Specular = ( ( ( _BaseColor + ( ( ( Output_Triplanar295_g1 * saturate( (-1.0 + (simplePerlin2D69 - 0.0) * (2.0 - -1.0) / (1.0 - 0.0)) ) ) * _SparkleIntensity ) * _SparkleColor ) ) * _BaseAndSparkleContribution ) + lerpResult99 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1536;0;1920;1019;2050.209;605.4592;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;187;-651.1932,658.8665;Inherit;False;2159.241;628.0695;;15;166;164;185;162;163;165;154;167;175;170;150;155;184;153;188;SHORE EFFECT;0.5471698,0.3806092,0.1626024,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;162;-571.496,1118.655;Inherit;False;Property;_ShoreHeighSpeed;Shore Heigh Speed;18;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;163;-326.4958,1123.655;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;122;-2159.253,219.7524;Inherit;False;1300;327.2;;5;118;74;71;70;69;Sparkle Camera Noise;0.6933962,1,0.9968354,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;185;-123.9221,1124.961;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;118;-2020.82,260.4027;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;-1832.895,342.2282;Inherit;False;Property;_SparkleCameraNoise;Sparkle Camera Noise;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;164;4.504218,1125.655;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-102.4958,1028.655;Inherit;False;Property;_ShoreHeightAmplitude;Shore Height Amplitude;19;0;Create;True;0;0;0;False;0;False;0.5;-0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;177.5042,1071.655;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;154;24.12039,858.8772;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;3;-1569.114,-349.1442;Inherit;False;1519.741;530.8874;;7;14;13;12;10;88;90;91;Sparkle;1,1,1,1;0;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;69;-1573.892,262.4222;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;167;323.1041,1010.855;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-1479.492,-74.9203;Inherit;False;Property;_SparkleSize;Sparkle Size;15;0;Create;True;0;0;0;False;0;False;1;0.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;90;-1539.369,-292.8567;Inherit;True;Property;_SparkleTexture;Sparkle Texture;12;2;[Header];[SingleLineTexture];Create;True;1;Sparkle;0;0;False;0;False;None;796ec3b43a1a521448244de0b2421746;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TFHCRemapNode;71;-1294.117,268.9204;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;188;222.0151,1180.965;Inherit;False;Property;_ShoreHeightAdjustment;Shore Height Adjustment;20;0;Create;True;0;0;0;False;0;False;0;0.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;175;486.8802,1011.22;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;74;-1072.002,271.3818;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;88;-1180.38,-292.5467;Inherit;False;Procedural Sample;-1;;1;f5379ff72769e2b4495e5ce2f004d8d4;2,157,2,315,2;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,1;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-856.4309,-287.3716;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;147;1023.437,-821.9051;Inherit;False;1448.876;673.6324;;7;145;142;143;134;136;141;133;Normal Map;0.789021,0.4103774,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-751.3214,-152.7379;Float;False;Property;_SparkleIntensity;Sparkle Intensity;14;0;Create;True;0;0;0;False;0;False;1;16.22;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;170;670.1042,1011.213;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-280.9196,480.5038;Inherit;False;Property;_Power;Power;10;0;Create;True;0;0;0;False;0;False;6;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-521.1009,-140.2276;Float;False;Property;_SparkleColor;Sparkle Color;13;0;Create;True;0;0;0;False;0;False;1,0.9108636,0.7783019,1;1,0.9108636,0.7783019,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-514.9056,-286.996;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-284.2491,396.3618;Inherit;False;Property;_Scale;Scale;9;0;Create;True;0;0;0;False;0;False;0.3;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-60.35005,-500.1234;Inherit;False;Property;_MaxLightIntensity;Max Light Intensity;2;0;Create;True;0;0;0;False;0;False;1;3.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;129;-16.98797,-723.644;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SaturateNode;184;855.0779,1010.32;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-287.7586,301.2516;Inherit;False;Property;_Bias;Bias;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;150;657.7521,777.2267;Inherit;False;Property;_WetSandColor;Wet Sand Color;17;1;[Header];Create;True;1;Shore;0;0;False;0;False;0.4245283,0.2140283,0.1021271,1;0.4245283,0.2140283,0.1021271,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;130;-61.64997,-588.5236;Inherit;False;Property;_MinLightIntensity;Min Light Intensity;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;1142.156,-457.7795;Inherit;False;Property;_BumpScale;Normal Scale;4;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;136;1091.642,-683.5178;Inherit;True;Property;_BumpMap1;Normal Map;3;1;[SingleLineTexture];Create;False;0;0;0;False;0;False;None;f8736fabb1ee2e44db73d9bec56b4653;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Vector2Node;142;1104.284,-353.5249;Inherit;False;Property;_TriplanarNormalSize;Triplanar Normal Size;6;0;Create;True;0;0;0;False;0;False;1,1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-273.7154,-288.4651;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;141;1501.925,-442.2107;Inherit;True;Spherical;World;True;Top Texture 0;_TopTexture0;white;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;131;243.8503,-678.2236;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-678.7348,-816.9679;Inherit;False;Property;_BaseColor;Base Color;0;0;Create;True;0;0;0;False;0;False;0.7254902,0.6509804,0.5294118,1;0.7254902,0.6509804,0.5294118,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;77;-46.56667,326.6491;Inherit;False;Standard;TangentNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;153;1033.12,870.236;Inherit;False;3;0;COLOR;1,1,1,1;False;1;COLOR;1,1,1,1;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;133;1504.18,-678.4591;Inherit;True;Property;_Normal;Normal;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;96;216.5227,326.6198;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;146.7665,-308.2864;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;132;444.4428,-678.8906;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;143;2004.183,-545.8584;Inherit;False;Property;_TriplanarNormal;Triplanar Normal;5;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;114;59.28899,-181.6389;Inherit;False;Property;_BaseAndSparkleContribution;Base And Sparkle Contribution;11;0;Create;True;0;0;0;False;0;False;3;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;87;121.1077,127.5806;Inherit;False;Property;_RimLightColor;Rim Light Color;7;2;[HDR];[Header];Create;True;1;Rim Light Specular;0;0;False;0;False;23.96863,8.309601,0,1;0.0009651729,0.0003385685,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;155;1245.297,864.2618;Inherit;False;WaterShore;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;99;405.55,108.0228;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;156;552.1493,-472.4723;Inherit;False;155;WaterShore;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;631.9973,-795.9192;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;368.4696,-305.3271;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;145;2239.691,-545.6304;Inherit;False;NormalOutput;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;146;589.4203,-188.9803;Inherit;False;145;NormalOutput;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;152;783.0707,-597.7796;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;112;672.4748,86.52695;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;863.0948,7.27712;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Mameshiba Games/Sand;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;163;0;162;0
WireConnection;185;0;163;0
WireConnection;164;0;185;0
WireConnection;166;0;165;0
WireConnection;166;1;164;0
WireConnection;69;0;118;0
WireConnection;69;1;70;0
WireConnection;167;0;154;2
WireConnection;167;1;166;0
WireConnection;71;0;69;0
WireConnection;175;0;167;0
WireConnection;175;1;188;0
WireConnection;74;0;71;0
WireConnection;88;82;90;0
WireConnection;88;104;91;0
WireConnection;55;0;88;0
WireConnection;55;1;74;0
WireConnection;170;0;175;0
WireConnection;13;0;55;0
WireConnection;13;1;10;0
WireConnection;184;0;170;0
WireConnection;14;0;13;0
WireConnection;14;1;12;0
WireConnection;141;0;136;0
WireConnection;141;8;134;0
WireConnection;141;3;142;0
WireConnection;131;0;129;2
WireConnection;131;1;130;0
WireConnection;131;2;128;0
WireConnection;77;1;83;0
WireConnection;77;2;84;0
WireConnection;77;3;85;0
WireConnection;153;0;150;0
WireConnection;153;2;184;0
WireConnection;133;0;136;0
WireConnection;133;5;134;0
WireConnection;96;0;77;0
WireConnection;15;0;1;0
WireConnection;15;1;14;0
WireConnection;132;0;131;0
WireConnection;143;0;133;0
WireConnection;143;1;141;0
WireConnection;155;0;153;0
WireConnection;99;1;87;0
WireConnection;99;2;96;0
WireConnection;115;0;1;0
WireConnection;115;1;132;0
WireConnection;113;0;15;0
WireConnection;113;1;114;0
WireConnection;145;0;143;0
WireConnection;152;0;115;0
WireConnection;152;1;156;0
WireConnection;112;0;113;0
WireConnection;112;1;99;0
WireConnection;0;1;146;0
WireConnection;0;2;152;0
WireConnection;0;3;112;0
ASEEND*/
//CHKSM=1F706735B11E3F12D2506A238908194739C44082