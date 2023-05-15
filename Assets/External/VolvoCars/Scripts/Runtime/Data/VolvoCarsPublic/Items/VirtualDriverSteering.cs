#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Is the car driving itself (to any degree)?</summary>
    public class VirtualDriverSteering : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(VirtualDriverSteering))]
    public class VirtualDriverSteeringEditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Is the car driving itself (to any degree)?", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}