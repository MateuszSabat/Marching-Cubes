using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    [CustomEditor(typeof(Mesh))]
    public class MeshEditor : Editor
    {
        bool showMeshSettings;

        bool showGrid;

        bool showChunks;

        Editor densityEditor;
        bool showDensity;

        bool showUpdateSettings;

        Editor brushEditor;
        bool showBrush;

        Mesh mesh;

        SerializedProperty shading;
        SerializedProperty gridSize;
        SerializedProperty chunkSize;
        SerializedProperty vertexDistance;
        SerializedProperty pointsPerAxis;
        SerializedProperty bounded;
        SerializedProperty densityGenerator;
       // SerializedProperty isoLevel;
        SerializedProperty chunkPrefab;
        SerializedProperty autoUpdateEditor;
        SerializedProperty autoUpdateGame;
        SerializedProperty brush;
        SerializedProperty dataSaves;

        bool sculpt;

        private void OnEnable()
        {
            mesh = (Mesh)target;

            shading = serializedObject.FindProperty("shading");
            gridSize = serializedObject.FindProperty("gridSize");
            chunkSize = serializedObject.FindProperty("chunkSize");
            vertexDistance = serializedObject.FindProperty("vertexDistance");
            pointsPerAxis = serializedObject.FindProperty("pointsPerAxis");
            bounded = serializedObject.FindProperty("bounded");
            densityGenerator = serializedObject.FindProperty("densityGenerator");
           // isoLevel = serializedObject.FindProperty("isoLevel");
            chunkPrefab = serializedObject.FindProperty("chunkPrefab");
            autoUpdateEditor = serializedObject.FindProperty("autoUpdateEditor");
            autoUpdateGame = serializedObject.FindProperty("autoUpdateGame");
            brush = serializedObject.FindProperty("brush");
            dataSaves = serializedObject.FindProperty("dataSave");

            showMeshSettings = false;
            showGrid = false;
            showChunks = false;
            showDensity = false;
            showUpdateSettings = false;
            showBrush = false;

            if(mesh.densityGenerator != null)
                CreateCachedEditor(densityGenerator.objectReferenceValue, null, ref densityEditor);
            if(mesh.brush != null)
                CreateCachedEditor(brush.objectReferenceValue, null, ref brushEditor);

            sculpt = false;
        }

        public override void OnInspectorGUI()
        {
            bool changedChunkSize = false;
            bool changedVertexDistance = false;
            bool changedPointsPerAxis = false;

            showMeshSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMeshSettings, "Mesh Settings");
            if (showMeshSettings)
            {
                EditorGUILayout.PropertyField(shading);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showGrid = EditorGUILayout.BeginFoldoutHeaderGroup(showGrid, "Grid");
            if (showGrid)
            {
                EditorGUILayout.PropertyField(gridSize);
                EditorGUILayout.PropertyField(bounded);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showChunks = EditorGUILayout.BeginFoldoutHeaderGroup(showChunks, "Chunks");
            if (showChunks)
            {
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
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showDensity = EditorGUILayout.BeginFoldoutHeaderGroup(showDensity, "Density");
            if (showDensity)
            {
                // EditorGUILayout.PropertyField(isoLevel);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(densityGenerator);
                if (EditorGUI.EndChangeCheck() && mesh.densityGenerator != null)
                    CreateCachedEditor(densityGenerator.objectReferenceValue, null, ref densityEditor);
                if (mesh.densityGenerator != null)
                {
                    EditorGUI.indentLevel++;
                    densityEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showUpdateSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showUpdateSettings, "Update Settings");
            if (showUpdateSettings)
            {
                EditorGUILayout.PropertyField(autoUpdateEditor);
                EditorGUILayout.PropertyField(autoUpdateGame);
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(dataSaves);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            showBrush = EditorGUILayout.BeginFoldoutHeaderGroup(showBrush, "Brush");
            if (showBrush)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(brush);
                if (EditorGUI.EndChangeCheck() && mesh.brush != null) {
                    CreateCachedEditor(brush.objectReferenceValue, null, ref brushEditor);
                    mesh.brush.MCMesh = mesh;
                }
                if (mesh.brush != null)
                {
                    EditorGUI.indentLevel++;
                    brushEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(5f);
            if (!sculpt)
            {
                if (mesh.exist)
                {
                    if (GUILayout.Button("Load From File", GUILayout.Height(25f)))
                    {
                        mesh.LoadFromFile();
                        mesh.UpdateChunks();
                    }
                    if (GUILayout.Button("Save To File", GUILayout.Height(25f)))
                        mesh.SaveToFile();
                    if (GUILayout.Button("Sculpt", GUILayout.Height(25f)))
                        StartSculpting();
                    if (GUILayout.Button("Regenerate", GUILayout.Height(25f)))
                        mesh.GenerateEditor();
                    if (GUILayout.Button("Reset", GUILayout.Height(25f)))
                        mesh.ResetChunks();
                }
                else
                {
                    if (GUILayout.Button("Generate", GUILayout.Height(25f)))
                        mesh.GenerateEditor();
                }
            }
            else
            {
                if (GUILayout.Button("Save", GUILayout.Height(25f)))
                    SaveSculpt();
                if (GUILayout.Button("Discard", GUILayout.Height(25f)))
                    DiscardSculpt();
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

        private void OnSceneGUI()
        {
            if (sculpt)
            {

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Event e = Event.current;
                if (e.control)
                {
                    if (e.type == EventType.ScrollWheel)
                    {
                        mesh.brush.ChangeRadious(e.delta.y);
                    }
                }
                if((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
                {
                    if(e.button == 0)
                    {
                        Vector3 mousePos = e.mousePosition;
                        float p = EditorGUIUtility.pixelsPerPoint;
                        mousePos.y = Camera.current.pixelHeight - mousePos.y * p;
                        mousePos.x *= p;
                        Ray ray = Camera.current.ScreenPointToRay(mousePos);

                        if(Physics.Raycast(ray, out RaycastHit hit))
                        {
                            mesh.brush.Paint(hit.point);
                        }
                    }

                }

                if (e.type == EventType.MouseUp && e.button == 0) // disble selecting objects
                    e.Use();
            }            
        }

        void StartSculpting()
        {
            if (mesh.brush.MCMesh == null)
                mesh.brush.MCMesh = mesh;
            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            sculpt = true;
            Undo.RecordObject(mesh.gameObject, "Start Sculpting");
        }
        void SaveSculpt()
        {
            sculpt = false;
            mesh.SaveToFile();
        }
        void DiscardSculpt()
        {
            sculpt = false;
            mesh.LoadFromFile();
            mesh.UpdateChunks();
        }
    }
}
