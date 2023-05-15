using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Rotator))]
[CanEditMultipleObjects]
public class RotatorEditor : Editor
{

    SerializedProperty angle;
    SerializedProperty min;
    SerializedProperty max;

    void OnEnable()
    {
        angle = serializedObject.FindProperty("angle");
        min = serializedObject.FindProperty("min");
        max = serializedObject.FindProperty("max");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        var value = EditorGUILayout.Slider(angle.floatValue, min.floatValue, max.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            ((Rotator)target).SetAngle(value);
            angle.floatValue = value;
        }
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Reset Rotation"))
        {
            ((Rotator)target).Reset();
            angle.floatValue = 0f;
        }

        DrawDefaultInspector();
    }
}