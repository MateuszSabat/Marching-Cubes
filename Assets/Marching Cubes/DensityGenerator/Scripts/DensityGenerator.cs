using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCube
{
    public abstract class DensityGenerator : ScriptableObject
    {
        const int threadGroupSize = 8;
        public ComputeShader gradientGenerator;
        public ComputeShader densityCompute;

        private ComputeBuffer gradients;
        [Header("Noise")]
        public int seed;
        public float size;
        public int gradientPointsPerAxis;

        public void GenerateGradient()
        {
            gradients = new ComputeBuffer(gradientPointsPerAxis * gradientPointsPerAxis * gradientPointsPerAxis, 12);
            gradientGenerator.SetInt("seed", seed);
            gradientGenerator.SetInt("gradientPointPerAxis", gradientPointsPerAxis);
            gradientGenerator.SetBuffer(0, "gradients", gradients);

            int threadGroup = Mathf.CeilToInt(gradientPointsPerAxis / 8f);

            gradientGenerator.Dispatch(0, threadGroup, threadGroup, threadGroup);
        }

        public virtual ComputeBuffer GeneratePoints(ComputeBuffer pointsBuffer, int pointsPerAxis, float vertexDistance, Chunk chunk)
        {
            int threadsPerAxis = Mathf.CeilToInt(pointsPerAxis / (float)threadGroupSize);

            densityCompute.SetFloat("size", size);
            densityCompute.SetBuffer(0, "gradients", gradients);
            densityCompute.SetInt("gradientPointPerAxis", gradientPointsPerAxis);
            densityCompute.SetBuffer(0, "points", pointsBuffer);
            densityCompute.SetVector("chunkPos", chunk.transform.position);
            densityCompute.SetInt("pointsPerAxis", pointsPerAxis);
            densityCompute.SetFloat("vertexDistance", vertexDistance);
            densityCompute.SetBool("xMin", chunk.zeroBounds.xMin);
            densityCompute.SetBool("xMax", chunk.zeroBounds.xMax);
            densityCompute.SetBool("yMin", chunk.zeroBounds.yMin);
            densityCompute.SetBool("yMax", chunk.zeroBounds.yMax);
            densityCompute.SetBool("zMin", chunk.zeroBounds.zMin);
            densityCompute.SetBool("zMax", chunk.zeroBounds.zMax);

            densityCompute.Dispatch(0, threadsPerAxis, threadsPerAxis, threadsPerAxis);

            return pointsBuffer;
        }

        public void ReleaseBuffers()
        {
            gradients.Release();
        }
    }
}