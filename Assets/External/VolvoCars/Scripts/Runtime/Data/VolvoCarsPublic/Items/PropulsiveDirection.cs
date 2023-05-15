#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>The longitudinal direction in which positive torque is acting. Forward: 1. None: 0. Reverse: -1.</summary>
    public class PropulsiveDirection : Data<int> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(PropulsiveDirection))]
    public class PropulsiveDirectionEditor : DataEditor<int>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The longitudinal direction in which positive wheel torque is acting. " +
                "\nForward: 1 " +
                "\nNone: 0 " +
                "\nReverse: -1 ", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}