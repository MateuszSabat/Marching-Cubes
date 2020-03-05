using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MarchingCube {
    [CreateAssetMenu(fileName = "TerrainDensityGenerator", menuName = "DensityGenerators/Terrain")]
    public class TerrainDensity : DensityGenerator
    {

        public float amplitude;
        public int octaves;
        public float lacunarity;
        public float persistance;

        public float add;

        public override ComputeBuffer GeneratePoints(ComputeBuffer pointsBuffer, int pointsPerAxis, float vertexDistance, Chunk chunk)
        {
            densityCompute.SetFloat("amplitude", amplitude);
            densityCompute.SetInt("octaves", octaves);
            densityCompute.SetFloat("lacunarity", lacunarity);
            densityCompute.SetFloat("persistance", persistance);

            densityCompute.SetFloat("add", add);

            return base.GeneratePoints(pointsBuffer, pointsPerAxis, vertexDistance, chunk);
        }
    }
}
