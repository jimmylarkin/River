Shader "Custom/FlatShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 vertexColor;
		};

		fixed4 _Color;

	    void vert (inout appdata_full v, out Input o) {
	        UNITY_INITIALIZE_OUTPUT(Input, o);
	        o.vertexColor = v.color;
	    }

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;			
		    float3 posddx = ddx(IN.worldPos.xyz);
	        float3 posddy = ddy(IN.worldPos.xyz);
	        float3 derivedNormal = cross(normalize(posddy), normalize(posddx));
			half3 worldNormal = UnityObjectToWorldNormal(derivedNormal);
			o.Albedo = IN.vertexColor;
			o.Normal = derivedNormal;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
