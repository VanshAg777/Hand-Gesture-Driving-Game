using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InfoText))]
public class InfoTextPropertyDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        string text = property.FindPropertyRelative("text").stringValue;
        int msgTypeIdx = property.FindPropertyRelative("messageType").enumValueIndex;
        bool show = property.FindPropertyRelative("show").boolValue;

        if (show) {
            MessageType msgType;
            switch (msgTypeIdx) {
                case 0:
                    msgType = MessageType.None;
                    break;
                case 1:
                    msgType = MessageType.Info;
                    break;
                case 2:
                    msgType = MessageType.Warning;
                    break;
                default:
                    msgType = MessageType.Error;
                    break;
            }

            EditorGUI.HelpBox(position, text, msgType);
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.FindPropertyRelative("show").boolValue) {
            GUIStyle helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
            if (helpBoxStyle == null) {
                return base.GetPropertyHeight(property, label);
            }

            float height = Mathf.Max(25f, helpBoxStyle.CalcHeight(new GUIContent(property.FindPropertyRelative("text").stringValue), EditorGUIUtility.currentViewWidth) + 10);
            return height;

        } else {
            return 0;
        }
    }
}
