// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Loading"
{
	Properties
	{
	_Color("Color", Color) = (0, 1, 0, 1)
	_Speed("Speed", Range(1, 10)) = 1
	_Radius("Radius", Range(0, 0.5)) = 0.3
	}
		SubShader
	{
	Tags { "Queue" = "Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off

	Pass
	{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

	#define PI 3.14159

	struct appdata
	{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	};

	struct v2f
	{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	};

	fixed4 _Color;
	half _Speed;
	fixed _Radius;

	fixed4 circle(float2 uv, float2 center, float radius)
	{
		//if(pow(uv.x - center.x, 2) + pow(uv.y - center.y, 2) < pow(radius, 2)) return _Color;

		if (length(uv - center) < radius) return _Color;
		else return fixed4(0, 0, 0, 0);
		}

		v2f vert(appdata v)
		{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;

		return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
		fixed4 finalCol = (0, 0, 0, 0);

		for (int count = 7; count > 1; count--)
		{
		 half radian = fmod(_Time.y * _Speed + count * 0.5, 2 * PI);//»¡¶È
		 half2 center = half2(0.5 - _Radius * cos(radian), 0.5 + _Radius * sin(radian));

		 finalCol += circle(i.uv, center, count * 0.01);
		}

		return finalCol;
		}
		ENDCG
		}
	}
}