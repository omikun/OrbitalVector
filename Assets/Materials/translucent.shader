Shader "Custom/Blur"
{
	Properties
	{
		_Factor ("Factor", Range(0, 5)) = 1.0
        _Backlit ("Backlit", Range(0, 1)) = .01
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		
		GrabPass { }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = ComputeGrabScreenPos(o.pos);
				return o;
			}
			
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			float _Factor;
            float _Backlit;

			half4 frag (v2f i) : SV_Target
			{

				half4 pixelCol = half4(0, 0, 0, 0);

				#define ADDPIXEL(weight,kernelX) tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x + _GrabTexture_TexelSize.x * kernelX * _Factor, i.uv.y, i.uv.z, i.uv.w))) * weight
				
				pixelCol += ADDPIXEL(0.05, 4.0);
				pixelCol += ADDPIXEL(0.09, 3.0);
				pixelCol += ADDPIXEL(0.12, 2.0);
				pixelCol += ADDPIXEL(0.15, 1.0);
				pixelCol += ADDPIXEL(0.18, 0.0);
				pixelCol += ADDPIXEL(0.15, -1.0);
				pixelCol += ADDPIXEL(0.12, -2.0);
				pixelCol += ADDPIXEL(0.09, -3.0);
				pixelCol += ADDPIXEL(0.05, -4.0);
                pixelCol += half4(_Backlit, _Backlit, _Backlit, .0);

				return pixelCol;
			}
			ENDCG
		}

		GrabPass { }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = ComputeGrabScreenPos(o.pos);
				return o;
			}
			
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			float _Factor;
            float _Backlit;

			fixed4 frag (v2f i) : SV_Target
			{

				fixed4 pixelCol = fixed4(0, 0, 0, 0);

				#define ADDPIXEL(weight,kernelY) tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x, i.uv.y + _GrabTexture_TexelSize.y * kernelY * _Factor, i.uv.z, i.uv.w))) * weight
				
				pixelCol += ADDPIXEL(0.05, 4.0);
				pixelCol += ADDPIXEL(0.09, 3.0);
				pixelCol += ADDPIXEL(0.12, 2.0);
				pixelCol += ADDPIXEL(0.15, 1.0);
				pixelCol += ADDPIXEL(0.18, 0.0);
				pixelCol += ADDPIXEL(0.15, -1.0);
				pixelCol += ADDPIXEL(0.12, -2.0);
				pixelCol += ADDPIXEL(0.09, -3.0);
				pixelCol += ADDPIXEL(0.05, -4.0);
                pixelCol += half4(_Backlit, _Backlit, _Backlit, .0);
				return pixelCol;
			}
			ENDCG
		}
	}
}
