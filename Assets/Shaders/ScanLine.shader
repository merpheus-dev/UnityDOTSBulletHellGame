Shader "Hidden/ScanLine"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		#define mod(x, y) (x - y * floor(x / y))
		#define fract(x)  x - floor(x)
		#define PIXELSIZE 3.0
		
		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _Blend;

		float scanline(float2 uv) {
			return sin(_ScreenParams.y * uv.y * 0.7 - _Time * 10.0);
		}

		float slowscan(float2 uv) {
			return sin(_ScreenParams.y * uv.y * 0.02 + _Time * 6.0);
		}

        float4 Frag(VaryingsDefault i) : SV_Target
        {
			float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
			color.rgb = lerp(color.rgb, lerp(scanline(i.texcoord), slowscan(i.texcoord),.5), _Blend);
            return color;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}