using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    public class Mesh : MonoBehaviour
    {
        public static bool shaderSet = false;
        ComputeBuffer sBuff;
        public void Awake()
        {
            GenerateEditor();
        }
        private void SetShaderParams()
        {
            if (!shaderSet)
            {
                shaderSet = true;
                if (SubstanceTable.substances == null)
                    SubstanceTable.Init();
                if (sBuff != null)
                    sBuff.Release();
                List<Substance> s = SubstanceTable.substances;
                sBuff = new ComputeBuffer(s.Count, 12);

                Vector3[] sData = new Vector3[s.Count];
                for (int i = 0; i < s.Count; i++)
                {
                    sData[i].x = s[i].r;
                    sData[i].y = s[i].g;
                    sData[i].z = s[i].b;
                }
                sBuff.SetData(sData);

                Shader.SetGlobalBuffer("substances", sBuff);
                Shader.SetGlobalInt("pointsPerAxis", pointsPerAxis);
                Shader.SetGlobalFloat("vertexDistance", vertexDistance);
            }
        }

        public bool exist;

        public enum Shading { Smooth, Flat}
        public Shading shading;

        public Vector3Int gridSize;

        public float chunkSize;
        public float vertexDistance;
        public int pointsPerAxis;

        public bool bounded;

        public DensityGenerator densityGenerator;

        [Range(0, 1)]
        public const float isoLevel = 0.5f;

        public Chunk[] chunks;

        public GameObject chunkPrefab;

        public bool drawGizmos;

        public bool autoUpdateEditor;
        public bool autoUpdateGame;

        public Brush brush;

        public MeshDataSaves dataSave;

        private void OnEnable()
        {
            if (chunks == null)
                chunks = GetComponentsInChildren<Chunk>();
            if (brush != null)
                brush.MCMesh = this;
        }

        public void GenerateEditor()
        {
            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            pointsPerAxis = Mathf.FloorToInt(chunkSize / vertexDistance) + 1;
            if (exist)
                ResetChunks();
            exist = true;

            densityGenerator.GenerateGradient();

            for(int x=0; x<gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        Transform chunk = Instantiate(chunkPrefab).transform;
                        chunk.position = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                        chunk.rotation = Quaternion.identity;
                        chunk.parent = transform;

                        Chunk c = chunk.GetComponent<Chunk>();

                        c.zeroBounds = new ZeroBounds();

                        c.MCmesh = this;

                        if (bounded)
                        {
                            if (x == 0)
                                c.zeroBounds.xMin = true;
                            if (x == gridSize.x - 1)
                                c.zeroBounds.xMax = true;
                            if (y == 0)
                                c.zeroBounds.yMin = true;
                            if (y == gridSize.y-1)
                                c.zeroBounds.yMax = true;
                            if (z == 0)
                                c.zeroBounds.zMin = true;
                            if (z == gridSize.z - 1)
                                c.zeroBounds.zMax = true;
                        }

                        c.GenerateMesh(isoLevel, shading);
                    }
            chunks = GetComponentsInChildren<Chunk>();
            SetShaderParams();
            densityGenerator.ReleaseBuffers();
        }
        public void ResetChunks()
        {
            Chunk[] ch = GetComponentsInChildren<Chunk>();
            foreach(Chunk c in ch)
            {
                if (Application.isPlaying)
                    Destroy(c.gameObject);
                else
                    DestroyImmediate(c.gameObject);
            }
            exist = false;
            chunks = new Chunk[0];
        }

        public void UpdateChunks()
        {
            if (chunks == null)
                chunks = GetComponentsInChildren<Chunk>();
            foreach (Chunk c in chunks)
            {
                c.UpdateMesh(isoLevel, shading == Shading.Flat);
            }
        }

        private void OnDestroy()
        {
            sBuff.Release();
        }


        public void SaveToFile()
        {
            dataSave.chunkCount = chunks.Length;
            dataSave.pointsPerChunk = chunks[0].density.Length;
            dataSave.points = new Vector4[chunks.Length * chunks[0].density.Length];
            dataSave.substances = new int[chunks.Length * chunks[0].substances.Length];

            for(int i=0; i<chunks.Length; i++)
                for (int j = 0; j < chunks[0].density.Length; j++) {
                    dataSave.points[i * dataSave.pointsPerChunk + j] = chunks[i].density[j];
                    dataSave.substances[i * dataSave.pointsPerChunk + j] = chunks[i].substances[j];
                }

            EditorUtility.SetDirty(dataSave);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void LoadFromFile()
        {
            for (int i = 0; i < chunks.Length; i++) {
                chunks[i].density = new Vector4[dataSave.pointsPerChunk];
                chunks[i].substances = new int[dataSave.pointsPerChunk];
                for (int j = 0; j < chunks[0].density.Length; j++)
                {
                    chunks[i].density[j] = dataSave.points[i * dataSave.pointsPerChunk + j];
                    chunks[i].substances[j] = dataSave.substances[i * dataSave.pointsPerChunk + j];
                }
            }
        }


    }
}