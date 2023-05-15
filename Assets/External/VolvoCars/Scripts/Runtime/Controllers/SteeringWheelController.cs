using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolvoCars.Behaviour
{
    public class SteeringWheelController : MonoBehaviour
    {
        [SerializeField] Transform steeringWheelTransform;
        [SerializeField] Data.SteeringWheelAngle steeringWheelAngle;
        private Quaternion originalRotation;

        private void Awake()
        {
            originalRotation = steeringWheelTransform.localRotation;
        }

        void Update()
        {
            steeringWheelTransform.localRotation = originalRotation * Quaternion.Euler(-steeringWheelAngle.Value*Mathf.Rad2Deg, 0, 0);
        }
    }

}