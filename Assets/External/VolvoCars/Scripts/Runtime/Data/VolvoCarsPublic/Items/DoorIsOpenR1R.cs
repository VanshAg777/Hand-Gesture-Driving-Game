#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes whether the right front door is open.</summary>
    public class DoorIsOpenR1R : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(DoorIsOpenR1R))]
    public class DoorIsOpenR1REditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes whether the right front door is open.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}