#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>The car's signed longitudinal acceleration.</summary>
    public class Acceleration : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(Acceleration))]
    public class AccelerationEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The car's signed longitudinal acceleration.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}