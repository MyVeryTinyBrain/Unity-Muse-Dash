// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Simple "just colors" shader that's used for built-in debug visualizations,
// in the editor etc. Just outputs _Color * vertex color; and blend/Z/cull/bias
// controlled by material parameters.

Shader "GL/GL"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Int) = 5.0 // SrcAlpha
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
		_ZWrite("ZWrite", Int) = 1.0 // On
		_ZTest("ZTest", Int) = 4.0 // LEqual
		_Cull("Cull", Int) = 0.0 // Off
		_ZBias("ZBias", Float) = 0.0
		
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		Pass
		{
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			ZTest[_ZTest]
			Cull[_Cull]
			Offset[_ZBias],[_ZBias]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}