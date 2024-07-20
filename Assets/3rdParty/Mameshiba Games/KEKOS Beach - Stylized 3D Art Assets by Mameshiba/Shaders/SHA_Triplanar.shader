// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Standard_Triplanar"
{
	Properties
	{
		[SingleLineTexture]_MainTex("Main Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		[SingleLineTexture]_MetallicGlossMap("Metallic Texture", 2D) = "black" {}
		_SmoothnessTextureChannel("Smoothness", Range( 0 , 1)) = 1
		[Normal][SingleLineTexture]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1
		_Tiling("Tiling", Vector) = (1,1,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		[Header(Secondary Maps)][SingleLineTexture]_DetailAlbedoMap("Detail Albedo", 2D) = "gray" {}
		[Normal][SingleLineTexture]_DetailNormalMap("Normal Map", 2D) = "bump" {}
		_DetailNormalMapScale("Bump Scale", Float) = 1
		_DetailTiling("Tiling", Vector) = (1,1,0,0)
		_DetailOffset("Offset", Vector) = (0,0,0,0)
		[Header(Triplanar)]_Falloff("Falloff", Range( 1 , 200)) = 3
		[SingleLineTexture]_TriplanarAlbedo("Triplanar Albedo", 2D) = "white" {}
		[SingleLineTexture]_TriplanarMetallic("Triplanar Metallic", 2D) = "black" {}
		_TriplanarSmoothness("Triplanar Smoothness", Range( 0 , 1)) = 1
		[Normal][SingleLineTexture]_TriplanarNormalMap("Normal Map", 2D) = "bump" {}
		[HideInInspector]_Black("Black", 2D) = "black" {}
		_TriplanarNormalMapScale("Bump Scale", Float) = 1
		_TriplanarTiling("Tiling", Vector) = (1,1,0,0)
		[HideInInspector]_White("White", 2D) = "white" {}
		_TriplanarOffset("Offset", Vector) = (0,0,0,0)
		[SingleLineTexture]_TopPattern("Top Pattern", 2D) = "white" {}
		_TopSize("Top Size", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
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
		};

		uniform sampler2D _DetailNormalMap;
		uniform float2 _DetailTiling;
		uniform float2 _DetailOffset;
		uniform float _DetailNormalMapScale;
		uniform sampler2D _BumpMap;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _BumpScale;
		uniform sampler2D _TriplanarNormalMap;
		uniform float2 _TriplanarTiling;
		uniform float2 _TriplanarOffset;
		uniform float _TriplanarNormalMapScale;
		uniform sampler2D _White;
		uniform sampler2D _Black;
		uniform float _Falloff;
		uniform sampler2D _TopPattern;
		uniform float _TopSize;
		uniform sampler2D _DetailAlbedoMap;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _TriplanarAlbedo;
		uniform sampler2D _MetallicGlossMap;
		uniform sampler2D _TriplanarMetallic;
		uniform float _SmoothnessTextureChannel;
		uniform float _TriplanarSmoothness;


		inline float4 TriplanarSampling26( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm  = tex2D( midTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm  = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			yNormN = tex2D( botTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm  = tex2D( midTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord25_g28 = i.uv_texcoord * _DetailTiling + _DetailOffset;
			float2 uv_TexCoord18_g28 = i.uv_texcoord * _Tiling + _Offset;
			float3 tex2DNode2_g28 = UnpackScaleNormal( tex2D( _BumpMap, uv_TexCoord18_g28 ), _BumpScale );
			float2 uv_TexCoord43 = i.uv_texcoord * _TriplanarTiling + _TriplanarOffset;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar26 = TriplanarSampling26( _White, _Black, _Black, ase_worldPos, ase_worldNormal, _Falloff, float2( 1,1 ), float3( 1,1,1 ), float3(0,0,0) );
			float localStochasticTiling53_g25 = ( 0.0 );
			float2 temp_cast_0 = (_TopSize).xx;
			float2 temp_output_104_0_g25 = temp_cast_0;
			float3 temp_output_80_0_g25 = ase_worldPos;
			float2 Triplanar_UV050_g25 = ( temp_output_104_0_g25 * (temp_output_80_0_g25).zy );
			float2 UV53_g25 = Triplanar_UV050_g25;
			float2 UV153_g25 = float2( 0,0 );
			float2 UV253_g25 = float2( 0,0 );
			float2 UV353_g25 = float2( 0,0 );
			float W153_g25 = 0.0;
			float W253_g25 = 0.0;
			float W353_g25 = 0.0;
			StochasticTiling( UV53_g25 , UV153_g25 , UV253_g25 , UV353_g25 , W153_g25 , W253_g25 , W353_g25 );
			float2 temp_output_57_0_g25 = ddx( Triplanar_UV050_g25 );
			float2 temp_output_58_0_g25 = ddy( Triplanar_UV050_g25 );
			float localTriplanarWeights108_g25 = ( 0.0 );
			float3 WorldNormal108_g25 = ase_worldNormal;
			float W0108_g25 = 0.0;
			float W1108_g25 = 0.0;
			float W2108_g25 = 0.0;
			TriplanarWeights( WorldNormal108_g25 , W0108_g25 , W1108_g25 , W2108_g25 );
			float localStochasticTiling83_g25 = ( 0.0 );
			float2 Triplanar_UV164_g25 = ( temp_output_104_0_g25 * (temp_output_80_0_g25).zx );
			float2 UV83_g25 = Triplanar_UV164_g25;
			float2 UV183_g25 = float2( 0,0 );
			float2 UV283_g25 = float2( 0,0 );
			float2 UV383_g25 = float2( 0,0 );
			float W183_g25 = 0.0;
			float W283_g25 = 0.0;
			float W383_g25 = 0.0;
			StochasticTiling( UV83_g25 , UV183_g25 , UV283_g25 , UV383_g25 , W183_g25 , W283_g25 , W383_g25 );
			float2 temp_output_86_0_g25 = ddx( Triplanar_UV164_g25 );
			float2 temp_output_92_0_g25 = ddy( Triplanar_UV164_g25 );
			float localStochasticTiling117_g25 = ( 0.0 );
			float2 Triplanar_UV271_g25 = ( temp_output_104_0_g25 * (temp_output_80_0_g25).xy );
			float2 UV117_g25 = Triplanar_UV271_g25;
			float2 UV1117_g25 = float2( 0,0 );
			float2 UV2117_g25 = float2( 0,0 );
			float2 UV3117_g25 = float2( 0,0 );
			float W1117_g25 = 0.0;
			float W2117_g25 = 0.0;
			float W3117_g25 = 0.0;
			StochasticTiling( UV117_g25 , UV1117_g25 , UV2117_g25 , UV3117_g25 , W1117_g25 , W2117_g25 , W3117_g25 );
			float2 temp_output_107_0_g25 = ddx( Triplanar_UV271_g25 );
			float2 temp_output_110_0_g25 = ddy( Triplanar_UV271_g25 );
			float4 Output_Triplanar295_g25 = ( ( ( ( tex2D( _TopPattern, UV153_g25, temp_output_57_0_g25, temp_output_58_0_g25 ) * W153_g25 ) + ( tex2D( _TopPattern, UV253_g25, temp_output_57_0_g25, temp_output_58_0_g25 ) * W253_g25 ) + ( tex2D( _TopPattern, UV353_g25, temp_output_57_0_g25, temp_output_58_0_g25 ) * W353_g25 ) ) * W0108_g25 ) + ( W1108_g25 * ( ( tex2D( _TopPattern, UV183_g25, temp_output_86_0_g25, temp_output_92_0_g25 ) * W183_g25 ) + ( tex2D( _TopPattern, UV283_g25, temp_output_86_0_g25, temp_output_92_0_g25 ) * W283_g25 ) + ( tex2D( _TopPattern, UV383_g25, temp_output_86_0_g25, temp_output_92_0_g25 ) * W383_g25 ) ) ) + ( W2108_g25 * ( ( tex2D( _TopPattern, UV1117_g25, temp_output_107_0_g25, temp_output_110_0_g25 ) * W1117_g25 ) + ( tex2D( _TopPattern, UV2117_g25, temp_output_107_0_g25, temp_output_110_0_g25 ) * W2117_g25 ) + ( tex2D( _TopPattern, UV3117_g25, temp_output_107_0_g25, temp_output_110_0_g25 ) * W3117_g25 ) ) ) );
			float4 temp_output_58_0 = ( triplanar26 * Output_Triplanar295_g25 );
			float3 lerpResult31 = lerp( BlendNormals( UnpackScaleNormal( tex2D( _DetailNormalMap, uv_TexCoord25_g28 ), _DetailNormalMapScale ) , tex2DNode2_g28 ) , UnpackScaleNormal( tex2D( _TriplanarNormalMap, uv_TexCoord43 ), _TriplanarNormalMapScale ) , temp_output_58_0.xyz);
			o.Normal = lerpResult31;
			float4 tex2DNode1_g28 = tex2D( _MainTex, uv_TexCoord18_g28 );
			float4 lerpResult27 = lerp( ( ( tex2D( _DetailAlbedoMap, uv_TexCoord25_g28 ) * unity_ColorSpaceDouble ) * _Color * tex2DNode1_g28 ) , tex2D( _TriplanarAlbedo, uv_TexCoord43 ) , temp_output_58_0);
			o.Albedo = lerpResult27.rgb;
			float4 tex2DNode5_g28 = tex2D( _MetallicGlossMap, uv_TexCoord18_g28 );
			float4 tex2DNode46 = tex2D( _TriplanarMetallic, uv_TexCoord43 );
			float lerpResult45 = lerp( tex2DNode5_g28.r , tex2DNode46.r , temp_output_58_0.x);
			o.Metallic = lerpResult45;
			float lerpResult50 = lerp( ( tex2DNode5_g28.a * _SmoothnessTextureChannel ) , ( tex2DNode46.a * _TriplanarSmoothness ) , temp_output_58_0.x);
			o.Smoothness = lerpResult50;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
0;73.6;1536;712;1531.016;-32.84109;1.3;False;False
Node;AmplifyShaderEditor.Vector2Node;42;-1610.818,1013.264;Inherit;False;Property;_TriplanarTiling;Tiling;23;0;Create;False;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;41;-1604.458,1205.63;Inherit;False;Property;_TriplanarOffset;Offset;25;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;23;-1508.479,99.11191;Inherit;True;Property;_Black;Black;21;1;[HideInInspector];Create;True;0;0;0;False;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WorldPosInputsNode;65;-1252.167,665.2916;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;43;-1408.998,1092.354;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;55;-1294.267,466.5708;Inherit;True;Property;_TopPattern;Top Pattern;26;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;24;-1513.951,-100.28;Inherit;True;Property;_White;White;24;1;[HideInInspector];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;25;-1473.455,349.3972;Inherit;False;Property;_Falloff;Falloff;16;1;[Header];Create;True;1;Triplanar;0;0;False;0;False;3;0;1;200;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-1216.945,818.4487;Inherit;False;Property;_TopSize;Top Size;27;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1066.788,1156.749;Inherit;False;Property;_TriplanarNormalMapScale;Bump Scale;22;0;Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;46;-873.2795,1243.482;Inherit;True;Property;_TriplanarMetallic;Triplanar Metallic;18;1;[SingleLineTexture];Create;True;1;Triplanar;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;-861.1224,1453.208;Inherit;False;Property;_TriplanarSmoothness;Triplanar Smoothness;19;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;26;-1118.767,94.57541;Inherit;True;Cylindrical;World;False;Top Texture 0;_TopTexture0;white;16;None;Mid Texture 0;_MidTexture0;white;18;None;Bot Texture 0;_BotTexture0;white;17;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT3;1,1,1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;52;-983.033,466.6556;Inherit;True;Procedural Sample;-1;;25;f5379ff72769e2b4495e5ce2f004d8d4;2,157,2,315,2;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,1;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-566.291,1339.227;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-706.696,97.14165;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;38;-874.0479,812.1228;Inherit;True;Property;_TriplanarAlbedo;Triplanar Albedo;17;1;[SingleLineTexture];Create;True;1;Triplanar;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;39;-863.8246,1022.129;Inherit;True;Property;_TriplanarNormalMap;Normal Map;20;2;[Normal];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;67;-798.8596,-313.6611;Inherit;False;Builtin Material;0;;28;60f6c6f9fe3c1dc4187a8a52aa0c9efd;0;0;8;COLOR;0;COLOR;36;FLOAT3;10;FLOAT3;35;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;16
Node;AmplifyShaderEditor.LerpOp;27;-245.8774,-77.9901;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;45;-249.609,223.5439;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;50;-248.09,360.3216;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;31;-246.5263,76.4635;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;94.22447,75.98747;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Mameshiba Games/Standard_Triplanar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;43;0;42;0
WireConnection;43;1;41;0
WireConnection;46;1;43;0
WireConnection;26;0;24;0
WireConnection;26;1;23;0
WireConnection;26;2;23;0
WireConnection;26;4;25;0
WireConnection;52;82;55;0
WireConnection;52;80;65;0
WireConnection;52;104;60;0
WireConnection;49;0;46;4
WireConnection;49;1;48;0
WireConnection;58;0;26;0
WireConnection;58;1;52;0
WireConnection;38;1;43;0
WireConnection;39;1;43;0
WireConnection;39;5;44;0
WireConnection;27;0;67;36
WireConnection;27;1;38;0
WireConnection;27;2;58;0
WireConnection;45;0;67;13
WireConnection;45;1;46;1
WireConnection;45;2;58;0
WireConnection;50;0;67;14
WireConnection;50;1;49;0
WireConnection;50;2;58;0
WireConnection;31;0;67;35
WireConnection;31;1;39;0
WireConnection;31;2;58;0
WireConnection;0;0;27;0
WireConnection;0;1;31;0
WireConnection;0;3;45;0
WireConnection;0;4;50;0
ASEEND*/
//CHKSM=17AE1E7FF38C12826EFA802A6C5231EA020CD7E9