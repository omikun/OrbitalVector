Shader "Custom/Blur2"
{
	Properties
	{
		_Factor("Factor", Range(0, 500)) = 1.0
		_Backlit("Backlit", Range(0, 1)) = .01
	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

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
			/*
			fixed4 frag (v2f i) : SV_Target
			{

				fixed4 pixelCol = fixed4(0, 0, 0, 0);

				#define ADDPIXEL(weight,kernelY) 
			tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x, i.uv.y + _GrabTexture_TexelSize.y * kernelY * _Factor, i.uv.z, i.uv.w))) * weight
				
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
			*/

			float SCurve(float x) { // ---- by CeeJayDK
				x = x * 2.0 - 1.0;
				return -x * abs(x) * 0.5 + x + 0.5;
				//return dot(vec3(-x, 2.0, 1.0 ),vec3(abs(x), x, 1.0)) * 0.5; // possibly faster version
			}

			//vec4 BlurH(sampler2D source, vec2 size, vec2 uv, float radius) {
			fixed4 frag (v2f i) : SV_Target
			{
				float radius = _Factor;
				fixed4 A = fixed4(0, 0, 0, 0);
				fixed4 C = fixed4(0, 0, 0, 0);

				float divisor = 0.0;
				float weight = 0.0;

				float radiusMultiplier = 1.0 / radius;

				// Hardcoded for radius 20 (normally we input the radius
				// in there), needs to be literal here

				for (float x = -radius; x <= radius; x=x+4)
				{
					//A = texture(source, uv + vec2(x * width, 0.0));
					A = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uv.x + x * _GrabTexture_TexelSize.x, i.uv.y, i.uv.z, i.uv.w)));

					weight = SCurve(1.0 - (abs(x) * radiusMultiplier));

					C += A * weight;

					divisor += weight;
				}

				fixed4 pixelCol = fixed4(C.r / divisor, C.g / divisor, C.b / divisor, 1.0);
				pixelCol += half4(_Backlit, _Backlit, _Backlit, .0);
				return pixelCol;
			}

			ENDCG
		}
	}
}
