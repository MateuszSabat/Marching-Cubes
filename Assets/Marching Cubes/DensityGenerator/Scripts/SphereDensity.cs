using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCube {
    [CreateAssetMenu(fileName = "SphereDensityGenerator", menuName = "DensityGenerators/Sphere")]
    public class SphereDensity : DensityGenerator
    {
        public float radious;
        public Vector3 center;

        public override ComputeBuffer GeneratePoints(ComputeBuffer pointsBuffer, int pointsPerAxis, float vertexDistance, Chunk chunk)
        {
            Vector4 t = new Vector4(center.x, center.y, center.z, radious);
            densityCompute.SetVector("transform", t);

            return base.GeneratePoints(pointsBuffer, pointsPerAxis, vertexDistance, chunk);
        }
    }
}
