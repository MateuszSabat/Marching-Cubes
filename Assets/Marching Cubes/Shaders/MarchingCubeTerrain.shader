Shader "MarchingCubes/MarchingCubeTerrain"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200


		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		struct Substance {
			float r;
			float g;
			float b;
		};

		struct Input			
		{
			fixed3 color;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.color = v.color; //vertexColor
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{
			o.Albedo = IN.color;
		}
		ENDCG
    }
}
