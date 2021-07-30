Shader "Custom/Unlit/GradientSkybox"
{
	Properties
	{
		_TopColour("Top Colour", Color) = (0,0,0,1)
		_MidColour("Mid Colour", Color) = (1,1,1,1)
		_BottomColour("Bottom Colour", Color) = (0,0,1,1)
		_SunSize("Sun Size", Float) = 500

	}
		SubShader
	{
		Tags { "RenderType" = "Background" "Queue" = "Background" }

		Pass
		{
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"	//Gets the _LightColor0

			float4 _TopColour;
			float4 _MidColour;
			float4 _BottomColour;
			float _SunSize;


            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD1;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
                return o;
            }

			float CalculateSunlight(float3 viewDirection)
			{
				float3 sunDirection = _WorldSpaceLightPos0.xyz;	//Direction to the sun
				float angle = max(0, dot(viewDirection, sunDirection));
				float highlight = pow(angle, _SunSize) * 3;
				highlight *= _LightColor0;
				return highlight;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 colour;
				float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
				float3 vectorUp = float3(0, 1, 0);
				float viewDot = dot(viewDir, vectorUp);
				if (viewDot >= 0)
				{
					float t = saturate(viewDot * 3);
					
					colour = lerp(_MidColour, _TopColour, t);
				}
				else
				{
					float t = saturate(-viewDot * 3);
					
					colour = lerp(_MidColour, _BottomColour, t);
				}
				colour += CalculateSunlight(viewDir);
				return colour;
			}
		ENDCG
	}
	}
}
