Shader "Custom/FlatVertexColorShader" {
	Properties{
	}
		SubShader{
			Pass {
				Tags {"LightMode"="ForwardBase"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc" // for UnityObjectToWorldNormal
            	#include "UnityLightingCommon.cginc" // for _LightColor0

				//user defined variables
				uniform float4 _Color;

				struct v2f 
				{
					float4 pos: SV_POSITION;
					fixed3 color : COLOR0;
					float4 posWorld : TEXCOORD1;
					float4 normal : TEXCOORD2;
				};

				v2f vert(appdata_full v)
				{
					v2f o;
					o.posWorld = mul(_Object2World, v.vertex);
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					// half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					// half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					// fixed3 difussedReflection = nl * _LightColor0;
					// fixed3 finalColor = difussedReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
					o.color =  v.color;// * _Color.rgb;// * v.color;
					return o;
				}

	            fixed4 frag(v2f i) : COLOR
	            {
	            	float3 posddx = ddx(i.posWorld.xyz);
                    float3 posddy = ddy(i.posWorld.xyz);
                    float3 derivedNormal = cross(normalize(posddy), normalize(posddx));
					half3 worldNormal = UnityObjectToWorldNormal(derivedNormal);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                    fixed3 difussedReflection = nl * _LightColor0.rgb;
					fixed3 finalColor = difussedReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
                    return fixed4(finalColor * i.color, 1.0);
                    //return fixed4(i.color, 1.0);
	            }
			ENDCG
		}
	}
}
