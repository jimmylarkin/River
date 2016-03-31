Shader "Custom/FlatTexturedShader" {
	Properties{
		_MainTex("Base texture", 2D) = "white" {}
	}
	SubShader{
		Pass {
			// indicate that our pass is the "base" pass in forward
        	// rendering pipeline. It gets ambient and main directional
        	// light data set up; light direction in _WorldSpaceLightPos0
        	// and color in _LightColor0
			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc" // for UnityObjectToWorldNormal
        	#include "Lighting.cginc" // for _LightColor0
            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

			//user defined variables
			uniform float4 _Color;
			// textures from shader properties
        	sampler2D _MainTex;
        	uniform float4 _MainTex_ST;

			struct v2f 
			{
				float4 pos: SV_POSITION;
				fixed3 color : COLOR0;  //vertex color
				float4 posWorld : TEXCOORD0;
				float2 uv : TEXCOORD3;
				float2 offset : TEXCOORD4;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.posWorld = mul(_Object2World, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw + v.texcoord1;
				o.color =  v.color;
				o.offset = v.texcoord1;
				// compute shadows data
                TRANSFER_SHADOW(o);
				return o;
			}

            fixed4 frag(v2f i) : SV_Target
            {
            	float3 posddx = ddx(i.posWorld.xyz);
                float3 posddy = ddy(i.posWorld.xyz);
                float3 derivedNormal = cross(normalize(posddy), normalize(posddx));
				half3 worldNormal = UnityObjectToWorldNormal(derivedNormal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                fixed3 difussedReflection = nl * _LightColor0.rgb;
                fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 finalColor = difussedReflection  * shadow + ShadeSH9(half4(worldNormal, 1));
				fixed3 baseTextelColor = tex2D(_MainTex, i.uv).rgb;
				fixed3 finalTextelColor = baseTextelColor* finalColor;	
				//return fixed4(derivedNormal, 1.0);			
                return fixed4(finalTextelColor, 1.0);
            }
			ENDCG
		}
		//shadow pass
		Pass {
			Tags {"LightMode"="ShadowCaster"}

			            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
		}
	}
}
