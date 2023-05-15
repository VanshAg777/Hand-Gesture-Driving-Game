#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    /// <summary>The car's accelator predal position between 0-1</summary>
    public class AcceleratorPedalPosition : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(AcceleratorPedalPosition))]
    public class AcceleratorPedalPositionEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The car's accelator predal position between 0-1", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}