using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    [CreateAssetMenu(fileName = "New Brush", menuName = "Marching Cubes/Brush/EditorBrush")]
    public class Brush : ScriptableObject
    {
        public Mesh MCMesh;

        public float radious;

        public ComputeShader paintCompute;

        public float set;
        public float smooth;
        public int substance;

        public bool changeSubstance;

        public enum DensnityPaintStyle {Add, Set}
        public DensnityPaintStyle dPaintStyle;

        public enum SmoothType { None, Linear};
        public SmoothType smoothType;

        public ComputeShader imageCompute;

        public void Paint(Vector3 point)
        {
            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            if (MCMesh.chunks == null)
                MCMesh.chunks = MCMesh.GetComponentsInChildren<Chunk>();
            float radiousSqr = radious * radious;
            foreach(Chunk c in MCMesh.chunks)
                if (SqrDistanceFrom(c, point) <= radiousSqr)
                    c.Paint(this, point);
        }

        public virtual void PaintChunk(ComputeBuffer density, ComputeBuffer substances, Chunk chunk, Vector3 point)
        {
            paintCompute.SetBuffer(0, "points", density);
            paintCompute.SetBuffer(0, "substances", substances);
            paintCompute.SetInt("pointsPerAxis", MCMesh.pointsPerAxis);
            paintCompute.SetVector("localCenter", point - chunk.transform.position);
            paintCompute.SetFloat("radious", radious);
            if(Application.isPlaying)
                paintCompute.SetFloat("set", set * Time.deltaTime);
            else
                paintCompute.SetFloat("set", set);
            paintCompute.SetFloat("add", dPaintStyle == DensnityPaintStyle.Add ? 1.0f : 0.0f);
            paintCompute.SetFloat("smooth", smooth);
            paintCompute.SetInt("smoothType", (int)smoothType);
            paintCompute.SetInt("substance", substance);
            paintCompute.SetBool("changeSubstance", changeSubstance);
            paintCompute.SetBool("xMin", chunk.zeroBounds.xMin);
            paintCompute.SetBool("xMax", chunk.zeroBounds.xMax);
            paintCompute.SetBool("yMin", chunk.zeroBounds.yMin);
            paintCompute.SetBool("yMax", chunk.zeroBounds.yMax);
            paintCompute.SetBool("zMin", chunk.zeroBounds.zMin);
            paintCompute.SetBool("zMax", chunk.zeroBounds.zMax);

            int threadGroup = Mathf.CeilToInt(MCMesh.pointsPerAxis / 8f);

            paintCompute.Dispatch(0, threadGroup, threadGroup, threadGroup);
        }


        public float SqrDistanceFrom(Chunk c, Vector3 to)
        {
            float dx = 0, dy = 0, dz = 0;

            if (to.x < c.transform.position.x)
                dx = c.transform.position.x - to.x;
            else if (to.x > c.transform.position.x + MCMesh.chunkSize)
                dx = to.x - c.transform.position.x - MCMesh.chunkSize;

            if (to.y < c.transform.position.y)
                dy = c.transform.position.y - to.y;
            else if (to.y > c.transform.position.y + MCMesh.chunkSize)
                dy = to.y - c.transform.position.y - MCMesh.chunkSize;

            if (to.z < c.transform.position.z)
                dz = c.transform.position.z - to.z;
            else if (to.z > c.transform.position.z + MCMesh.chunkSize)
                dz = to.z - c.transform.position.z - MCMesh.chunkSize;

            return dx * dx + dy * dy + dz * dz;
        }

        public void ChangeRadious(float delta)
        {
            radious -= delta * (radious + 1f )/ 10;
            if (radious < 0)
                radious = 0;
        }
    }
}
