#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes whether the trunklid/tailgate is open.</summary>
    public class TrunklidIsOpen : Data<bool> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(TrunklidIsOpen))]
    public class TrunklidIsOpenEditor : DataEditor<bool>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes whether the trunklid/tailgate is open.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}