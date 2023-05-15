#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{
    
    /// <summary>How much the steering wheel has been turned. This is only used for animating the steering wheel. Use UserSteeringInput or SteeringRackForce to make the car turn.</summary>
    public class SteeringWheelAngle : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteeringWheelAngle))]
    public class SteeringWheelAngleEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("How much the steering wheel has been turned. This is only used for animating the steering wheel. Use UserSteeringInput or SteeringRackForce to make the car turn.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}