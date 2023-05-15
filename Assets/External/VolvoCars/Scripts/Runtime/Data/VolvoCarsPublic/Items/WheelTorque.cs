#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>Wheel torque for all four wheels applied in the direction defined by the PropulsiveDirection data item. Negative torque is decelerating.</summary>
    public class WheelTorque : Data<Value.Public.WheelTorque> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(WheelTorque))]
    public class WheelTorqueEditor : DataEditor<Value.Public.WheelTorque>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Wheel torque for all four wheels applied in the direction defined by the PropulsiveDirection data item. Negative torque is decelerating.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}