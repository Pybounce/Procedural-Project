
Shader "Custom/Lit/Water"
{
    Properties
    {
		_TintColour ("Tint Colour", Color) = (1,1,1,1)
		_ShallowColour ("Shallow Colour",Color) = (1,1,1,1)
		_DeepColour ("Deep Colour", Color) = (1,1,1,1)
		_TopFog("Top Fog", Color) = (1,1,1,1)
		_MidFog("Mid Fog", Color) = (1,1,1,1)
		_BottomFog("Bottom Fog", Color) = (1,1,1,1)
		_FogEnd("Fog End", Float) = 10000
		_Gloss ("Gloss", Float) = 12
		_RippleNormals("Ripple Normal Map", 2D) = "bump"

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
			Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"	//Gets the _LightColor0

			float4 _TintColour;
			float _Gloss;
			sampler2D _RippleNormals;
			float4 _ShallowColour;
			float4 _DeepColour;
			float _FogEnd;
			float4 _TopFog;
			float4 _MidFog;
			float4 _BottomFog;


            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;	//w = sign
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD2;
				float4 worldPos : TEXCOORD3;
				float3 tangent : TEXCOORD4;
				float3 bitangent : TEXCOORD5;

            };

			float PingPong(float n, float max)
			{
				float l = max * 2;
				float t = n % l;
				if (t >= 0 && t < max)
				{
					return t;
				}
				else
				{
					return l - t;
				}
			}

			float CalculateSpecular(float3 n, float3 viewDir, float gloss)
			{
				float3 sunDir = _WorldSpaceLightPos0.xyz;	//Direction to the sun
				float angle = max(0, dot(normalize(sunDir - viewDir), n));
				float highlight = pow(angle, gloss) * 3;
				return highlight;
			}

			

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
				o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;
				o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
                return o;
            }

			float4 CalculateFogColour(float3 worldPos)
			{
				float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos);
				float3 vectorUp = float3(0, 1.0, 0);
				float viewDot = dot(viewDir, vectorUp);

				if (viewDot >= 0)
				{
					float t = saturate(viewDot * 3);
					return lerp(_MidFog, _TopFog, t);
				}
				else
				{
					float t = saturate(-viewDot * 3);
					return lerp(_MidFog, _BottomFog, t);
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//Normals
				float2 newUVs = float2(abs((i.worldPos.x + (_Time.y * 50)) / 300) % 1, abs(i.worldPos.z / 300) % 1);
				float3 rippleNormal = UnpackNormal(tex2D(_RippleNormals, newUVs.xy));	//Tangent space
				float3x3 matrixTangentToWorld = {
					i.tangent.x, i.bitangent.x, i.normal.x,
					i.tangent.y, i.bitangent.y, i.normal.y,
					i.tangent.z, i.bitangent.z, i.normal.z,
				};
				float3 normal = mul(matrixTangentToWorld, rippleNormal);

				//Specular
				float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
				float specularHighlight = CalculateSpecular(normal, viewDir, _Gloss);
				specularHighlight *= _LightColor0;

				//Reset normal
				normal = float3(0, 1, 0);	//So the normal is only used for specular highlights

				//Directional Lighting
				float nl = max(0, dot(normal, _WorldSpaceLightPos0));
				float4 diff = nl * _LightColor0;

				//Ambient Diffuse Lighting
				diff.rgb += ShadeSH9(float4(normal, 1));

                
				//Depth
				float depthV2 = (i.uv.xxx / 3000) + (sin(_Time.y) / 20) - 0.999;
				float4 depthC = lerp(_DeepColour, _ShallowColour, saturate(depthV2));
				
				//Fresnel
				float fresnel =  (1 / pow(2, -viewDir.y * 0.1));

				//Main colour
				float4 col = _TintColour;
				col.xyz += depthC.xyz;
				col += specularHighlight;
				col.a = fresnel;
				col *= diff;
				
				
				//Fog
				float distanceFromCamera = distance(i.worldPos.xyz, _WorldSpaceCameraPos.xyz);
				const float y = 6.9087547793152;	//log(e, 10001)
				float x = exp(y / _FogEnd);
				distanceFromCamera = pow(x, distanceFromCamera) - 1;
				distanceFromCamera /= 1000;
				distanceFromCamera = saturate(distanceFromCamera);

				float4 fogColour = CalculateFogColour(i.worldPos);

				col = lerp(col, fogColour, distanceFromCamera);
				return col;
            }
            ENDCG
        }
    }
}
