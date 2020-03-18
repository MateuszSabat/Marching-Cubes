using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    public class CurveEditorWindow : EditorWindow
    {
        public SubstanceGenerator generator;

        float curvePanelSize;
        Material mat;

        int active = 1;

        float clickR = 0.01f;
        struct ActivePoint
        {
            public bool exist;
            public int i;
            public int which;
            public Vector2 relativePosition;
        };
        ActivePoint activePoint;

        bool dragingPoint;


        private void OnEnable()
        {
            mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        }


        private void OnGUI()
        {
            DrawCurvePanel();

            Event e = Event.current;
            if(e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    Vector2 mousePos = e.mousePosition;
                    mousePos.x -= 20f;
                    mousePos.y -= 20f;
                    mousePos /= curvePanelSize;
                    mousePos.y = 1 - mousePos.y;

                    dragingPoint = IsCurvePoint(mousePos);

                    e.Use();
                }
            }
            if(e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    dragingPoint = false;
                }
            }
            if(e.type == EventType.MouseDrag && dragingPoint)
            {
                Vector2 mousePos = e.mousePosition;
                mousePos.x -= 20f;
                mousePos.y -= 20f;
                mousePos /= curvePanelSize;
                mousePos.y = 1 - mousePos.y;

                if (activePoint.which == 0)
                    if (active == 0)
                        generator.heightCurve.points[activePoint.i].pos = mousePos + activePoint.relativePosition;
                    else
                        generator.curves[active].points[activePoint.i].pos = mousePos + activePoint.relativePosition;
                else
                    if (active == 0)
                        generator.heightCurve.points[activePoint.i].dir = activePoint.which * (mousePos + activePoint.relativePosition - generator.heightCurve.points[activePoint.i].pos);
                    else
                        generator.curves[active].points[activePoint.i].dir = activePoint.which * (mousePos + activePoint.relativePosition - generator.curves[active].points[activePoint.i].pos);

                if (active == 0)
                    generator.heightCurve.Recalculate();
                else
                    generator.curves[active].Recalculate();
            }
        }


        void Vertex3(float x, float y, float z)
        {
            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);
            z = Mathf.Clamp01(z);
            GL.Vertex3(x * curvePanelSize, (1 - y) * curvePanelSize, z * curvePanelSize);
        }
        void Vertex3(Vector2 pos)
        {
            Vertex3(pos.x, pos.y, 0);
        }

        void DrawCircle(Vector2 center, float r)
        {
            float alpha = 0;
            float dAlpha = 0.4f;
            Vertex3(center + new Vector2(1, 0) * r);
            while(alpha < 6.28318530717f)
            {
                alpha += dAlpha;
                Vertex3(center + new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha)) * r);
                Vertex3(center + new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha)) * r);
            }
            alpha += dAlpha;
            Vertex3(center + new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha)) * r);
        }
        void DrawSquere(Vector2 center, float size)
        {
            Vertex3(center + new Vector2(size, size));
            Vertex3(center + new Vector2(size, -size));
            Vertex3(center + new Vector2(size, -size));
            Vertex3(center + new Vector2(-size, -size));
            Vertex3(center + new Vector2(-size, -size));
            Vertex3(center + new Vector2(-size, size));
            Vertex3(center + new Vector2(-size, size));
            Vertex3(center + new Vector2(size, size));
        }

        void DrawPoint(SubstanceGenerator.Curve.Point p)
        {
            Vertex3(p.pos + p.dir);
            Vertex3(p.pos - p.dir);
            DrawSquere(p.pos, 0.003f);
            DrawCircle(p.pos + p.dir, 0.003f);
            DrawCircle(p.pos - p.dir, 0.003f);
        }

        void DrawCurve(int i)
        {
            SubstanceGenerator.Curve c = generator.curves[i];
            if (i == active)
                GL.Color(c.color);
            else
                GL.Color(c.color * 0.5f);

            Vertex3(c.p[0]);
            for(int j=1; j<c.p.Count - 1; j++)
            {
                Vertex3(c.p[j]);
                Vertex3(c.p[j]);
            }
            Vertex3(c.p[c.p.Count - 1]);
            if (i == active)
            {
                for (int j = 0; j < c.points.Count; j++)
                    DrawPoint(c.points[j]);
            }
        }

        void DrawHeightCurve()
        {
            SubstanceGenerator.Curve c = generator.heightCurve;
            if (active == 0)
                GL.Color(c.color);
            else
                GL.Color(c.color * 0.5f);

            Vertex3(c.p[0]);
            for (int j = 1; j < c.p.Count - 1; j++)
            {
                Vertex3(c.p[j]);
                Vertex3(c.p[j]);
            }
            Vertex3(c.p[c.p.Count - 1]);
            if (active == 0)
            {
                for (int j = 0; j < c.points.Count; j++)
                    DrawPoint(c.points[j]);
            }
        }

        void DrawCurvePanel()
        {
            bool activeVisible = true;
            curvePanelSize = 0.8f * Mathf.Min(Screen.width, Screen.height - 210f) - 40f;
            Rect rect = GUILayoutUtility.GetRect(curvePanelSize, curvePanelSize);
            rect.x = 20f;
            rect.y = 20f;
            if(Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                GL.PushMatrix();
                GL.Clear(true, false, Color.black);
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

                if (active == 0)
                {
                    activeVisible = generator.heightCurveVisible;

                    for (int i = 1; i < SubstanceTable.substances.Count; i++)
                        if (generator.state[i] == SubstanceGenerator.SubstanceState.Visible)
                            DrawCurve(i);

                    DrawHeightCurve();
                }
                else {
                    DrawHeightCurve();
                    for (int i = 1; i < SubstanceTable.substances.Count; i++)
                        if (generator.state[i] == SubstanceGenerator.SubstanceState.Visible && i != active)
                            DrawCurve(i);
                    if (generator.state[active] == SubstanceGenerator.SubstanceState.Visible)
                        DrawCurve(active);
                    else
                        activeVisible = false;
                }

                GL.End();

                GL.PopMatrix();

                GUI.EndClip();
            }
            EditorGUI.indentLevel++;
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active curve: ", GUILayout.Width(100f));
            active = EditorGUILayout.IntField(active, GUILayout.Width(40f));
            if (GUILayout.Button("<", GUILayout.Width(20f)))
                active--;
            if (GUILayout.Button(">", GUILayout.Width(20f)))
                active++;
            active = Mathf.Clamp(active, 0, SubstanceTable.substances.Count - 1);
            EditorGUILayout.LabelField( active == 0 ? "Height Curve" : SubstanceTable.substances[active].name);
            GUILayout.EndHorizontal();
            if (!activeVisible)
                EditorGUILayout.LabelField("Active curve is not visible");
            GUILayout.Space(5f);
            if (activePoint.exist)
            {
                activePoint.i = EditorGUILayout.IntField("Current Point", activePoint.i);
                SubstanceGenerator.Curve c = active == 0 ? generator.heightCurve : generator.curves[active];
                activePoint.i = Mathf.Clamp(activePoint.i, 0, c.points.Count - 1);
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                c.points[activePoint.i].pos = EditorGUILayout.Vector2Field("Position", c.points[activePoint.i].pos);
                c.points[activePoint.i].dir = EditorGUILayout.Vector2Field("Tangent Direction", c.points[activePoint.i].dir);
                if (EditorGUI.EndChangeCheck())
                    c.Recalculate();
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("No active point");
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(10f);
            if (GUILayout.Button("Save"))
                Save();
        }

        bool IsCurvePoint(Vector2 pos)
        {
            activePoint.exist = false;

            if (active == 0)
            {
                for (int i = 0; i < generator.heightCurve.points.Count; i++)
                {
                    if (Vector3.SqrMagnitude(generator.heightCurve.points[i].pos - pos) <= clickR)
                    {
                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = 0;
                        activePoint.relativePosition = generator.heightCurve.points[i].pos - pos;
                        return true;
                    }
                    if (Vector3.SqrMagnitude(generator.heightCurve.points[i].pos + generator.heightCurve.points[i].dir - pos) <= clickR)
                    {
                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = 1;
                        activePoint.relativePosition = generator.heightCurve.points[i].pos + generator.heightCurve.points[i].dir - pos;
                        return true;
                    }
                    if (Vector3.SqrMagnitude(generator.heightCurve.points[i].pos - generator.heightCurve.points[i].dir - pos) <= clickR)
                    {

                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = -1;
                        activePoint.relativePosition = generator.heightCurve.points[i].pos - generator.heightCurve.points[i].dir - pos;
                        return true;
                    }
                }
            }
            else
            {

                for (int i = 0; i < generator.curves[active].points.Count; i++)
                {
                    if (Vector3.SqrMagnitude(generator.curves[active].points[i].pos - pos) <= clickR)
                    {
                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = 0;
                        activePoint.relativePosition = generator.curves[active].points[i].pos - pos;
                        return true;
                    }
                    if (Vector3.SqrMagnitude(generator.curves[active].points[i].pos + generator.curves[active].points[i].dir - pos) <= clickR)
                    {
                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = 1;
                        activePoint.relativePosition = generator.curves[active].points[i].pos + generator.curves[active].points[i].dir - pos;
                        return true;
                    }
                    if (Vector3.SqrMagnitude(generator.curves[active].points[i].pos - generator.curves[active].points[i].dir - pos) <= clickR)
                    {

                        activePoint.exist = true;
                        activePoint.i = i;
                        activePoint.which = -1;
                        activePoint.relativePosition = generator.curves[active].points[i].pos - generator.curves[active].points[i].dir - pos;
                        return true;
                    }
                }
            }

            return false;
        }

        void Save()
        {
            EditorUtility.SetDirty(generator);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
