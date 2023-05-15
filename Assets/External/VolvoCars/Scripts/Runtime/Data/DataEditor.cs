using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace VolvoCars.Data
{

    public abstract class DataEditor<T> : Editor
    {
        private SerializedProperty defaultValueProp;
        private SerializedProperty valueProp;
        private SerializedProperty tempProp;
        private GenericData dataItem;
        private bool publishOnValueChange = false;

        public void OnEnable()
        {
            defaultValueProp = serializedObject.FindProperty("defaultValue");
            valueProp = serializedObject.FindProperty("_value");
            dataItem = ((GenericData)target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("Default Value is the initial value and is remembered between sessions. \nValue is used during runtime.", MessageType.None);

            if (Application.isPlaying)
                GUI.enabled = false;

            EditorGUILayout.PropertyField(defaultValueProp, true);
            GUI.enabled = !GUI.enabled;

            EditorGUILayout.PropertyField(valueProp, true);
            GUI.enabled = true;

            if (Application.isPlaying) {
                GUILayout.Space(20);
                publishOnValueChange = EditorGUILayout.Toggle("Publish changed values directly", publishOnValueChange);
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();

                if (Application.isPlaying && publishOnValueChange) {
                    dataItem.TriggerUpdate();
                } else if (!Application.isPlaying) {
                    dataItem.SetDefaultValueAsValue();
                }

            }

            if (Application.isPlaying) {
                if (!publishOnValueChange)
                    if (GUILayout.Button("Publish values", GUILayout.Height(30)))
                        dataItem.TriggerUpdate();

                GUILayout.Space(15);
                if (GUILayout.Button("Store current value as default", GUILayout.Height(30))) {
                    dataItem.SetCurrentValueAsDefault();
                }
            }

        }

        protected virtual void PersistValue(string path, T value)
        {
            Debug.LogError("PersistValue called in the DataEditor base class.");
        }

        protected virtual void LoadPersistedValue(string path, SerializedProperty valueProp)
        {
            Debug.LogError("LoadPersistedValue called in the DataEditor base class.");
        }


    }

}

#endif