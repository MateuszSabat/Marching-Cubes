using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes {
    [CreateAssetMenu(fileName = "SphereDensityGenerator", menuName = "Marching Cubes/Density Generators/Sphere")]
    public class SphereDensity : DensityGenerator
    {
        public float radious;
        public Vector3 center;

        public override void GeneratePoints(ComputeBuffer pointsBuffer, ComputeBuffer substancesBuffer, int pointsPerAxis, float vertexDistance, Chunk chunk)
        {
            Vector4 t = new Vector4(center.x, center.y, center.z, radious);
            densityCompute.SetVector("transform", t);

            base.GeneratePoints(pointsBuffer, substancesBuffer, pointsPerAxis, vertexDistance, chunk);
        }
    }
}
