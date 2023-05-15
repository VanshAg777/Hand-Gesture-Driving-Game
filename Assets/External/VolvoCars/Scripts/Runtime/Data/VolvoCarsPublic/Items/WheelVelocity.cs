#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    /// <summary>Rotational velocity of for each wheel in rpm</summary>
    public class WheelVelocity : Data<Value.Public.WheelTorque> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(WheelVelocity))]
    public class WheelVelocityEditor : DataEditor<Value.Public.WheelTorque>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Rotational velocity of for each wheel in rpm", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}