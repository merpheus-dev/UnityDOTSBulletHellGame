// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mattatz/TextureAnimation"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)

		_Cols("Cols Count", Int) = 5
		_Rows("Rows Count", Int) = 3
		_Frame("Per Frame Length", Float) = 0.5
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}
			LOD 100

			ZWrite Off
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

				fixed4 _Color;

				uint _Cols;
				uint _Rows;

				float _Frame;

				fixed4 shot(sampler2D tex, float2 uv, float dx, float dy, int frame) {
					return tex2D(tex, float2(
						(uv.x * dx) + fmod(frame, _Cols) * dx,
						1.0 - ((uv.y * dy) + (frame / _Cols) * dy)
					));
				}

				v2f vert(appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
					int frames = _Rows * _Cols;
					float frame = fmod(_Time.y / _Frame, frames);
					int current = floor(frame);
					float dx = 1.0 / _Cols;
					float dy = 1.0 / _Rows;

					// not lerping to next frame
					// return shot(_MainTex, i.uv, dx, dy, current) * _Color;

					int next = floor(fmod(frame + 1, frames));
					return lerp(shot(_MainTex, i.uv, dx, dy, current), shot(_MainTex, i.uv, dx, dy, next), frame - current) * _Color;
				}

				ENDCG
			}
		}
}