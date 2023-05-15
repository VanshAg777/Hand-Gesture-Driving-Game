#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes whether the left rear door is open.</summary>
    public class DoorIsOpenR2L : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DoorIsOpenR2L))]
    public class DoorIsOpenR2LEditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes whether the left rear door is open.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}