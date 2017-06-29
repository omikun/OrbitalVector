Shader "Custom/Clouds2D"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_phase("_phase", Range(0.0, 20)) = 0
		_cloudscale("_cloudscale", Range(0.5, 2)) = 1.1
		_speed("_speed", Range(0.0, 0.1)) = 0.03
		_clouddark("_clouddark", Range(0, 1)) = 0.5
		_cloudlight("_cloudlight", Range(0, 1)) = 0.3
		_cloudcover("_cloudcover", Range(0, 1)) = 0.2
		_cloudalpha("_cloudalpha", Range(1, 10)) = 8.0
		_skytint("_skytint", Range(0, 1)) = 0.5
   	 	_SkyColour1("_SkyColour1", Color)= (0.2, 0.4, 0.6)
   	 	_SkyColour2("_SkyColour2", Color)= (0.4, 0.7, 1.0)
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		LOD 100

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
				float2 uv : TEXCOORD0;
			};

			float4 _MainTex_ST;
			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv = o.pos;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float _phase;
			float _cloudscale;
			float _speed;
			float _clouddark;
			float _cloudlight;
			float _cloudcover;
			float _cloudalpha;
			float _skytint;
			fixed3 _SkyColour1 = fixed3(0.2, 0.4, 0.6);
			fixed3 _SkyColour2 = fixed3(0.4, 0.7, 1.0);


			fixed2 hash(fixed2 p) {
				p = fixed2(dot(p,fixed2(127.1,311.7)), dot(p,fixed2(269.5,183.3)));
				return -1.0 + 2.0*frac(sin(p)*43758.5453123);
			}

			float noise(in fixed2 p) {
				const float K1 = 0.366025404; // (sqrt(3)-1)/2;
				const float K2 = 0.211324865; // (3-sqrt(3))/6;
				fixed2 i = floor(p + (p.x + p.y)*K1);
				fixed2 a = p - i + (i.x + i.y)*K2;
				fixed2 o = (a.x > a.y) ? fixed2(1.0,0.0) : fixed2(0.0,1.0); //fixed2 of = 0.5 + 0.5*fixed2(sign(a.x-a.y), sign(a.y-a.x));
				fixed2 b = a - o + K2;
				fixed2 c = a - 1.0 + 2.0*K2;
				fixed3 h = max(0.5 - fixed3(dot(a,a), dot(b,b), dot(c,c)), 0.0);
				fixed3 n = h*h*h*h*fixed3(dot(a,hash(i + 0.0)), dot(b,hash(i + o)), dot(c,hash(i + 1.0)));
				return dot(n, fixed3(70.0, 70, 70));
			}

			//const mat2 m = mat2(1.6,  1.2, -1.2,  1.6);
			fixed2 mMul(fixed2 n)
			{
				n.x = 1.6 * n.x + 1.2 * n.y;
				n.y = -1.2 * n.x + 1.6 * n.y;
				return n;
			}
			float fbm(fixed2 n) {
				float total = 0.0, amplitude = 0.1;
				for (int i = 0; i < 7; i++) {
					total += noise(n) * amplitude;
					//n = m * n;
					n = mMul(n);
					amplitude *= 0.4;
				}
				return total;
			}

			// -----------------------------------------------

			//void mainImage( out fixed4 fragColor, in fixed2 fragCoord ) 

			fixed4 frag(v2f input) : SV_Target
			{
				//fixed2 p = fragCoord.xy / iResolution.xy;
				//fixed2 uv = p*fixed2(iResolution.x / iResolution.y,1.0);
				float iGlobalTime = _phase;
				float time = iGlobalTime * _speed;
				fixed2 uv = input.uv;
				float q = fbm(uv * _cloudscale * 0.5);

				//ridged noise shape
				float r = 0.0;
				uv *= _cloudscale;
				uv -= q - time;
				float weight = 0.8;
				for (int i = 0; i < 8; i++) {
					r += abs(weight*noise(uv));
					uv = mMul(uv) + time;
					weight *= 0.7;
				}

				//noise shape
				float f = 0.0;
				//uv = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv = input.uv;
				uv *= _cloudscale;
				uv -= q - time;
				weight = 0.7;
				for (int i = 0; i < 8; i++) {
					f += weight*noise(uv);
					uv = mMul(uv) + time;
					weight *= 0.6;
				}

				f *= r + f;

				//noise colour
				float c = 0.0;
				time = iGlobalTime * _speed * 2.0;
				//uv = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv = input.uv;
				uv *= _cloudscale*2.0;
				uv -= q - time;
				weight = 0.4;
				for (int i = 0; i < 7; i++) {
					c += weight*noise(uv);
					uv = mMul(uv) + time;
					weight *= 0.6;
				}

				//noise ridge colour
				float c1 = 0.0;
				time = iGlobalTime * _speed * 3.0;
				//uv = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv = input.uv;
				uv *= _cloudscale*3.0;
				uv -= q - time;
				weight = 0.4;
				for (int i = 0; i < 7; i++) {
					c1 += abs(weight*noise(uv));
					uv = mMul(uv) + time;
					weight *= 0.6;
				}

				c += c1;

				fixed3 _SkyColour = lerp(_SkyColour2, _SkyColour1, uv.y);
				fixed3 cloudcolour = fixed3(1.1, 1.1, 0.9) * clamp((_clouddark + _cloudlight*c), 0.0, 1.0);

				f = _cloudcover + _cloudalpha*f*r;

				fixed3 result = lerp(_SkyColour, clamp(_skytint * _SkyColour + cloudcolour, 0.0, 1.0), clamp(f + c, 0.0, 1.0));

				fixed4 fragColor = fixed4(result, 1.0);
				return fragColor;

			}
			ENDCG
		}
	}
}