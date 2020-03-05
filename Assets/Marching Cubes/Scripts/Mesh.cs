using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCube
{
    public class Mesh : MonoBehaviour
    {
        public bool exist;

        public enum Shading { Smooth, Flat}
        public Shading shading;

        public bool interpolation;

        public Vector3Int gridSize;

        public bool disableChunkSelection = false;

        public float chunkSize;
        public float vertexDistance;
        public int pointsPerAxis;

        public bool bounded;

        public DensityGenerator densityGenerator;

        [Range(0, 1)]
        public float isoLevel;

        Chunk[,,] chunks;

        public GameObject chunkPrefab;

        public bool drawGizmos;

        public bool autoUpdateEditor;
        public bool autoUpdateGame;


        public void GenerateEditor()
        {
            pointsPerAxis = Mathf.FloorToInt(chunkSize / vertexDistance) + 1;
            if (exist)
                ResetChunks();
            exist = true;
            chunks = new Chunk[gridSize.x, gridSize.y, gridSize.z];

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

                        chunks[x, y, z] = c;

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

                        c.GenerateMesh(isoLevel, shading, interpolation);
                    }
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
            chunks = new Chunk[0,0,0];
        }

    }
}