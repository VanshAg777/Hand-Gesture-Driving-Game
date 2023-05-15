#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolvoCars.Data
{

    /// <summary>The force applied to the steering rack. Positive values are steering left since the rack is behind the centers of the wheels. In the default chassis dynamics script, the VirtualDriverSteering data needs to be set to true in order for this to be read. Otherwise UserSteeringInput will be used.</summary>
    public class SteeringRackForce : Data<float> { }

#if UNITY_EDITOR
    [CustomEditor(typeof(SteeringRackForce))]
    public class StRackFEditor : DataEditor<float>
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The force applied to the steering rack. Positive values are steering left since the rack is behind the centers of the wheels. In the default chassis dynamics script, the VirtualDriverSteering data needs to be set to true in order for this to be read. Otherwise UserSteeringInput will be used.", MessageType.None, true);
            base.OnInspectorGUI();
        }

    }
#endif

}