// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Skybox"
{
	Properties
	{
		_TopColor("Top Color", Color) = (0.5235849,0.8721517,1,1)
		_MidColor("Mid Color", Color) = (0.2249911,0.5004692,0.6037736,1)
		_BottomColor("Bottom Color", Color) = (1,0.9742292,0.5254902,1)
		_Middle("Middle", Range( 0.01 , 0.99)) = 0.1
		[Header(Additional Cube Texture)][SingleLineTexture]_AdditionalTexture("Additional Texture", CUBE) = "black" {}
		[Toggle]_UseAdditionalTexture("Use Additional Texture", Float) = 0
		_BrightColor("Bright Color", Color) = (1,1,1,1)
		_DarkColor("Dark Color", Color) = (1,1,1,1)
		_MaxLightIntensity("Max Light Intensity", Float) = 1
		_MinLightIntensity("Min Light Intensity", Float) = 0
		_RotationSpeed("Rotation Speed", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "Lighting.cginc"
			#define ASE_NEEDS_VERT_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _RotationSpeed;
			uniform float _UseAdditionalTexture;
			uniform float4 _DarkColor;
			uniform float4 _BrightColor;
			uniform float _MinLightIntensity;
			uniform float _MaxLightIntensity;
			uniform samplerCUBE _AdditionalTexture;
			uniform float4 _AdditionalTexture_ST;
			uniform float4 _BottomColor;
			uniform float4 _MidColor;
			uniform float _Middle;
			uniform float4 _TopColor;
			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float mulTime40 = _Time.y * _RotationSpeed;
				float3 rotatedValue38 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, float3( 0,1,0 ), mulTime40 );
				
				o.ase_texcoord1.xyz = v.ase_texcoord.xyz;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( rotatedValue38 - v.vertex.xyz );
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
				float4 ase_lightColor = 0;
				#else //aselc
				float4 ase_lightColor = _LightColor0;
				#endif //aselc
				float4 lerpResult50 = lerp( _DarkColor , _BrightColor , saturate( (0.0 + (ase_lightColor.a - _MinLightIntensity) * (1.0 - 0.0) / (_MaxLightIntensity - _MinLightIntensity)) ));
				float3 uv_AdditionalTexture3 = i.ase_texcoord1.xyz;
				uv_AdditionalTexture3.xy = i.ase_texcoord1.xyz.xy * _AdditionalTexture_ST.xy + _AdditionalTexture_ST.zw;
				float2 texCoord4 = i.ase_texcoord1.xyz.xy * float2( 1,1 ) + float2( 0,0 );
				float4 lerpResult7 = lerp( _BottomColor , _MidColor , saturate( ( texCoord4.y / _Middle ) ));
				float4 lerpResult13 = lerp( _MidColor , _TopColor , ( ( texCoord4.y - _Middle ) / ( 1.0 - _Middle ) ));
				float4 blendOpSrc61 = (( _UseAdditionalTexture )?( ( lerpResult50 * texCUBE( _AdditionalTexture, uv_AdditionalTexture3 ) ) ):( float4( 0,0,0,0 ) ));
				float4 blendOpDest61 = ( ( lerpResult7 * step( texCoord4.y , _Middle ) ) + ( lerpResult13 * step( _Middle , texCoord4.y ) ) );
				
				
				finalColor = ( saturate( 2.0f*blendOpDest61*blendOpSrc61 + blendOpDest61*blendOpDest61*(1.0f - 2.0f*blendOpSrc61) ));
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18921
1648;109;1487;753;-500.0757;406.1871;1.200072;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-370.5145,565.5339;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-429.663,458.2484;Inherit;False;Property;_Middle;Middle;3;0;Create;True;0;0;0;False;0;False;0.1;0;0.01;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;268.0531,-93.16312;Inherit;False;Property;_MaxLightIntensity;Max Light Intensity;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;44;311.4151,-316.6839;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;45;266.7531,-181.5633;Inherit;False;Property;_MinLightIntensity;Min Light Intensity;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;9;-111.0393,367.7298;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;14;264.9809,686.0821;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;15;262.9855,789.081;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;46;572.2538,-271.2634;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;16;475.0017,673.063;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;47;772.8463,-271.9304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;25;6.393035,362.7335;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;49;683.1463,-482.6304;Inherit;False;Property;_BrightColor;Bright Color;6;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;48;684.6147,-678.4832;Inherit;False;Property;_DarkColor;Dark Color;7;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-347.311,47.17042;Inherit;False;Property;_BottomColor;Bottom Color;2;0;Create;True;0;0;0;False;0;False;1,0.9742292,0.5254902,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;349.2932,449.4209;Inherit;False;Property;_TopColor;Top Color;0;0;Create;True;0;0;0;False;0;False;0.5235849,0.8721517,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-345.8162,243.1803;Inherit;False;Property;_MidColor;Mid Color;1;0;Create;True;0;0;0;False;0;False;0.2249911,0.5004692,0.6037736,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;7;151.4126,176.4097;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;59;884.4471,-99.17781;Inherit;True;Property;_AdditionalTexture;Additional Texture;4;2;[Header];[SingleLineTexture];Create;True;1;Additional Cube Texture;0;0;False;0;False;-1;None;None;True;0;False;black;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;13;605.5285,329.5;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;42;1037.391,509.1419;Inherit;False;Property;_RotationSpeed;Rotation Speed;10;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;11;172.4613,392.1179;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;19;715.0156,465.4626;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;50;1016.547,-502.4305;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;903.5427,321.5012;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;40;1343.508,515.5999;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;336.3519,181.1915;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;39;1333.175,604.7225;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1245.181,-260.6649;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;1054.244,183.3872;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;38;1574.709,497.517;Inherit;False;False;4;0;FLOAT3;0,1,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;60;1464.975,-171.1813;Inherit;False;Property;_UseAdditionalTexture;Use Additional Texture;5;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;61;1757.49,63.43039;Inherit;False;SoftLight;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;41;1885.992,582.7651;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2039.04,69.63842;Float;False;True;-1;2;ASEMaterialInspector;100;1;Mameshiba Games/Skybox;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;9;0;4;2
WireConnection;9;1;8;0
WireConnection;14;0;4;2
WireConnection;14;1;8;0
WireConnection;15;0;8;0
WireConnection;46;0;44;2
WireConnection;46;1;45;0
WireConnection;46;2;43;0
WireConnection;16;0;14;0
WireConnection;16;1;15;0
WireConnection;47;0;46;0
WireConnection;25;0;9;0
WireConnection;7;0;3;0
WireConnection;7;1;2;0
WireConnection;7;2;25;0
WireConnection;13;0;2;0
WireConnection;13;1;1;0
WireConnection;13;2;16;0
WireConnection;11;0;4;2
WireConnection;11;1;8;0
WireConnection;19;0;8;0
WireConnection;19;1;4;2
WireConnection;50;0;48;0
WireConnection;50;1;49;0
WireConnection;50;2;47;0
WireConnection;17;0;13;0
WireConnection;17;1;19;0
WireConnection;40;0;42;0
WireConnection;10;0;7;0
WireConnection;10;1;11;0
WireConnection;51;0;50;0
WireConnection;51;1;59;0
WireConnection;12;0;10;0
WireConnection;12;1;17;0
WireConnection;38;1;40;0
WireConnection;38;3;39;0
WireConnection;60;1;51;0
WireConnection;61;0;60;0
WireConnection;61;1;12;0
WireConnection;41;0;38;0
WireConnection;41;1;39;0
WireConnection;0;0;61;0
WireConnection;0;1;41;0
ASEEND*/
//CHKSM=D1A41EBC75E50F5A94E7DA03142015ED6DB2B757