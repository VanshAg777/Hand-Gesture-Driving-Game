#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes how much the driver is steering, as a ratio of maximum steering [-1 1].</summary>
    public class UserSteeringInput : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(UserSteeringInput))]
    public class UserSteeringInputEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes how much the driver is steering, as a ratio of maximum steering [-1 1].", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}