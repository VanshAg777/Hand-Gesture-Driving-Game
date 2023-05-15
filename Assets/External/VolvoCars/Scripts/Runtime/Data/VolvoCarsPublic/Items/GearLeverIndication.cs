#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Describes which gear should be indicated to the driver.</summary>
    public class GearLeverIndication : Data<int> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(GearLeverIndication))]
    public class GearLvrIndcnEditor : DataEditor<int>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Describes which gear should be indicated to the driver. PRND.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}