using com.unity.testtrack.terrainsystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProceduralRoadTool
{

    [CustomEditor(typeof(SplineTool))]
    public class SplineToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SplineTool targetObj = ((SplineTool)target);

            if (GUILayout.Button("Re-Generate"))
            {
                EditorUtility.DisplayProgressBar("Generating the road", "Re-generating the road", 0);
                targetObj.Generate();
                EditorUtility.ClearProgressBar();
            }
            
            if (GUILayout.Button("Reset"))
            {
                targetObj.Reset();
            }

            base.OnInspectorGUI();
        }
        
    }

}
