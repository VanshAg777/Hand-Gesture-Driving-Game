#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes whether the left front door is open.</summary>
    public class DoorIsOpenR1L : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DoorIsOpenR1L))]
    public class DoorIsOpenR1LEditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes whether the left front door is open.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}