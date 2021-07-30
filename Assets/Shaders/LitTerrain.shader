Shader "Custom/Lit/LitTerrain"
{
	Properties
	{
		_ColourOne ("Colour 1", Color) = (1,1,1,1)
		_ColourTwo ("Colour 2", Color) = (0,0,0,0)
		_TopFog("Top Fog", Color) = (1,1,1,1)
		_MidFog("Mid Fog", Color) = (1,1,1,1)
		_BottomFog("Bottom Fog", Color) = (1,1,1,1)
		_FogEnd("Fog End", Float) = 10000

	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{

			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"	//Gets the _LightColor0

			float4 _ColourOne;
			float4 _ColourTwo;
			float _FogEnd;
			float4 _TopFog;
			float4 _MidFog;
			float4 _BottomFog;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 diff : COLOR0;

            };

			

			float4 GetGradientColour(float posY)
			{
				posY -= 3000;
				float maxHeight = 4000;
				float normalisedHeight = posY / maxHeight;
				float4 col = lerp(_ColourTwo, _ColourOne, normalisedHeight);
				return col;
			}
			float4 GetNormalColour(float normalY, float posY)
			{
				float4 colOne = _ColourOne;
				float4 colTwo = _ColourTwo;// lerp(_ColourTwo, float4(0.5, 0.5, 0.5, 1), posY / 35000);
				float lerpMod = -0.5;
				
				
				float4 col = lerp(colTwo, colOne, clamp(normalY + lerpMod, 0, 1));
				return col;
			}

            v2f vert (appdata v)
            {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = v.normal;
				o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
				//This calculates the lighting
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float nl = max(0.1, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				//This calculates ambient lighting into the diffuse
				o.diff.rgb += ShadeSH9(float4(worldNormal, 1));
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

			float4 frag(v2f i) : SV_Target
			{

				//i.diff *= GetGradientColour(i.vertWorldPos.y);
				i.diff *= GetNormalColour(i.normal.y, i.worldPos.y);
				
				//Fog
				float distanceFromCamera = distance(i.worldPos.xyz, _WorldSpaceCameraPos.xyz);
				const float y = 6.9087547793152;	//log(e, 10001)
				float x = exp(y / _FogEnd);
				distanceFromCamera = pow(x, distanceFromCamera) - 1;
				distanceFromCamera /= 1000;
				distanceFromCamera = saturate(distanceFromCamera);

				float4 fogColour = CalculateFogColour(i.worldPos);
				
				i.diff = lerp(i.diff, fogColour, distanceFromCamera);
				return i.diff;
			}
				ENDCG
	}
	}
}
