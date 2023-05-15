#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{

    /// <summary>Controls the characteristics of a light source, see which one in the topic.</summary>
    public class LampBrake : Data<Value.Public.LampGeneral> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(LampBrake))]
    public class LampBrakeEditor : DataEditor<Value.Public.LampGeneral>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls the characteristics of a light source, see which one in the topic.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}