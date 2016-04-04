Shader "Custom/FlatShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_ColorTint ("Tint", Color) = (1.0, 0.6, 0.6, 1.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert fullforwardshadows vertex:vert finalcolor:mycolor

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		sampler2D _MainTex;
		fixed4 _ColorTint;

		void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
	    {
	    	color *= _ColorTint;
	    }
		
	    void vert (inout appdata_full v, out Input o) {
	        UNITY_INITIALIZE_OUTPUT(Input, o);
	    }

		void surf (Input IN, inout SurfaceOutput o) {		
		    float3 posddx = ddx(IN.worldPos.xyz);
	        float3 posddy = ddy(IN.worldPos.xyz);
	        float3 derivedNormal = cross(normalize(posddy), normalize(posddx));
			half3 worldNormal = UnityObjectToWorldNormal(derivedNormal);
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
			o.Normal = worldNormal;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
