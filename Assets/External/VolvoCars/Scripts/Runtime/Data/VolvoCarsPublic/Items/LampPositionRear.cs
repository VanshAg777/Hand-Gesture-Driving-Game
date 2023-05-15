#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{

    /// <summary>Controls the characteristics of a light source, see which one in the topic.</summary>
    public class LampPositionRear : Data<Value.Public.LampGeneral> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(LampPositionRear))]
    public class LampPositionRearEditor : DataEditor<Value.Public.LampGeneral>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Controls the characteristics of a light source, see which one in the topic.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}