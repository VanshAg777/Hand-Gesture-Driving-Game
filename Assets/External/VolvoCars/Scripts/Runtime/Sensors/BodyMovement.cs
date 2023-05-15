using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.ActorManagement.Car.Sensors
{
    public class BodyMovement : MonoBehaviour
    {
        [Space(5)]
        [SerializeField]
        private InfoText info = new InfoText("This sensor measures the movement of the car body, " +
            "such as its velocity and acceleration. The output is stored in the data items below.");

        [Header("References")]
        public Rigidbody rigidBody;
        public RigidBodyExtension rigidBodyExtension;

        [Header("Data")]
        public Data.Velocity velocityData;
        public Data.Acceleration accelerationData;

        [Header("Runtime Info")]
        [SerializeField]
        [ReadOnly]
        private float velocityKPH;

        [SerializeField]
        [ReadOnly]
        private float acceleration;

        [SerializeField]
        [ReadOnly]
        private float last0To100Time;

        [SerializeField]
        [ReadOnly]
        private float last100To0Distance;

        private float velocity, previousVelocity, smoothness, timerStart;
#if UNITY_EDITOR
        private bool zeroTo100TimerActive = false;
        private bool hundredToZeroActive = false;
#endif

        private void FixedUpdate()
        {
            // Velocity
            smoothness = 0.5f;
            velocity = (1f - smoothness) * Mathf.Round(GetLongitudinalVelocity() * 1000f) / 1000f + smoothness * velocity;
            velocityKPH = Mathf.Round(velocity * 3.6f * 10f) / 10f;

            // Acceleration
            acceleration = rigidBodyExtension.acceleration; 

            // Update data
            velocityData.Value = velocity;
            accelerationData.Value = acceleration;

#if UNITY_EDITOR
            if (Mathf.Abs(previousVelocity) < 0.001 && velocity > previousVelocity) {
                timerStart = Time.time;
                zeroTo100TimerActive = true;
            }
            if (velocity >= 100f / 3.6f && (previousVelocity < 100f / 3.6f) && zeroTo100TimerActive) {
                last0To100Time = Time.time - timerStart;
                zeroTo100TimerActive = false;
            }
            if (velocity <= 100f / 3.6f && previousVelocity > 100f / 3.6f) {
                hundredToZeroActive = true;
            }
            if (hundredToZeroActive) {
                last100To0Distance += velocity * Time.fixedDeltaTime;
            }
            if (velocity > 100f / 3.6f) {
                last100To0Distance = 0;
            }
            if (velocity < 0.001f) {
                last100To0Distance = Mathf.Round(last100To0Distance * 100) / 100f;
                hundredToZeroActive = false;
            }
#endif
            previousVelocity = velocity;

        }

        public float GetLongitudinalVelocity()
        {
            return rigidBody.transform.InverseTransformVector(rigidBody.velocity).z;
        }

    }
}