using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    [CreateAssetMenu(fileName = "New Player Brush Add", menuName = "Marching Cubes/Brush/Player Brush Add")]
    public class PlayerBrushAdd : Brush
    {
        public override void PaintChunk(ComputeBuffer density, ComputeBuffer substances, Chunk chunk, Vector3 point)
        {
            paintCompute.SetBuffer(0, "points", density);
            paintCompute.SetBuffer(0, "substances", substances);
            paintCompute.SetInt("pointsPerAxis", MCMesh.pointsPerAxis);
            paintCompute.SetVector("localCenter", point - chunk.transform.position);
            paintCompute.SetFloat("radious", radious);
            paintCompute.SetFloat("force", set * Time.deltaTime);
            paintCompute.SetFloat("add", dPaintStyle == DensnityPaintStyle.Add ? 1.0f : 0.0f);
            paintCompute.SetFloat("smooth", smooth);
            paintCompute.SetInt("smoothType", (int)smoothType);
            paintCompute.SetInt("substance", substance);
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
