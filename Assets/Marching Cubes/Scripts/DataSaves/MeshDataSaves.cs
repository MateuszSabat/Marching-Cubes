using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Data Save", menuName = "Marching Cubes/Mesh Data Save")]
    public class MeshDataSaves : ScriptableObject
    {
        public int chunkCount;
        public int pointsPerChunk;
        public Vector4[] points;
        public int[] substances;
    }
}
