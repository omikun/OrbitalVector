Shader "Custom/Clouds3"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TexSize("Texture Size", Range(1, 512)) = 512
		_phase("_phase", Range(0.0, 20)) = 0
		_cloudscale("_cloudscale", Range(0.5, 10)) = 1.1
		_speed("_speed", Range(0.0, 0.1)) = 0.03
		_clouddark("_clouddark", Range(0, 1)) = 0.5
		_cloudlight("_cloudlight", Range(0, 1)) = 0.3
		_cloudcover("_cloudcover", Range(0, 1)) = 0.2
		_cloudalpha("_cloudalpha", Range(1, 10)) = 8.0
		_skytint("_skytint", Range(0, 1)) = 0.5
   	 	_SkyColour1("_SkyColour1", Color)= (0.2, 0.4, 0.6)
   	 	_SkyColour2("_SkyColour2", Color)= (0.4, 0.7, 1.0)
	_Normals("Normal Map tl", 2D) = "black" {}
	}
		SubShader
	{
		//Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

//		Pass
//		{
			CGPROGRAM
			#pragma surface surf Lambert alpha
			#pragma target 3.0

			float4 _MainTex_ST;
			sampler2D _MainTex;
			sampler2D _Normals;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
				half4 Custom;
			};
			struct Input {
				float2 uv_Tex;
				float2 uv_Normals;
			};
			float _TexSize;
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
			//fixed4 frag(v2f IN) : SV_Target

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed2 p = IN.uv_Tex;// / _TexSize;
				fixed2 uv_Tex = IN.uv_Tex / _TexSize;
				//fixed2 p = fragCoord.xy / iResolution.xy;
				//fixed2 uv_Tex = p*fixed2(iResolution.x / iResolution.y,1.0);
				float iGlobalTime = _phase;
				float time = iGlobalTime * _speed;
				float q = fbm(uv_Tex * _cloudscale * 0.5);

				//ridged noise shape
				float r = 0.0;
				uv_Tex *= _cloudscale;
				uv_Tex -= q - time;
				float weight = 0.8;
				for (int i = 0; i < 8; i++) {
					r += abs(weight*noise(uv_Tex));
					uv_Tex = mMul(uv_Tex) + time;
					weight *= 0.7;
				}

				//noise shape
				float f = 0.0;
				//uv_Tex = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv_Tex = IN.uv_Tex;
				uv_Tex *= _cloudscale;
				uv_Tex -= q - time;
				weight = 0.7;
				for (int i = 0; i < 8; i++) {
					f += weight*noise(uv_Tex);
					uv_Tex = mMul(uv_Tex) + time;
					weight *= 0.6;
				}

				f *= r + f;

				//noise colour
				float c = 0.0;
				time = iGlobalTime * _speed * 2.0;
				//uv_Tex = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv_Tex = IN.uv_Tex;
				uv_Tex *= _cloudscale*2.0;
				uv_Tex -= q - time;
				weight = 0.4;
				for (int i = 0; i < 7; i++) {
					c += weight*noise(uv_Tex);
					uv_Tex = mMul(uv_Tex) + time;
					weight *= 0.6;
				}

				//noise ridge colour
				float c1 = 0.0;
				time = iGlobalTime * _speed * 3.0;
				//uv_Tex = p*fixed2(iResolution.x / iResolution.y,1.0);
				uv_Tex = IN.uv_Tex;
				uv_Tex *= _cloudscale*3.0;
				uv_Tex -= q - time;
				weight = 0.4;
				for (int i = 0; i < 7; i++) {
					c1 += abs(weight*noise(uv_Tex));
					uv_Tex = mMul(uv_Tex) + time;
					weight *= 0.6;
				}

				c += c1;

				fixed3 _SkyColour = lerp(_SkyColour2, _SkyColour1, p.y);
				fixed3 cloudcolour = fixed3(1.1, 1.1, 0.9) * clamp((_clouddark + _cloudlight*c), 0.0, 1.0);

				f = _cloudcover + _cloudalpha*f*r;

				fixed3 result = lerp(_SkyColour, clamp(_skytint * _SkyColour + cloudcolour, 0.0, 1.0), clamp(f + c, 0.0, 1.0));

				//fixed4 fragColor = fixed4(cloudcolour, clamp(f + c, 0.0, 1.0));
				//fixed4 fragColor = fixed4(result, 0.5);
				//fixed4 fragColor = fixed4(result, 1.0);
				//fixed4 fragColor = fixed4(p.x, p.y, 0.0, 1.0);
				//return fragColor;
				result = fixed3(uv_Tex.x, uv_Tex.y, 0);
				//result = fixed3(p.x, p.y, 0);
				o.Albedo = result;
				o.Alpha = 1;// 1 - clamp(f + c, 0.0, 1.0);
				float4 Sampled2D0 = tex2D(_Normals, IN.uv_Normals.xy);
				float4 UnpackNormal0 = float4(UnpackNormal(Sampled2D0).xyz, 1.0);
				UnpackNormal0.xy = UnpackNormal0.xy;
				o.Normal = UnpackNormal0;
				o.Normal = normalize(o.Normal);

			}
			ENDCG
		//}
	}
}