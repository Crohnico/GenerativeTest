// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mameshiba Games/Wind Plants"
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
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_FixedWindForce("Fixed Wind Force", Float) = 0
		[Header(Height Falloff)]_FalloffAngle("Falloff Angle", Vector) = (0,1,0,0)
		_FalloffDisplacement("Falloff Displacement", Range( -10 , 10)) = 0
		_FalloffSmooth("Falloff Smooth", Range( 0 , 20)) = 1
		_AngleAmplitude("Angle Amplitude", Range( 0 , 10)) = 1
		_MaxAngle("Max Angle", Range( 0 , 45)) = 45
		_WindStrenghtTrunk("Wind Strenght Trunk", Range( 0 , 2)) = 1
		[Toggle]_DebugMode("Debug Mode", Float) = 0
		[Header(Leaves)]_LeavesMask("Leaves Mask", 2D) = "white" {}
		_WindStrenghtLeaves("Wind Strenght Leaves", Range( 0 , 3)) = 1
		_ScaleWindLeaves("Scale Wind Leaves", Float) = 0
		_LeavesDisplacement("Leaves Displacement", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float WindDirection;
		uniform float _FixedWindForce;
		uniform float WindForce;
		uniform float _AngleAmplitude;
		uniform float _WindStrenghtTrunk;
		uniform float _MaxAngle;
		uniform float _FalloffSmooth;
		uniform float3 _FalloffAngle;
		uniform float _FalloffDisplacement;
		uniform sampler2D _LeavesMask;
		uniform float4 _LeavesMask_ST;
		uniform float _WindStrenghtLeaves;
		uniform float _ScaleWindLeaves;
		uniform float _LeavesDisplacement;
		uniform float _DebugMode;
		uniform sampler2D _BumpMap;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _BumpScale;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _MetallicGlossMap;
		uniform float _SmoothnessTextureChannel;
		uniform float _Cutoff = 0.5;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 break4_g7 = mul( UNITY_MATRIX_M, float4( float3(1,0,0) , 0.0 ) ).xyz;
			float temp_output_164_0 = radians( ( WindDirection - ( degrees( atan2( break4_g7.x , break4_g7.z ) ) + -90.0 ) ) );
			float3 appendResult92 = (float3(cos( temp_output_164_0 ) , 0.0 , -sin( temp_output_164_0 )));
			float3 DirectionAxis149 = appendResult92;
			float ifLocalVar177 = 0;
			if( _FixedWindForce <= 0.0 )
				ifLocalVar177 = WindForce;
			else
				ifLocalVar177 = _FixedWindForce;
			float WindForce172 = ifLocalVar177;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult125 = (float2(ase_worldPos.x , ase_worldPos.z));
			float simpleNoise122 = SimpleNoise( appendResult125 );
			float temp_output_127_0 = ( simpleNoise122 * 3.0 );
			float mulTime100 = _Time.y * ( WindForce172 * _WindStrenghtTrunk );
			float temp_output_97_0 = ( WindForce172 * (0.0 + (( ( ( _AngleAmplitude + temp_output_127_0 ) * sin( ( temp_output_127_0 + mulTime100 ) ) ) + _MaxAngle ) - 0.0) * (-0.15 - 0.0) / (90.0 - 0.0)) );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 temp_cast_2 = (_FalloffSmooth).xxx;
			float3 temp_cast_3 = (_FalloffDisplacement).xxx;
			float3 smoothstepResult107 = smoothstep( float3( 0,0,0 ) , temp_cast_2 , ( ( ase_vertex3Pos * _FalloffAngle ) - temp_cast_3 ));
			float3 HeightRamp109 = smoothstepResult107;
			float3 temp_output_110_0 = ( ase_vertex3Pos * HeightRamp109 );
			float3 rotatedValue84 = RotateAroundAxis( float3( 0,0,0 ), temp_output_110_0, DirectionAxis149, temp_output_97_0 );
			float2 uv_LeavesMask = v.texcoord * _LeavesMask_ST.xy + _LeavesMask_ST.zw;
			float2 appendResult136 = (float2(ase_worldPos.x , ase_worldPos.z));
			float mulTime145 = _Time.y * ( _WindStrenghtLeaves * WindForce172 );
			float simpleNoise137 = SimpleNoise( ( appendResult136 + mulTime145 )*_ScaleWindLeaves );
			float3 rotatedValue140 = RotateAroundAxis( float3( 0,0,0 ), DirectionAxis149, float3( 0,1,0 ), 90.0 );
			v.vertex.xyz += ( ( rotatedValue84 - temp_output_110_0 ) + ( tex2Dlod( _LeavesMask, float4( uv_LeavesMask, 0, 0.0) ).r * simpleNoise137 * _LeavesDisplacement * ( rotatedValue140 + float3( 0,1,0 ) ) ) );
			v.vertex.w = 1;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 rotatedValue82 = RotateAroundAxis( float3( 0,0,0 ), ase_vertexNormal, DirectionAxis149, temp_output_97_0 );
			v.normal = rotatedValue82;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord18_g8 = i.uv_texcoord * _Tiling + _Offset;
			float3 tex2DNode2_g8 = UnpackScaleNormal( tex2D( _BumpMap, uv_TexCoord18_g8 ), _BumpScale );
			o.Normal = (( _DebugMode )?( float3( 1,1,1 ) ):( tex2DNode2_g8 ));
			float4 tex2DNode1_g8 = tex2D( _MainTex, uv_TexCoord18_g8 );
			float3 temp_cast_0 = (_FalloffSmooth).xxx;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 temp_cast_1 = (_FalloffDisplacement).xxx;
			float3 smoothstepResult107 = smoothstep( float3( 0,0,0 ) , temp_cast_0 , ( ( ase_vertex3Pos * _FalloffAngle ) - temp_cast_1 ));
			float3 HeightRamp109 = smoothstepResult107;
			o.Albedo = (( _DebugMode )?( float4( HeightRamp109 , 0.0 ) ):( ( _Color * tex2DNode1_g8 ) )).rgb;
			float4 tex2DNode5_g8 = tex2D( _MetallicGlossMap, uv_TexCoord18_g8 );
			o.Metallic = (( _DebugMode )?( 0.0 ):( tex2DNode5_g8.r ));
			o.Smoothness = (( _DebugMode )?( 0.0 ):( ( tex2DNode5_g8.a * _SmoothnessTextureChannel ) ));
			o.Alpha = 1;
			clip( tex2DNode1_g8.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
1648;109;1487;753;5298.913;763.86;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;169;-4829.902,-464.2134;Inherit;False;877.448;362.7826;;4;172;175;170;177;WIND FORCE;0,0.6099918,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-4777.92,-293.165;Inherit;False;Global;WindForce;WindForce;13;0;Create;True;0;0;0;False;0;False;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;-4732.729,-388.1437;Inherit;False;Property;_FixedWindForce;Fixed Wind Force;17;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;177;-4417.629,-381.0438;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;172;-4177.966,-385.4276;Inherit;False;WindForce;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;124;-3829.625,231.7999;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;155;-3881.275,-513.3196;Inherit;False;1527.988;435.14;;9;149;146;92;95;90;91;30;96;164;DIRECTION AXIS;1,0,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-3832.402,-346.755;Inherit;False;Global;WindDirection;WindDirection;13;0;Create;True;0;0;0;False;0;False;0;35.96716;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;146;-3786.262,-256.4897;Inherit;False;ObjectGlobalRotation;-1;;7;94dcb6f0e89a88f48bb738f5668d8963;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;125;-3617.886,261.4705;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-3740.178,580.0422;Inherit;False;Property;_WindStrenghtTrunk;Wind Strenght Trunk;23;0;Create;True;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;-3656.553,484.7876;Inherit;False;172;WindForce;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;158;-2244.835,-530.3536;Inherit;False;1571.823;499.8087;;8;109;108;107;5;10;129;3;128;HEIGHT RAMP;1,0,0.992969,1;0;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;122;-3462.66,258.9984;Inherit;False;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;96;-3463.387,-290.8369;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-3408.454,534.6769;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-3227.108,314.1303;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;3;-2116.925,-418.2625;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;128;-2092.801,-247.8621;Inherit;False;Property;_FalloffAngle;Falloff Angle;18;1;[Header];Create;True;1;Height Falloff;0;0;False;0;False;0,1,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;100;-3261.47,532.8033;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;164;-3276.74,-286.9871;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1829.909,-204.1002;Inherit;False;Property;_FalloffDisplacement;Falloff Displacement;19;0;Create;True;0;0;0;False;0;False;0;0;-10;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;123;-3048.082,411.6445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-3171.832,184.0496;Inherit;False;Property;_AngleAmplitude;Angle Amplitude;21;0;Create;True;0;0;0;False;0;False;1;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-1876.495,-335.1058;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinOpNode;91;-3093.991,-257.9224;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;166;-3161.386,793.9135;Inherit;False;2475.398;780.9266;;13;134;131;137;132;138;139;136;145;135;144;143;165;173;LEAVES;0.3125114,1,0.2877358,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;10;-1499.788,-334.491;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinOpNode;101;-2852.25,411.558;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;95;-2944.44,-257.4909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-1486.46,-203.2211;Inherit;False;Property;_FalloffSmooth;Falloff Smooth;20;0;Create;True;0;0;0;False;0;False;1;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;90;-3084.345,-347.4612;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;130;-2857.377,286.2982;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-2815.633,523.2111;Inherit;False;Property;_MaxAngle;Max Angle;22;0;Create;True;0;0;0;False;0;False;45;0;0;45;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;173;-2866.237,1151.015;Inherit;False;172;WindForce;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;92;-2780.825,-320.4115;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;107;-1142.523,-337.6458;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-2672.436,330.6099;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-2968.406,1045.486;Inherit;False;Property;_WindStrenghtLeaves;Wind Strenght Leaves;26;0;Create;True;0;0;0;False;0;False;1;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;165;-1678.66,1283.948;Inherit;False;725.4136;231.9789;Leaves Displacement Rotation;3;141;140;151;;1,1,1,0.3137255;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;-2586.062,1075.787;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;135;-2302.335,937.6453;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;149;-2615.368,-323.5507;Inherit;False;DirectionAxis;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;109;-944.3491,-343.7116;Inherit;False;HeightRamp;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-2491.728,330.1323;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-1621.785,1401.03;Inherit;False;149;DirectionAxis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;136;-2075.658,989.042;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;-2295.722,236.2405;Inherit;False;172;WindForce;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-1573.749,568.6341;Inherit;False;109;HeightRamp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;88;-1585.279,413.6253;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;145;-2431.92,1075.977;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;98;-2303.125,336.9512;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;90;False;3;FLOAT;0;False;4;FLOAT;-0.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-2020.67,286.6688;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-1374.746,416.62;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;-1424.427,241.4138;Inherit;False;149;DirectionAxis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;-1928.388,1054.927;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;140;-1425.987,1335.696;Inherit;False;False;4;0;FLOAT3;0,1,0;False;1;FLOAT;90;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-2003.357,1169.996;Inherit;False;Property;_ScaleWindLeaves;Scale Wind Leaves;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;141;-1107.209,1337.139;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,1,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;22;-447.4104,-5.080737;Inherit;False;Builtin Material;0;;8;60f6c6f9fe3c1dc4187a8a52aa0c9efd;0;0;8;COLOR;0;COLOR;36;FLOAT3;10;FLOAT3;35;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;16
Node;AmplifyShaderEditor.WireNode;153;-820.8825,108.3371;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;84;-1229.114,269.9539;Inherit;False;False;4;0;FLOAT3;0,1,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-1237.284,1130.815;Inherit;False;Property;_LeavesDisplacement;Leaves Displacement;28;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;137;-1731.847,1046.725;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;131;-1286.392,879.7978;Inherit;True;Property;_LeavesMask;Leaves Mask;25;1;[Header];Create;True;1;Leaves;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;156;-160.8088,-139.0586;Inherit;False;109;HeightRamp;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;157;-229.6043,-178.7915;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;81;-149.2984,716.7318;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;154;-223.5219,615.6931;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-139.1159,588.3873;Inherit;False;149;DirectionAxis;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-929.5988,1043.634;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;86;-913.6843,390.9926;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;159;99.67118,-61.22268;Inherit;False;Property;_DebugMode;Debug Mode;24;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;82;59.32926,641.6063;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;160;99.67114,44.64255;Inherit;False;Property;_DebugMode;Debug Mode;24;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;161;98.46815,149.3048;Inherit;False;Property;_DebugMode;Debug Mode;25;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;142;-660.7318,395.7016;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;106;103.4542,-223.5664;Inherit;False;Property;_DebugMode;Debug Mode;27;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;400.1457,10.86368;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Mameshiba Games/Wind Plants;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;16;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;177;0;175;0
WireConnection;177;2;175;0
WireConnection;177;3;170;0
WireConnection;177;4;170;0
WireConnection;172;0;177;0
WireConnection;125;0;124;1
WireConnection;125;1;124;3
WireConnection;122;0;125;0
WireConnection;96;0;30;0
WireConnection;96;1;146;0
WireConnection;105;0;171;0
WireConnection;105;1;20;0
WireConnection;127;0;122;0
WireConnection;100;0;105;0
WireConnection;164;0;96;0
WireConnection;123;0;127;0
WireConnection;123;1;100;0
WireConnection;129;0;3;0
WireConnection;129;1;128;0
WireConnection;91;0;164;0
WireConnection;10;0;129;0
WireConnection;10;1;5;0
WireConnection;101;0;123;0
WireConnection;95;0;91;0
WireConnection;90;0;164;0
WireConnection;130;0;99;0
WireConnection;130;1;127;0
WireConnection;92;0;90;0
WireConnection;92;2;95;0
WireConnection;107;0;10;0
WireConnection;107;2;108;0
WireConnection;103;0;130;0
WireConnection;103;1;101;0
WireConnection;144;0;143;0
WireConnection;144;1;173;0
WireConnection;149;0;92;0
WireConnection;109;0;107;0
WireConnection;102;0;103;0
WireConnection;102;1;85;0
WireConnection;136;0;135;1
WireConnection;136;1;135;3
WireConnection;145;0;144;0
WireConnection;98;0;102;0
WireConnection;97;0;174;0
WireConnection;97;1;98;0
WireConnection;110;0;88;0
WireConnection;110;1;111;0
WireConnection;138;0;136;0
WireConnection;138;1;145;0
WireConnection;140;3;151;0
WireConnection;141;0;140;0
WireConnection;153;0;97;0
WireConnection;84;0;150;0
WireConnection;84;1;97;0
WireConnection;84;3;110;0
WireConnection;137;0;138;0
WireConnection;137;1;139;0
WireConnection;157;0;22;0
WireConnection;154;0;153;0
WireConnection;134;0;131;1
WireConnection;134;1;137;0
WireConnection;134;2;132;0
WireConnection;134;3;141;0
WireConnection;86;0;84;0
WireConnection;86;1;110;0
WireConnection;159;0;22;10
WireConnection;82;0;152;0
WireConnection;82;1;154;0
WireConnection;82;3;81;0
WireConnection;160;0;22;13
WireConnection;161;0;22;14
WireConnection;142;0;86;0
WireConnection;142;1;134;0
WireConnection;106;0;157;0
WireConnection;106;1;156;0
WireConnection;0;0;106;0
WireConnection;0;1;159;0
WireConnection;0;3;160;0
WireConnection;0;4;161;0
WireConnection;0;10;22;16
WireConnection;0;11;142;0
WireConnection;0;12;82;0
ASEEND*/
//CHKSM=B4835BA915AEA1F14F785B2D5C556380504CB09D