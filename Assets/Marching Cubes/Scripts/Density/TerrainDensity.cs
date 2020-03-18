using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MarchingCubes {
    [CreateAssetMenu(fileName = "TerrainDensityGenerator", menuName = "Marching Cubes/Density Generators/Terrain")]
    public class TerrainDensity : DensityGenerator
    {

        public float amplitude;
        public int octaves;
        public float lacunarity;
        public float persistance;

        public float add;

        public float wholeHeight;

        public override void GeneratePoints(ComputeBuffer pointsBuffer, ComputeBuffer substancesBuffer, int pointsPerAxis, float vertexDistance, Chunk chunk)
        {
            densityCompute.SetFloat("amplitude", amplitude);
            densityCompute.SetInt("octaves", octaves);
            densityCompute.SetFloat("lacunarity", lacunarity);
            densityCompute.SetFloat("persistance", persistance);

            densityCompute.SetFloat("add", add);

            densityCompute.SetFloat("inverseWholeHeight", 1/wholeHeight);

            base.GeneratePoints(pointsBuffer, substancesBuffer, pointsPerAxis, vertexDistance, chunk);
        }
    }
}
