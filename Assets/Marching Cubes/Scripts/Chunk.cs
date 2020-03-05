using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCube
{
    public class Chunk : MonoBehaviour
    {
        //compute
        public ComputeShader marchCompute;
        //kernals
        private int flatInterpolateID;


        //buffers
        private ComputeBuffer triangleBuffer;
        private ComputeBuffer pointsBuffer;
        private ComputeBuffer trisCountBuffer;

        //data containers
        private int isoLevelID;
        private int pointsPerAxisID;

        [HideInInspector]
        public MarchingCube.Mesh MCmesh;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        [HideInInspector]
        public ZeroBounds zeroBounds;

        public Vector4[] density;
        public int densityIndex(int x, int y, int z)
        {
            return x + y * MCmesh.pointsPerAxis + z * MCmesh.pointsPerAxis * MCmesh.pointsPerAxis;
        }

        public void SetDensityCPU()
        {
            density = new Vector4[MCmesh.pointsPerAxis * MCmesh.pointsPerAxis * MCmesh.pointsPerAxis];
            for(int x = 0; x< MCmesh.pointsPerAxis; x++)
                for(int y = 0; y< MCmesh.pointsPerAxis; y++)
                    for(int z = 0; z< MCmesh.pointsPerAxis; z++)
                    {
                        float w = Vector3.Distance(transform.position + new Vector3(x * MCmesh.vertexDistance, y * MCmesh.vertexDistance, z * MCmesh.vertexDistance), new Vector3(5, 5, 5))/5;
                        density[densityIndex(x, y, z)] = new Vector4(x * MCmesh.vertexDistance, y * MCmesh.vertexDistance, z * MCmesh.vertexDistance, 1 - Mathf.Clamp01(w)); 
                    }

            if (zeroBounds.xMin)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(0, i, j)].w = 0;
            if (zeroBounds.xMax)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(MCmesh.pointsPerAxis - 1, i, j)].w = 0;
            if (zeroBounds.yMin)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(i, 0, j)].w = 0;
            if (zeroBounds.yMax)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(i, MCmesh.pointsPerAxis - 1, j)].w = 0;
            if (zeroBounds.zMin)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(i, j, 0)].w = 0;
            if (zeroBounds.zMax)
                for (int i = 0; i < MCmesh.pointsPerAxis; i++)
                    for (int j = 0; j < MCmesh.pointsPerAxis; j++)
                        density[densityIndex(i, j, MCmesh.pointsPerAxis - 1)].w = 0;
            
        }

        public void GenerateMesh(float isoLevel, Mesh.Shading shading, bool interpolation)
        {
            flatInterpolateID = marchCompute.FindKernel("CSMarchFlatInterpolate");

            isoLevelID = Shader.PropertyToID("isoLevel");
            pointsPerAxisID = Shader.PropertyToID("pointsPerAxis");

            meshFilter = GetComponent<MeshFilter>();
            // meshRenderer = GetComponent<MeshRenderer>();
            // meshCollider = GetComponent<MeshCollider>();


            if (shading == Mesh.Shading.Flat)
            {
                UpdateMesh(isoLevel, true, interpolation);
            }
            else
            {

            }
        }

        public void UpdateMesh(float isoLevel, bool flatShading, bool interpolation)
        {
            int voxelPerAxis = MCmesh.pointsPerAxis - 1;
            int threadPerAxis = Mathf.CeilToInt(voxelPerAxis / 8f);

            CreateBuffers();


            if (flatShading)
            {
                if (interpolation)
                {
                    pointsBuffer.SetData(density);

                    triangleBuffer.SetCounterValue(0);
                    marchCompute.SetBuffer(flatInterpolateID, "points", pointsBuffer);
                    marchCompute.SetBuffer(flatInterpolateID, "triangles", triangleBuffer);
                    marchCompute.SetInt(pointsPerAxisID, MCmesh.pointsPerAxis);
                    marchCompute.SetFloat(isoLevelID, isoLevel);

                    marchCompute.Dispatch(0, threadPerAxis, threadPerAxis, threadPerAxis);

                    ComputeBuffer.CopyCount(triangleBuffer, trisCountBuffer, 0);
                    int[] triCountArray = new int[1]{ 0 };
                    trisCountBuffer.GetData(triCountArray);
                    trisCountBuffer.Dispose();
                    int triCount = triCountArray[0];

                    Triangle[] tris = new Triangle[triCount];
                    triangleBuffer.GetData(tris, 0, 0, triCount);

                    UnityEngine.Mesh newMesh = new UnityEngine.Mesh();

                    Vector3[] vertices = new Vector3[triCount * 3];
                    int[] triangles = new int[triCount * 3];

                    for (int i = 0; i < triCount; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            vertices[i * 3 + j] = tris[i][j];
                            triangles[i * 3 + j] = i * 3 + j;
                        }
                    newMesh.vertices = vertices;
                    newMesh.triangles = triangles;

                    newMesh.RecalculateNormals();

                    meshFilter.sharedMesh = newMesh;

                    if (!Application.isPlaying)
                        ReleaseBuffer();
                }
            }
        }

        void CreateBuffers()
        {
            int pointsCount = MCmesh.pointsPerAxis * MCmesh.pointsPerAxis * MCmesh.pointsPerAxis;
            int voxelCount = (MCmesh.pointsPerAxis - 1);
            voxelCount = voxelCount * voxelCount * voxelCount;
            int maxTrisCount = voxelCount * 5;

            if(!Application.isPlaying || (pointsBuffer == null || pointsCount != pointsBuffer.count))
            {
                if (Application.isPlaying)
                    ReleaseBuffer();

                triangleBuffer = new ComputeBuffer(maxTrisCount, 36, ComputeBufferType.Append);
                pointsBuffer = new ComputeBuffer(pointsCount, 16);
                trisCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

                triangleBuffer.SetCounterValue(0);

                pointsBuffer = MCmesh.densityGenerator.GeneratePoints(pointsBuffer, MCmesh.pointsPerAxis, MCmesh.vertexDistance, this);
            }
        }
        void ReleaseBuffer()
        {
            if(triangleBuffer != null)
            {
                triangleBuffer.Release();
                pointsBuffer.Release();
                trisCountBuffer.Release();
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
                ReleaseBuffer();
        }
        
        struct Triangle
        {
#pragma warning disable 649 // disable unassigned variable warning
            Vector3 a;
            Vector3 b;
            Vector3 c;

            public Vector3 this [int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return a;
                        case 1:
                            return b;
                        default:
                            return c;
                    }
                }
            }
        }
    }

    public struct ZeroBounds
    {
        public bool xMin, xMax, yMin, yMax, zMin, zMax;

        public ZeroBounds(bool _xMin = false, bool _xMax = false, bool _yMin = false, bool _yMax = false, bool _zMin = false, bool _zMax = false)
        {
            xMin = _zMin;
            xMax = _xMax;
            yMin = _yMin;
            yMax = _yMax;
            zMin = _zMin;
            zMax = _zMax;
        }
    }
}
