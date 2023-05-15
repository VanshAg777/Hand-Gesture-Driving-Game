#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes whether the right rear door is open.</summary>
    public class DoorIsOpenR2R : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DoorIsOpenR2R))]
    public class DoorIsOpenR2REditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes whether the right rear door is open.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}