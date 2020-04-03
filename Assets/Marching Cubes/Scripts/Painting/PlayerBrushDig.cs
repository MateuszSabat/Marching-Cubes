using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    [CreateAssetMenu(fileName = "New Player Brush Dig", menuName = "Marching Cubes/Brush/Player Brush Dig")]
    public class PlayerBrushDig : Brush
    {
        public float[] forces;

        public override void PaintChunk(ComputeBuffer density, ComputeBuffer substances, Chunk chunk, Vector3 point)
        {
            ComputeBuffer forceBuffer = new ComputeBuffer(forces.Length, 4);
            forceBuffer.SetData(forces);

            paintCompute.SetBuffer(0, "points", density);
            paintCompute.SetBuffer(0, "substances", substances);
            paintCompute.SetBuffer(0, "force", forceBuffer);

            paintCompute.SetInt("pointsPerAxis", MCMesh.pointsPerAxis);
            paintCompute.SetVector("localCenter", point - chunk.transform.position);
            paintCompute.SetFloat("radious", radious);

            paintCompute.SetFloat("smooth", smooth);

            paintCompute.SetInt("smoothType", (int)smoothType);

            paintCompute.SetFloat("deltaTime", Time.deltaTime);

            paintCompute.SetBool("xMin", chunk.zeroBounds.xMin);
            paintCompute.SetBool("xMax", chunk.zeroBounds.xMax);
            paintCompute.SetBool("yMin", chunk.zeroBounds.yMin);
            paintCompute.SetBool("yMax", chunk.zeroBounds.yMax);
            paintCompute.SetBool("zMin", chunk.zeroBounds.zMin);
            paintCompute.SetBool("zMax", chunk.zeroBounds.zMax);

            int threadGroup = Mathf.CeilToInt(MCMesh.pointsPerAxis / 8f);

            paintCompute.Dispatch(0, threadGroup, threadGroup, threadGroup);
        }
    }
}
