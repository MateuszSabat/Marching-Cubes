using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MarchingCubes {
    [CustomEditor(typeof(Brush))]
    public class BrushEditor : Editor
    {
        Brush brush;
        RenderTexture brushImage;
        public int imageSize = 64;

        SerializedProperty imageCompute;
        SerializedProperty paintCompute;

        private void OnEnable()
        {
            // Add imageCompute
            brush = (Brush)target;
            imageCompute = serializedObject.FindProperty("imageCompute");
            paintCompute = serializedObject.FindProperty("paintCompute");
            brushImage = new RenderTexture(imageSize, imageSize, 1);
            brushImage.enableRandomWrite = true;
            brushImage.Create();

            if (brush.imageCompute != null)
            {
                brush.imageCompute.SetTexture(0, "Image", brushImage);
                brush.imageCompute.SetInt("size", imageSize);
                brush.imageCompute.SetFloat("smooth", brush.smooth);
                brush.imageCompute.SetInt("smoothType", (int)brush.smoothType);

                int threadGroup = Mathf.RoundToInt(imageSize / 8f);
                brush.imageCompute.Dispatch(0, threadGroup, threadGroup, 1);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Compute Shader", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(imageCompute);
            EditorGUILayout.PropertyField(paintCompute);
            EditorGUILayout.Space(5f);
            brush.radious = EditorGUILayout.FloatField("Radious", brush.radious);
            
            if (SubstanceTable.substances == null)
                SubstanceTable.Init();
            EditorGUILayout.LabelField("Density", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            brush.dPaintStyle = (Brush.DensnityPaintStyle)EditorGUILayout.EnumPopup("Paint Style", brush.dPaintStyle);
            brush.set = EditorGUILayout.Slider("Strength", brush.set, brush.dPaintStyle == Brush.DensnityPaintStyle.Set ? 0.0f : -1.0f, 1.0f);
            brush.smoothType = (Brush.SmoothType)EditorGUILayout.EnumPopup("Smooth Type", brush.smoothType);
            brush.smooth = EditorGUILayout.FloatField("Smooth", brush.smooth);
            RenderBrushImage(EditorGUI.EndChangeCheck());

            EditorGUILayout.LabelField("Substance", EditorStyles.boldLabel);
            brush.changeSubstance = EditorGUILayout.Toggle("change", brush.changeSubstance);
            brush.substance = EditorGUILayout.IntField("substance", brush.substance);
            if (brush.substance < 1)
                brush.substance = 1;
            if (brush.substance >= SubstanceTable.substances.Count)
                brush.substance = SubstanceTable.substances.Count - 1;
            SubstanceEditorWindow.DrawSubstance(brush.substance);

            serializedObject.ApplyModifiedProperties();
        }

        void RenderBrushImage(bool changeImage)
        {
            if (changeImage) {
                brush.imageCompute.SetTexture(0, "Image", brushImage);
                brush.imageCompute.SetInt("size", imageSize);
                brush.imageCompute.SetFloat("smooth", brush.smooth);
                brush.imageCompute.SetInt("smoothType", (int)brush.smoothType);

                int threadGroup = Mathf.RoundToInt(imageSize / 8f);
                brush.imageCompute.Dispatch(0, threadGroup, threadGroup, 1);
            }

            EditorGUILayout.LabelField(new GUIContent(brushImage), GUILayout.Width(64f), GUILayout.Height(64f));
        }

        public static void InspectBrush(Brush b)
        {

        }
    }
}
