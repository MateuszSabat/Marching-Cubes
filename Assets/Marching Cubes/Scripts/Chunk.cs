using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MarchingCubes
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
        private ComputeBuffer substancesBuffer;

        //data containers
        private int isoLevelID;
        private int pointsPerAxisID;

        [HideInInspector]
        public MarchingCubes.Mesh MCmesh;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material material;
        private MeshCollider meshCollider;

        [HideInInspector]
        public ZeroBounds zeroBounds;

        public Vector4[] density;
        public int[] substances;

        public int densityIndex(int x, int y, int z)
        {
            return x + y * MCmesh.pointsPerAxis + z * MCmesh.pointsPerAxis * MCmesh.pointsPerAxis;
        }

        public void GenerateMesh(float isoLevel, Mesh.Shading shading)
        {
            flatInterpolateID = marchCompute.FindKernel("CSMarchFlatInterpolate");

            isoLevelID = Shader.PropertyToID("isoLevel");
            pointsPerAxisID = Shader.PropertyToID("pointsPerAxis");

            meshFilter = GetComponent<MeshFilter>();

            meshRenderer = GetComponent<MeshRenderer>();
            material = new Material(Shader.Find("MarchingCubes/MarchingCubeTerrain"));
            meshRenderer.sharedMaterial = material;


            if (shading == Mesh.Shading.Flat)
            {
                CreateBuffers();
                MCmesh.densityGenerator.GeneratePoints(pointsBuffer, substancesBuffer, MCmesh.pointsPerAxis, MCmesh.vertexDistance, this);

                CreateMesh(isoLevel, true);
            }
            else
            {

            }
            if (meshCollider != null)
                Destroy(meshCollider);
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        public void CreateMesh(float isoLevel, bool flatShading)
        {
            int voxelPerAxis = MCmesh.pointsPerAxis - 1;
            int threadPerAxis = Mathf.CeilToInt(voxelPerAxis / 8f);            

            if (flatShading)
            {
                triangleBuffer.SetCounterValue(0);
                marchCompute.SetBuffer(flatInterpolateID, "points", pointsBuffer);
                marchCompute.SetBuffer(flatInterpolateID, "triangles", triangleBuffer);
                marchCompute.SetBuffer(flatInterpolateID, "substances", substancesBuffer);
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
                Color[] colors = new Color[triCount * 3];
                int[] triangles = new int[triCount * 3];

                for (int i = 0; i < triCount; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        vertices[i * 3 + j] = tris[i][j];
                        colors[i * 3 + j] = SubstanceTable.substances[tris[i].s].color;
                        triangles[i * 3 + j] = i * 3 + j;
                    }
                newMesh.vertices = vertices;
                newMesh.colors = colors;
                newMesh.triangles = triangles;

                newMesh.RecalculateNormals();

                meshFilter.sharedMesh = newMesh;

                material.SetBuffer("pointSubstances", substancesBuffer);

                pointsBuffer.GetData(density);
                substancesBuffer.GetData(substances);

                if (!Application.isPlaying)
                    ReleaseBuffer();
            }
        }

        public void UpdateMesh(float isoLevel, bool flatShading)
        {
            int voxelPerAxis = MCmesh.pointsPerAxis - 1;
            int threadPerAxis = Mathf.CeilToInt(voxelPerAxis / 8f);
            int voxels = voxelPerAxis * voxelPerAxis * voxelPerAxis;
            int maxTrisCount = voxels * 5;

            pointsBuffer = new ComputeBuffer(substances.Length, 16);
            pointsBuffer.SetData(density);
            substancesBuffer = new ComputeBuffer(substances.Length, sizeof(int));
            substancesBuffer.SetData(substances);

            triangleBuffer = new ComputeBuffer(maxTrisCount, 36 + sizeof(int), ComputeBufferType.Append);
            trisCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            triangleBuffer.SetCounterValue(0);


            if (flatShading)
            {
                triangleBuffer.SetCounterValue(0);
                marchCompute.SetBuffer(flatInterpolateID, "points", pointsBuffer);
                marchCompute.SetBuffer(flatInterpolateID, "triangles", triangleBuffer);
                marchCompute.SetBuffer(flatInterpolateID, "substances", substancesBuffer);
                marchCompute.SetInt(pointsPerAxisID, MCmesh.pointsPerAxis);
                marchCompute.SetFloat(isoLevelID, isoLevel);

                marchCompute.Dispatch(0, threadPerAxis, threadPerAxis, threadPerAxis);

                ComputeBuffer.CopyCount(triangleBuffer, trisCountBuffer, 0);
                int[] triCountArray = new int[1] { 0 };
                trisCountBuffer.GetData(triCountArray);
                trisCountBuffer.Dispose();
                int triCount = triCountArray[0];

                Triangle[] tris = new Triangle[triCount];
                triangleBuffer.GetData(tris, 0, 0, triCount);

                UnityEngine.Mesh newMesh = new UnityEngine.Mesh();

                Vector3[] vertices = new Vector3[triCount * 3];
                Color[] colors = new Color[triCount * 3];
                int[] triangles = new int[triCount * 3];

                for (int i = 0; i < triCount; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        vertices[i * 3 + j] = tris[i][j];
                        colors[i * 3 + j] = SubstanceTable.substances[tris[i].s].color;
                        triangles[i * 3 + j] = i * 3 + j;
                    }
                newMesh.vertices = vertices;
                newMesh.colors = colors;
                newMesh.triangles = triangles;

                newMesh.RecalculateNormals();

                meshFilter.sharedMesh = newMesh;

                if (!Application.isPlaying)
                    ReleaseBuffer();
            }


            if (Application.isPlaying)
                Destroy(meshCollider);
            else
                DestroyImmediate(meshCollider);

            meshCollider = gameObject.AddComponent<MeshCollider>();
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
                if (substancesBuffer != null)
                    substancesBuffer.Release();

                triangleBuffer = new ComputeBuffer(maxTrisCount, 36 + sizeof(int), ComputeBufferType.Append);
                pointsBuffer = new ComputeBuffer(pointsCount, 16);
                substancesBuffer = new ComputeBuffer(pointsCount, sizeof(int));
                trisCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

                triangleBuffer.SetCounterValue(0);
                
                density = new Vector4[pointsCount];
                substances = new int[pointsCount];
            }
        }
        void ReleaseBuffer()
        {
            if(triangleBuffer != null)
            {
                triangleBuffer.Release();
                pointsBuffer.Release();
                substancesBuffer.Release();
                trisCountBuffer.Release();
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
                ReleaseBuffer();
        }

        public void Paint(Brush b, Vector3 point)
        {
            pointsBuffer = new ComputeBuffer(substances.Length, 16);
            pointsBuffer.SetData(density);

            substancesBuffer = new ComputeBuffer(substances.Length, sizeof(int));
            substancesBuffer.SetData(substances);

            b.PaintChunk(pointsBuffer, substancesBuffer, this, point);

            pointsBuffer.GetData(density);
            substancesBuffer.GetData(substances);

            bool shading = MCmesh.shading == Mesh.Shading.Flat ? true : false;
            UpdateMesh(Mesh.isoLevel, shading);
        }
        
        struct Triangle
        {
#pragma warning disable 649 // disable unassigned variable warning
            Vector3 a;
            Vector3 b;
            Vector3 c;
            public int s;

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

    [System.Serializable]
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
