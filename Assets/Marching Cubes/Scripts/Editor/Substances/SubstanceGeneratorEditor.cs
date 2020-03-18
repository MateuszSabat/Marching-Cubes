using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    [CustomEditor(typeof(SubstanceGenerator))]
    public class SubstanceGeneratorEditor : Editor
    {

        SubstanceGenerator generator;

        bool cList;

        Vector2 subPos;

        Material mat;

        float curvePanelSize;

        private void OnEnable()
        {
            cList = false;
            generator = (SubstanceGenerator)target;
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            mat = new Material(shader);
            generator.Init();
        }

        public override void OnInspectorGUI()
        {
            GUIStyle left = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            GUIStyle center = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUIStyle right = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            if (generator.state == null || generator.curves == null)
            {
                generator.Init();
            }

            if (cList)
            {
                if (GUILayout.Button("Hide Curve List"))
                    cList = false;
                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField("Height Curve", EditorStyles.boldLabel);
                DrawHeightCurveState();
                EditorGUILayout.LabelField("Substances", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("color", "color of curve on graph"), GUILayout.Width(40f));
                EditorGUILayout.LabelField("name", GUILayout.Width(Screen.width * 0.2f));
                EditorGUILayout.LabelField(new GUIContent("zero", "these substances are not generated"), left, GUILayout.Width(Screen.width * 0.15f));
                EditorGUILayout.LabelField(new GUIContent("shown", "these substances are generated and displayed in inspector"), center, GUILayout.Width(Screen.width * 0.15f));
                EditorGUILayout.LabelField(new GUIContent("hidden", "these substances are generated, but not displayed in insector"), right, GUILayout.Width(Screen.width * 0.15f));
                GUILayout.EndHorizontal();
                subPos = GUILayout.BeginScrollView(subPos, GUILayout.Height(80f));
                EditorGUILayout.Space(5f);
                for (int i = 1; i < generator.state.Length; i++)
                    DrawSubstanceState(i);
                GUILayout.EndScrollView();
            }
            else
            {
                if (GUILayout.Button("Show Curve List"))
                    cList = true;
            }
            GUILayout.Space(5f);
            DrawCurvePanel();
            GUILayout.Space(5f);
            if (GUILayout.Button("Reset"))
            {
                generator.ResetAll();
            }

        }

        void DrawHeightCurveState()
        {
            GUILayout.BeginHorizontal();
            generator.heightCurve.color = EditorGUILayout.ColorField(generator.heightCurve.color, GUILayout.Width(40f));
            EditorGUILayout.LabelField("Height Curve Visible");
            generator.heightCurveVisible = EditorGUILayout.Toggle(generator.heightCurveVisible);
            GUILayout.EndHorizontal();
        }

        void DrawSubstanceState(int i)
        {
            GUILayout.BeginHorizontal();
            generator.curves[i].color = EditorGUILayout.ColorField(generator.curves[i].color, GUILayout.Width(40f));
            EditorGUILayout.LabelField(SubstanceTable.substances[i].name, GUILayout.Width(Screen.width * 0.2f - 40f));
            Rect sliderPos = new Rect(Screen.width * 0.2f + 50f, i * 20f - 15f, Screen.width * 0.45f - 10f, 20f);
            generator.state[i] = (SubstanceGenerator.SubstanceState)Mathf.RoundToInt(GUI.HorizontalSlider(sliderPos, (int)generator.state[i], 0, 2));
            GUILayout.EndHorizontal();
        }

        void DrawCurvePanel()
        {
            curvePanelSize = 0.8f * Mathf.Min(Screen.width, Screen.height)-40f;
            Rect rect = GUILayoutUtility.GetRect(curvePanelSize, curvePanelSize);
            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                GL.PushMatrix();
                GL.Clear(false, false, Color.black);
                mat.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(Color.black);
                Vertex3(0, 0, 0);
                Vertex3(0, 1, 0);
                Vertex3(0, 1, 0);
                Vertex3(1, 1, 0);
                Vertex3(1, 1, 0);
                Vertex3(1, 0, 0);
                Vertex3(1, 0, 0);
                Vertex3(0, 0, 0);

                if (generator.heightCurveVisible)
                    DrawHeightCurve();

                for (int i = 1; i < SubstanceTable.substances.Count; i++)
                    if (generator.state[i] == SubstanceGenerator.SubstanceState.Visible)
                        DrawCurve(i);

                GL.End();

                GL.PopMatrix();

                GUI.EndClip();
            }
            if (GUILayout.Button("Open Editor", GUILayout.Width(curvePanelSize)))
                OpenCurvesEditor();
        }
        void Vertex3(float x, float y, float z)
        {
            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);
            z = Mathf.Clamp01(z);
            GL.Vertex3(x * curvePanelSize, (1 - y) * curvePanelSize, z * curvePanelSize);
        }

        void DrawHeightCurve()
        {
            SubstanceGenerator.Curve c = generator.heightCurve;
            GL.Color(c.color);
            Vertex3(0, c.y[0], 0);
            for (int x = 1; x < 100; x++)
            {
                Vertex3(x * 0.01f, c.y[x], 0);
                Vertex3(x * 0.01f, c.y[x], 0);
            }
            Vertex3(1, c.y[100], 0);
        }

        void DrawCurve(int i)
        {
            SubstanceGenerator.Curve c = generator.curves[i];
            GL.Color(c.color);
            Vertex3(0, c.y[0], 0);
            for(int x=1; x<100; x++)
            {
                Vertex3(x * 0.01f, c.y[x], 0);
                Vertex3(x * 0.01f, c.y[x], 0);
            }
            Vertex3(1, c.y[100], 0);
        }

        public void OpenCurvesEditor()
        {
            CurveEditorWindow window = (CurveEditorWindow)EditorWindow.GetWindow(typeof(CurveEditorWindow), false, "Curve Editor");

            window.generator = generator;        
        }
    }
}
