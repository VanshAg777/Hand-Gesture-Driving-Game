using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    /// <summary>Whether the main camera is inside the car or not. True if inside the car.</summary>
    public class CameraIsInsideCar : Data<bool> { }


#if UNITY_EDITOR
    [CustomEditor(typeof(CameraIsInsideCar))]
    public class CameraIsInsideCarEditor : DataEditor<bool>
    {
        public override void OnInspectorGUI()
        {
			EditorGUILayout.HelpBox("Whether the main camera is inside the car or not. True if inside the car.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}