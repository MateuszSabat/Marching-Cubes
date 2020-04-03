Shader "MarchingCubes/MarchingCubeTerrain"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200


		CGPROGRAM
		#pragma surface surf Standard vertex:vert addshadow

		struct Substance {
			float r;
			float g;
			float b;
		};

		struct Input			
		{
			half3 color;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.color = v.color;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = IN.color;
		}
		ENDCG
    }
}
