using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MarchingCubes
{
    public class SubstanceEditorWindow : EditorWindow
    {
        public const float fieldHeight = 20f;

        private enum SubstanceBoxState { None, New, Inspect}
        private SubstanceBoxState boxState;
        private Substance modifiedSubstance;
        private int inspectedSubstance = 0;
        private bool showSubstances;
        Vector2 scrollPos;


        [MenuItem("Window/Marching Cubes/Substances")]
        public static void Initialize()
        {
            GetWindow(typeof(SubstanceEditorWindow), false, "Substances");
            SubstanceTable.Init();
        }

        private void OnGUI()
        {
            float cHeight = 85f + fieldHeight * 3f;
            EditorGUILayout.LabelField("Substance Inspector", EditorStyles.boldLabel, GUILayout.Height(fieldHeight));
            EditorGUILayout.Space(5f);
            if (boxState == SubstanceBoxState.New)
            {
                DrawSubstanceEditor();
                if (GUILayout.Button("Add Substance", GUILayout.Height(fieldHeight)))
                {
                    SubstanceTable.Add(modifiedSubstance);
                    boxState = SubstanceBoxState.None;
                }
                if (GUILayout.Button("Discard", GUILayout.Height(fieldHeight)))
                {
                    boxState = SubstanceBoxState.None;
                    modifiedSubstance = new Substance();
                }
                cHeight += 4f * fieldHeight;
            }
            else if(boxState == SubstanceBoxState.Inspect)
            {
                DrawSubstanceEditor();
                if (GUILayout.Button("Save", GUILayout.Height(fieldHeight)))
                {
                    SubstanceTable.substances[inspectedSubstance] = modifiedSubstance;
                    boxState = SubstanceBoxState.None;
                }
                if(GUILayout.Button("Remove", GUILayout.Height(fieldHeight)))
                {
                    SubstanceTable.Remove(modifiedSubstance);
                    boxState = SubstanceBoxState.None;
                }
                if (GUILayout.Button("Discard", GUILayout.Height(fieldHeight)))
                {
                    modifiedSubstance = new Substance();
                    boxState = SubstanceBoxState.None;
                }
                cHeight += 5f * fieldHeight;
            }
            else
            {
                if(GUILayout.Button("Create Substance", GUILayout.Height(fieldHeight)))
                {
                    boxState = SubstanceBoxState.New;
                    modifiedSubstance = new Substance();
                }
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Inspect Substance Of Index", GUILayout.Height(fieldHeight)))
                {
                    if (inspectedSubstance > 0 && inspectedSubstance < SubstanceTable.substances.Count)
                    {
                        boxState = SubstanceBoxState.Inspect;
                        modifiedSubstance = new Substance(SubstanceTable.substances[inspectedSubstance]);
                    }
                    else
                    {
                        Debug.LogWarning("Inspected Substance Index must be greater than zero and less than substances count");
                    }
                }
                inspectedSubstance = EditorGUILayout.IntField(inspectedSubstance, GUILayout.Height(fieldHeight));
                GUILayout.EndHorizontal();
                if(inspectedSubstance > 0 && inspectedSubstance < SubstanceTable.substances.Count)
                    EditorGUILayout.LabelField(SubstanceTable.substances[inspectedSubstance].name, GUILayout.Height(fieldHeight));
                else
                    EditorGUILayout.LabelField("", GUILayout.Height(fieldHeight));
                cHeight += 3f * fieldHeight;

            }
            EditorGUILayout.LabelField("Substance List", EditorStyles.boldLabel, GUILayout.Height(fieldHeight));
            if (showSubstances)
            {
                if (GUILayout.Button("Hide substance list", GUILayout.Height(fieldHeight)))
                    showSubstances = false;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - cHeight));
                for (int i = 0; i < SubstanceTable.substances.Count; i++)
                    DrawSubstance(i);
                EditorGUILayout.EndScrollView();
            }
            else
            {
                if(GUILayout.Button("Show substance list", GUILayout.Height(fieldHeight)))
                    showSubstances = true;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update", GUILayout.Height(25f)))
                SubstanceTable.Init();
        //    if (GUILayout.Button("Reset", GUILayout.Height(25f)))
        //        SubstanceTable.Reset();
            if (GUILayout.Button("Save To File", GUILayout.Height(25f)))
                SubstanceTable.Save();
        }

        private void DrawSubstanceEditor()
        {
            EditorGUI.indentLevel++;

            modifiedSubstance.name = EditorGUILayout.TextField("name", modifiedSubstance.name, GUILayout.Height(fieldHeight));
            Color c = EditorGUILayout.ColorField("color", modifiedSubstance.color, GUILayout.Height(fieldHeight));
            modifiedSubstance.r = c.r;
            modifiedSubstance.g = c.g;
            modifiedSubstance.b = c.b;

            EditorGUI.indentLevel--;
        }

        public static void DrawSubstance(int i)
        {
            EditorGUILayout.LabelField(i.ToString()+ ":");

            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("name", SubstanceTable.substances[i].name);
            EditorGUILayout.ColorField("color", SubstanceTable.substances[i].color);

            EditorGUI.indentLevel--;
        }

        public void UpdateSubstances()
        {
            SubstanceTable.RemoveDuplicate();
        }
    }
}
