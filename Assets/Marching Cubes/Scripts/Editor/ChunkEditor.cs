using UnityEngine;
using UnityEditor;

namespace MarchingCube
{
    [CustomEditor(typeof(Chunk))]
    public class ChunkEditor : Editor
    {
        Chunk chunk;
        private void OnEnable()
        {
            chunk = (Chunk)target;
            if(chunk.MCmesh.disableChunkSelection)
                Selection.activeObject = chunk.MCmesh.gameObject;
        }
    }
}
