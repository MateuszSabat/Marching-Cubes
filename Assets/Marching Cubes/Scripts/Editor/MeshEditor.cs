using UnityEngine;
using UnityEditor;

namespace MarchingCube
{
    [CustomEditor(typeof(Mesh))]
    public class MeshEditor : Editor
    {
        Mesh mesh;

        SerializedProperty shading;
        SerializedProperty interpolation;
        SerializedProperty gridSize;
        SerializedProperty chunkSize;
        SerializedProperty vertexDistance;
        SerializedProperty pointsPerAxis;
        SerializedProperty bounded;
        SerializedProperty densityGenerator;
        SerializedProperty isoLevel;
        SerializedProperty chunkPrefab;
        SerializedProperty autoUpdateEditor;
        SerializedProperty autoUpdateGame;

        private void OnEnable()
        {
            mesh = (Mesh)target;

            shading = serializedObject.FindProperty("shading");
            interpolation = serializedObject.FindProperty("interpolation");
            gridSize = serializedObject.FindProperty("gridSize");
            chunkSize = serializedObject.FindProperty("chunkSize");
            vertexDistance = serializedObject.FindProperty("vertexDistance");
            pointsPerAxis = serializedObject.FindProperty("pointsPerAxis");
            bounded = serializedObject.FindProperty("bounded");
            densityGenerator = serializedObject.FindProperty("densityGenerator");
            isoLevel = serializedObject.FindProperty("isoLevel");
            chunkPrefab = serializedObject.FindProperty("chunkPrefab");
            autoUpdateEditor = serializedObject.FindProperty("autoUpdateEditor");
            autoUpdateGame = serializedObject.FindProperty("autoUpdateGame");

        }

        public override void OnInspectorGUI()
        {
            bool changedChunkSize = false;
            bool changedVertexDistance = false;
            bool changedPointsPerAxis = false;

            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(shading);
            EditorGUILayout.PropertyField(interpolation);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(gridSize);
            EditorGUILayout.PropertyField(bounded);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Chunk", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(chunkSize);
            if (EditorGUI.EndChangeCheck())
                changedChunkSize = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(vertexDistance);
            if (EditorGUI.EndChangeCheck())
                changedVertexDistance = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pointsPerAxis);
            if (EditorGUI.EndChangeCheck())
                changedPointsPerAxis = true;
            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(chunkPrefab);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Density", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(densityGenerator);

            EditorGUILayout.PropertyField(isoLevel);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Update Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoUpdateEditor);
            EditorGUILayout.PropertyField(autoUpdateGame);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(5f);
            if (mesh.exist)
            {
                if (GUILayout.Button("Regenerate"))
                    mesh.GenerateEditor();
                if (GUILayout.Button("Reset"))
                    mesh.ResetChunks();
            }
            else
            {
                if (GUILayout.Button("Generate"))
                    mesh.GenerateEditor();
            }

            if (changedChunkSize)
            {
                changedVertexDistance = true;
            }
            if (changedVertexDistance)
            {
                if (mesh.vertexDistance <= 0.001f)
                    mesh.vertexDistance = 0.001f;
                if (mesh.vertexDistance > mesh.chunkSize)
                    mesh.vertexDistance = mesh.chunkSize;
                mesh.pointsPerAxis = Mathf.FloorToInt(mesh.chunkSize / mesh.vertexDistance) + 1;
                changedPointsPerAxis = true;
            }
            if (changedPointsPerAxis)
            {
                if (mesh.pointsPerAxis <= 1)
                    mesh.pointsPerAxis = 2;
                mesh.vertexDistance = mesh.chunkSize / (mesh.pointsPerAxis - 1);
            }
        }
    }
}
