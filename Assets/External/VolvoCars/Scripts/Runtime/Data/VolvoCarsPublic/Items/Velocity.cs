#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>The car's signed longitudinal speed.</summary>
    public class Velocity : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(Velocity))]
    public class VelocityEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The car's signed longitudinal speed.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}