// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Wind Movement"
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
		_FixedWindForce("Fixed Wind Force", Float) = 0
		[Header(Falloff)]_FalloffDirection("Falloff Direction", Vector) = (0,0,0,0)
		_FalloffDisplacement("Falloff Displacement", Float) = 0
		_FalloffSmooth("Falloff Smooth", Range( 0 , 300)) = 0
		[Header(Wind)]_WindSpeedX("Wind Speed X", Float) = 1
		_WindSpeedY("Wind Speed Y", Float) = 1
		_Scale("Scale", Float) = 0
		[Toggle]_DebugMode("Debug Mode", Float) = 0
		_WindDisplacementVector("Wind Displacement Vector", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float3 _FalloffDirection;
		uniform float _FalloffDisplacement;
		uniform float _FalloffSmooth;
		uniform float _WindSpeedY;
		uniform float _FixedWindForce;
		uniform float WindForce;
		uniform float WindDirection;
		uniform float _WindSpeedX;
		uniform float _Scale;
		uniform float3 _WindDisplacementVector;
		uniform float _DebugMode;
		uniform sampler2D _BumpMap;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _BumpScale;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _MetallicGlossMap;
		uniform float _SmoothnessTextureChannel;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 FalloffDir134 = _FalloffDirection;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float dotResult52 = dot( FalloffDir134 , ase_vertex3Pos );
			float ifLocalVar146 = 0;
			if( _FixedWindForce <= 0.0 )
				ifLocalVar146 = WindForce;
			else
				ifLocalVar146 = _FixedWindForce;
			float mulTime18 = _Time.y * ( _WindSpeedY * ifLocalVar146 );
			float3 break4_g9 = mul( UNITY_MATRIX_M, float4( float3(1,0,0) , 0.0 ) ).xyz;
			float WindDirection108 = radians( ( WindDirection - ( degrees( atan2( break4_g9.x , break4_g9.z ) ) + -90.0 ) ) );
			float3 rotatedValue66 = RotateAroundAxis( float3( 0,0,0 ), ase_vertex3Pos, FalloffDir134, WindDirection108 );
			float3 break67 = rotatedValue66;
			float mulTime32 = _Time.y * ( ifLocalVar146 * _WindSpeedX );
			float2 appendResult39 = (float2(( mulTime18 + break67.x ) , ( mulTime32 + break67.z )));
			float simplePerlin2D13 = snoise( appendResult39*_Scale );
			simplePerlin2D13 = simplePerlin2D13*0.5 + 0.5;
			float temp_output_23_0 = saturate( ( saturate( ( ( dotResult52 + _FalloffDisplacement ) * _FalloffSmooth ) ) * simplePerlin2D13 ) );
			float3 Axis101 = float3(0,1,0);
			float3 rotatedValue71 = RotateAroundAxis( float3( 0,0,0 ), ( _WindDisplacementVector / float3( 10,10,10 ) ), Axis101, WindDirection108 );
			float3 rotatedValue53 = RotateAroundAxis( float3( 0,0,0 ), ase_vertex3Pos, Axis101, WindDirection108 );
			float3 LocalVertexOffset115 = ( ( temp_output_23_0 * rotatedValue71 ) + ( rotatedValue53 - ase_vertex3Pos ) );
			v.vertex.xyz += LocalVertexOffset115;
			v.vertex.w = 1;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 rotatedValue92 = RotateAroundAxis( float3( 0,0,0 ), ase_vertexNormal, Axis101, WindDirection108 );
			v.normal = rotatedValue92;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord18_g10 = i.uv_texcoord * _Tiling + _Offset;
			float3 tex2DNode2_g10 = UnpackScaleNormal( tex2D( _BumpMap, uv_TexCoord18_g10 ), _BumpScale );
			o.Normal = (( _DebugMode )?( float3( 1,1,1 ) ):( tex2DNode2_g10 ));
			float4 tex2DNode1_g10 = tex2D( _MainTex, uv_TexCoord18_g10 );
			float3 FalloffDir134 = _FalloffDirection;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float dotResult52 = dot( FalloffDir134 , ase_vertex3Pos );
			float ifLocalVar146 = 0;
			if( _FixedWindForce <= 0.0 )
				ifLocalVar146 = WindForce;
			else
				ifLocalVar146 = _FixedWindForce;
			float mulTime18 = _Time.y * ( _WindSpeedY * ifLocalVar146 );
			float3 break4_g9 = mul( UNITY_MATRIX_M, float4( float3(1,0,0) , 0.0 ) ).xyz;
			float WindDirection108 = radians( ( WindDirection - ( degrees( atan2( break4_g9.x , break4_g9.z ) ) + -90.0 ) ) );
			float3 rotatedValue66 = RotateAroundAxis( float3( 0,0,0 ), ase_vertex3Pos, FalloffDir134, WindDirection108 );
			float3 break67 = rotatedValue66;
			float mulTime32 = _Time.y * ( ifLocalVar146 * _WindSpeedX );
			float2 appendResult39 = (float2(( mulTime18 + break67.x ) , ( mulTime32 + break67.z )));
			float simplePerlin2D13 = snoise( appendResult39*_Scale );
			simplePerlin2D13 = simplePerlin2D13*0.5 + 0.5;
			float temp_output_23_0 = saturate( ( saturate( ( ( dotResult52 + _FalloffDisplacement ) * _FalloffSmooth ) ) * simplePerlin2D13 ) );
			float DebugOutput83 = temp_output_23_0;
			float4 temp_cast_2 = (DebugOutput83).xxxx;
			o.Albedo = (( _DebugMode )?( temp_cast_2 ):( ( _Color * tex2DNode1_g10 ) )).rgb;
			float4 tex2DNode5_g10 = tex2D( _MetallicGlossMap, uv_TexCoord18_g10 );
			o.Metallic = (( _DebugMode )?( 0.0 ):( tex2DNode5_g10.r ));
			o.Smoothness = (( _DebugMode )?( 0.0 ):( ( tex2DNode5_g10.a * _SmoothnessTextureChannel ) ));
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1648;109;1487;753;2734.312;-35.26297;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;114;-664.5032,-512.3074;Inherit;False;1063.58;253.0461;Comment;5;141;143;139;108;87;WIND DIRECTION;0.1745283,0.5628824,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-637.2798,-435.9805;Inherit;False;Global;WindDirection;WindDirection;13;0;Create;True;0;0;0;False;0;False;0;35.96716;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;141;-384.3447,-346.5049;Inherit;False;ObjectGlobalRotation;-1;;9;94dcb6f0e89a88f48bb738f5668d8963;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;133;-1984.884,-513.16;Inherit;False;563.9054;226.6968;;2;50;134;FALLOFF DIRECTION;0.9364045,0.345098,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;143;-141.3447,-427.5049;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;139;36.19754,-429.4964;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;50;-1923.673,-453.1374;Inherit;False;Property;_FalloffDirection;Falloff Direction;17;1;[Header];Create;True;1;Falloff;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;118;-2312.915,-193.1892;Inherit;False;3168.082;1452.093;;36;117;15;39;42;41;18;32;89;19;33;90;115;63;104;70;109;71;72;2;6;5;4;52;13;8;7;83;23;14;119;127;135;140;146;144;145;LOCAL VERTEX OFFSET;0.2635397,1,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-2231.579,324.5712;Inherit;False;Property;_FixedWindForce;Fixed Wind Force;16;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;134;-1654.685,-454.6599;Inherit;False;FalloffDir;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-2276.77,419.5499;Inherit;False;Global;WindForce;WindForce;13;0;Create;True;0;0;0;False;0;False;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;127;-2111.696,617.3364;Inherit;False;946.0342;498.5181;WindRotation;5;66;67;111;136;137;;1,1,1,0.1843137;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;108;190.3611,-433.0833;Inherit;False;WindDirection;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-2028.489,225.3599;Inherit;False;Property;_WindSpeedY;Wind Speed Y;21;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;146;-1961.849,326.8815;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-2028.405,509.0881;Inherit;False;Property;_WindSpeedX;Wind Speed X;20;1;[Header];Create;True;1;Wind;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-2061.448,824.4724;Inherit;False;108;WindDirection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;137;-2048.279,924.817;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;136;-2045.857,703.64;Inherit;False;134;FalloffDir;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;135;-1555.758,-37.35995;Inherit;False;134;FalloffDir;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-1732.827,319.12;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;2;-1566.198,82.95683;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-1732.144,440.8566;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;66;-1766.489,767.5634;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;67;-1445.411,771.6035;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;5;-1346.898,128.5568;Inherit;False;Property;_FalloffDisplacement;Falloff Displacement;18;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;32;-1553.24,441.7108;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;52;-1319.698,-1.043163;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;18;-1541.966,319.12;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-1135.766,444.9288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1255.07,221.8673;Inherit;False;Property;_FalloffSmooth;Falloff Smooth;19;0;Create;True;0;0;0;False;0;False;0;0;0;300;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-727.4937,346.7889;Inherit;False;Property;_Scale;Scale;22;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-1219.271,321.164;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-1111.698,14.95683;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;107;-1265.507,-514.4367;Inherit;False;563.9054;226.6968;;2;56;101;AXIS;1,0.3443396,0.3443396,1;0;0
Node;AmplifyShaderEditor.Vector3Node;56;-1163.847,-456.6513;Inherit;False;Constant;_Axis;Axis;19;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;117;-611.8434,316.7624;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-918.9818,28.55905;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;39;-945.7778,362.1219;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;119;-548.6799,725.1771;Inherit;False;946.0342;498.5181;Model Rotation;5;60;53;112;54;105;;1,1,1,0.1843137;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-943.7071,-457.5765;Inherit;False;Axis;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;8;-748.9269,29.07632;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;70;-539.3771,359.2815;Inherit;False;Property;_WindDisplacementVector;Wind Displacement Vector;23;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NoiseGeneratorNode;13;-683.2339,180.3766;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;140;-246.1606,363.2169;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;10,10,10;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-442.6967,908.2498;Inherit;False;108;WindDirection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-370.9381,272.0794;Inherit;False;108;WindDirection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-422.8915,784.8751;Inherit;False;101;Axis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;104;-343.2267,191.6411;Inherit;False;101;Axis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;54;-419.7946,1049.811;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-446.2198,29.27928;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;53;-145.9659,888.2864;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;23;-245.2769,30.40968;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;71;-88.79065,254.5754;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;191.4841,26.52455;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;60;191.0634,1028.034;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-66.77835,-97.07919;Inherit;False;DebugOutput;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;423.1465,518.0555;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;95;785.0823,-618.0272;Inherit;False;Builtin Material;0;;10;60f6c6f9fe3c1dc4187a8a52aa0c9efd;0;0;8;COLOR;0;COLOR;36;FLOAT3;10;FLOAT3;35;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;16
Node;AmplifyShaderEditor.RegisterLocalVarNode;115;608.2512,511.6104;Inherit;False;LocalVertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;1013.218,-51.74277;Inherit;False;108;WindDirection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;1142.493,-618.2418;Inherit;False;83;DebugOutput;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;99;1062.086,-651.3264;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;91;1034.696,55.57316;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;106;1038.445,-151.6198;Inherit;False;101;Axis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;29;1383.388,-694.7291;Inherit;False;Property;_DebugMode;Debug Mode;24;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;98;1376.739,-357.809;Inherit;False;Property;_DebugMode;Debug Mode;25;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;92;1289.599,-68.74904;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;96;1372.386,-571.3314;Inherit;False;Property;_DebugMode;Debug Mode;22;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;1365.388,-174.0878;Inherit;False;115;LocalVertexOffset;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;97;1370.209,-464.4643;Inherit;False;Property;_DebugMode;Debug Mode;26;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1774.18,-529.8526;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Mameshiba Games/Wind Movement;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;143;0;87;0
WireConnection;143;1;141;0
WireConnection;139;0;143;0
WireConnection;134;0;50;0
WireConnection;108;0;139;0
WireConnection;146;0;144;0
WireConnection;146;2;144;0
WireConnection;146;3;145;0
WireConnection;146;4;145;0
WireConnection;90;0;19;0
WireConnection;90;1;146;0
WireConnection;89;0;146;0
WireConnection;89;1;33;0
WireConnection;66;0;136;0
WireConnection;66;1;111;0
WireConnection;66;3;137;0
WireConnection;67;0;66;0
WireConnection;32;0;89;0
WireConnection;52;0;135;0
WireConnection;52;1;2;0
WireConnection;18;0;90;0
WireConnection;42;0;32;0
WireConnection;42;1;67;2
WireConnection;41;0;18;0
WireConnection;41;1;67;0
WireConnection;4;0;52;0
WireConnection;4;1;5;0
WireConnection;117;0;15;0
WireConnection;7;0;4;0
WireConnection;7;1;6;0
WireConnection;39;0;41;0
WireConnection;39;1;42;0
WireConnection;101;0;56;0
WireConnection;8;0;7;0
WireConnection;13;0;39;0
WireConnection;13;1;117;0
WireConnection;140;0;70;0
WireConnection;14;0;8;0
WireConnection;14;1;13;0
WireConnection;53;0;105;0
WireConnection;53;1;112;0
WireConnection;53;3;54;0
WireConnection;23;0;14;0
WireConnection;71;0;104;0
WireConnection;71;1;109;0
WireConnection;71;3;140;0
WireConnection;72;0;23;0
WireConnection;72;1;71;0
WireConnection;60;0;53;0
WireConnection;60;1;54;0
WireConnection;83;0;23;0
WireConnection;63;0;72;0
WireConnection;63;1;60;0
WireConnection;115;0;63;0
WireConnection;99;0;95;0
WireConnection;29;0;99;0
WireConnection;29;1;84;0
WireConnection;98;0;95;14
WireConnection;92;0;106;0
WireConnection;92;1;113;0
WireConnection;92;3;91;0
WireConnection;96;0;95;10
WireConnection;97;0;95;13
WireConnection;0;0;29;0
WireConnection;0;1;96;0
WireConnection;0;3;97;0
WireConnection;0;4;98;0
WireConnection;0;11;116;0
WireConnection;0;12;92;0
ASEEND*/
//CHKSM=1651224E4CD676B1381DFC6DFBFBA36D6734153E