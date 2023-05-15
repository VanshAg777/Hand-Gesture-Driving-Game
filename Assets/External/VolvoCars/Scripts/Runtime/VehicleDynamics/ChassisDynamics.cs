using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VolvoCars.ActorManagement.Car.VehicleDynamics
{

    public class ChassisDynamics : MonoBehaviour
    {
        [SerializeField] private InfoText introduction = new InfoText("This script takes in the target values for steering and torque (acceleration/deceleration) from the user, " +
            "via the data items below (which could be updated by e.g. the demo car controller). " +
            "It then uses simplified vehicle dynamics and support systems before it updates values in the wheel colliders."); 

        #region parameters

        [Header("Settings")]
        [Space(2)]
        [Tooltip("The ratio between steering wheel angle and steering angle.")]
        public float steeringRatio = 15f;


        [Space(7)]
        public bool antiRoll = true;

        [Tooltip("As a fraction of the maximum spring force.")]
        [Range(0, 1)]
        public float antiRollStrength = 0.4f;
        
        [Space(7)]
        public bool antilockBraking = true;
        private Dictionary<WheelCollider, float> absFactor = new Dictionary<WheelCollider, float>();


        [Header("References")]
        public Rigidbody carRigidbody;
        public Wheel wheelFL;
        public Wheel wheelFR;
        public Wheel wheelRL;
        public Wheel wheelRR;

        [Header("Data")]
        public Data.SteeringWheelAngle steeringWheelAngleData;
        public Data.WheelTorque whlTqData;
        public Data.PropulsiveDirection propulsiveDirData;
        public Data.SteeringRackForce rackForceData;
        public Data.VirtualDriverSteering virtualDriverSteeringData;
        public Data.UserSteeringInput userSteeringInputData;
        public Data.Velocity velocity;
        public Data.WheelVelocity wheelVelocityData;


        // Private variables
        [Header("Runtime Info")]
        [SerializeField]
        [ReadOnly]
        private bool ABSActive = false;
        
        private float WHEEL_BASE;
        private float TRACK;

        private float vehicleSpeed;
        private float propulsiveDir;
        private Data.Value.Public.WheelTorque wheelTq = new Data.Value.Public.WheelTorque();
        private Data.Value.Public.WheelTorque wheelVelocity = new Data.Value.Public.WheelTorque();

        private bool virtualDriverSteering = false;
        private float userSteeringInput = 0f;
        private float steerRackForce;
        private const float MAX_RACK_POSITION = 0.093f; // [m]
        private float maxRackTravelSpeed = 0.1f;        // [m/s]
        private float steerAngle;
        private float targetRackPos, currentRackPos;

        private float antiRollStiffness;
        private float travelLeft, travelRight, antiRollForce;

        private WheelHit hit;
        private float originalWheelRadius;

        private Action<Data.Value.Public.WheelTorque> wheelTorqueAction;
        private Action<int> propulsiveDirAction;
        private Action<float> rackForceAction;
        private Action<bool> virtualDriverSteeringAction;
        private Action<float> userSteeringInputAction;
        private Action<float> velocityAction;
                
        [System.Serializable]
        public struct Wheel
        {
            public Transform wheelObject;
            public WheelCollider collider;

            [HideInInspector]
            public Quaternion originalRotation;
            [HideInInspector]
            public Vector3 originalPosition;
        }

        #endregion

        private void Awake()
        {
            WHEEL_BASE = (wheelFL.wheelObject.transform.position - wheelRL.wheelObject.transform.position).magnitude;
            TRACK = (wheelFL.wheelObject.transform.position - wheelFR.wheelObject.transform.position).magnitude;
        }

        void Start()
        {
            originalWheelRadius = wheelFL.collider.radius;
            
            absFactor[wheelFL.collider] = 1;
            absFactor[wheelFR.collider] = 1;
            absFactor[wheelRL.collider] = 1;
            absFactor[wheelRR.collider] = 1;

            wheelFR.collider.ConfigureVehicleSubsteps(1, 75, 50);

            // Subscriptions
            wheelTorqueAction = (v) => wheelTq = v; 
            whlTqData.SubscribeImmediate(wheelTorqueAction);

            propulsiveDirAction = (v) => propulsiveDir = v; 
            propulsiveDirData.SubscribeImmediate(propulsiveDirAction);

            rackForceAction = (v) => steerRackForce = v;
            rackForceData.SubscribeImmediate(rackForceAction);

            virtualDriverSteeringAction = (v) => virtualDriverSteering = v;
            virtualDriverSteeringData.SubscribeImmediate(virtualDriverSteeringAction);

            userSteeringInputAction = (v) => userSteeringInput = v;
            userSteeringInputData.SubscribeImmediate(userSteeringInputAction);

            velocityAction = (v) => vehicleSpeed = Mathf.Abs(v);
            velocity.SubscribeImmediate(velocityAction);

        }

        void FixedUpdate()
        {            
            SetWheelTorque(wheelFL.collider, wheelTq.fL, true);
            SetWheelTorque(wheelFR.collider, wheelTq.fR, true);
            SetWheelTorque(wheelRL.collider, wheelTq.rL, false);
            SetWheelTorque(wheelRR.collider, wheelTq.rR, false);
            
            ABSActive = (absFactor[wheelFL.collider] != 1 ||
                absFactor[wheelFR.collider] != 1 ||
                absFactor[wheelRL.collider] != 1 ||
                absFactor[wheelRR.collider] != 1);

            ApplyRackForce(steerRackForce);
            

            // Anti-roll
            if (antiRoll) {
                antiRollStiffness = wheelFL.collider.suspensionSpring.spring * antiRollStrength;

                // Front axle
                travelLeft = GetSuspensionTravel(wheelFL.collider);
                travelRight = GetSuspensionTravel(wheelFR.collider);
                antiRollForce = (travelLeft - travelRight) * antiRollStiffness;

                if (wheelFL.collider.GetGroundHit(out hit)) {
                    carRigidbody.AddForceAtPosition(wheelFL.collider.transform.up * -antiRollForce, wheelFL.collider.transform.position);
                }
                if (wheelFR.collider.GetGroundHit(out hit)) {
                    carRigidbody.AddForceAtPosition(wheelFR.collider.transform.up * antiRollForce, wheelFR.collider.transform.position);
                }

                // Rear axle
                travelLeft = GetSuspensionTravel(wheelRL.collider);
                travelRight = GetSuspensionTravel(wheelRR.collider);
                antiRollForce = (travelLeft - travelRight) * antiRollStiffness;
                if (wheelRL.collider.GetGroundHit(out hit)) {
                    carRigidbody.AddForceAtPosition(wheelRL.collider.transform.up * -antiRollForce, wheelRL.collider.transform.position);
                }
                if (wheelRR.collider.GetGroundHit(out hit)) {
                    carRigidbody.AddForceAtPosition(wheelRR.collider.transform.up * antiRollForce, wheelRR.collider.transform.position);
                }
            }

            wheelVelocity.fL = wheelFL.collider.rpm;
            wheelVelocity.fR = wheelFR.collider.rpm;
            wheelVelocity.rL = wheelRL.collider.rpm;
            wheelVelocity.rR = wheelRR.collider.rpm;

            // Update data
            steeringWheelAngleData.SetValue(steerAngle * steeringRatio);
            wheelVelocityData?.SetValue(wheelVelocity);

        }

        private void OnDestroy()
        {
            whlTqData.Unsubscribe(wheelTorqueAction);
            propulsiveDirData.Unsubscribe(propulsiveDirAction);
            rackForceData.Unsubscribe(rackForceAction);
            virtualDriverSteeringData.Unsubscribe(virtualDriverSteeringAction);
            userSteeringInputData.Unsubscribe(userSteeringInputAction);
            velocity.Unsubscribe(velocityAction);
        }

        private float GetSuspensionTravel(WheelCollider wheel)
        {
            float travel = 1.0f;
            if (wheel.GetGroundHit(out hit)) {
                travel = (-wheel.transform.InverseTransformPoint(hit.point).y - wheel.radius) / wheel.suspensionDistance;
            }
            return travel;
        }

        private void SetWheelTorque(WheelCollider wheelCollider, float requestedTorque, bool isFrontWheel = true)
        {

            if (wheelCollider == null)
                return;

            float propTorque, brake;

            if (requestedTorque >= 0) { // Propulsive torque
                brake = 0f;
                propTorque = Mathf.Abs(requestedTorque);
            } else { // Decelerating torque
                brake = Mathf.Abs(requestedTorque);
                propTorque = 0f;
            }

            WheelHit hit;
            wheelCollider.GetGroundHit(out hit);
            
            // Anti-lock braking (ABS)
            if (antilockBraking) {
                float torqueStep = 2f * Time.fixedDeltaTime;
                if (Mathf.Abs(hit.forwardSlip) * Mathf.Clamp01(brake) > 1.1f * wheelCollider.forwardFriction.asymptoteSlip) {
                    absFactor[wheelCollider] = wheelCollider.rpm < 0.01 ? // Wheel not spinning
                        Mathf.Min(Mathf.Clamp01(absFactor[wheelCollider] - torqueStep), 0.0f) :
                        Mathf.Clamp01(absFactor[wheelCollider] - 2f * torqueStep);
                    brake *= absFactor[wheelCollider];
                } else {
                    absFactor[wheelCollider] = Mathf.Clamp01(absFactor[wheelCollider] + 10 * torqueStep);
                    brake *= absFactor[wheelCollider];
                }
            }


            // Apply final torques
            propTorque = propulsiveDir == -1 ? -propTorque : propTorque;
            wheelCollider.motorTorque = propTorque;
            wheelCollider.brakeTorque = brake;

            if (brake > 0) {
                wheelCollider.forceAppPointDistance = Mathf.Lerp(wheelCollider.forceAppPointDistance, originalWheelRadius*0.8f, 5f * Time.fixedDeltaTime);
            } else {
                wheelCollider.forceAppPointDistance = wheelCollider.forceAppPointDistance = Mathf.Lerp(wheelCollider.forceAppPointDistance, originalWheelRadius, 5f * Time.fixedDeltaTime);
            }

        }

        private void ApplyRackForce(float rackForce)
        {
            if (wheelFL.collider == null || wheelFR.collider == null || wheelRL.collider == null | wheelRR.collider == null)
                return;

            // Use rack force to control wheel angle if driver support systems are active, otherwise the input from UI.
            if (virtualDriverSteering) {
                targetRackPos = rackForce / (6000f * Mathf.Pow(vehicleSpeed, 1.35f) + 100f); // approximation valid for steady state turning

                // Max speed instead of inertia (not real physics)
                if (Mathf.Abs(targetRackPos - currentRackPos) < maxRackTravelSpeed * Time.fixedDeltaTime) {
                    currentRackPos = targetRackPos;
                } else {
                    currentRackPos += Mathf.Sign(targetRackPos - currentRackPos) * maxRackTravelSpeed * Time.fixedDeltaTime;
                }
                currentRackPos = Mathf.Clamp(currentRackPos, -MAX_RACK_POSITION, MAX_RACK_POSITION);
            } else {
                currentRackPos = userSteeringInput * MAX_RACK_POSITION;
            }

            steerAngle = 6.5f * currentRackPos;

            // Ackermann steering 
            float rightAngle = Mathf.Atan(2f * WHEEL_BASE * Mathf.Sin(steerAngle) / (2f * WHEEL_BASE * Mathf.Cos(steerAngle) - TRACK * Mathf.Sin(steerAngle)));
            float leftAngle = Mathf.Atan(2f * WHEEL_BASE * Mathf.Sin(steerAngle) / (2f * WHEEL_BASE * Mathf.Cos(steerAngle) + TRACK * Mathf.Sin(steerAngle)));
            float ackermannRatio = Mathf.Min(0.58f * Mathf.Abs(steerAngle) / 0.605f, 1f);
            rightAngle = (1 - ackermannRatio) * steerAngle + ackermannRatio * rightAngle;
            leftAngle = (1 - ackermannRatio) * steerAngle + ackermannRatio * leftAngle;

            wheelFL.collider.steerAngle = leftAngle * 180f / Mathf.PI;
            wheelFR.collider.steerAngle = rightAngle * 180f / Mathf.PI;
        }


    }

}
