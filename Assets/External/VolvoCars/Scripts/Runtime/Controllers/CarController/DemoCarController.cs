﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DemoCarController : MonoBehaviour
{
    // For tutorial, see the Data section below and Start().
    public UDPReceive udpReceive;

    [Header("Information")]
    [SerializeField]
    private InfoText info = new InfoText("This demo controller lets you control the car using the axes named Horizontal, Vertical and Jump. " +
        "If you are using keyboard and standard Unity settings, this means either arrow keys or WASD together with Space.");

    [Header("Settings")]
    [SerializeField] private bool brakeToReverse = true;
    [SerializeField] private InfoText infoAboutCurves = new InfoText("The curves below describe available total wheel torque (Nm, y axis) vs vehicle speed (m/s, x axis).");
    [SerializeField] private AnimationCurve availableForwardTorque = AnimationCurve.Constant(0, 50, 2700);
    [SerializeField] private AnimationCurve availableReverseTorque = AnimationCurve.Linear(0, 2700, 15, 0);
    [SerializeField] [Tooltip("Print tutorial messages to console?")] private bool consoleMessages = true;

    [Header("Data")] // This is how you reference custom data, both for read and write purposes.
    [SerializeField] private VolvoCars.Data.PropulsiveDirection propulsiveDirection = default;
    [SerializeField] private VolvoCars.Data.WheelTorque wheelTorque = default;
    [SerializeField] private VolvoCars.Data.UserSteeringInput userSteeringInput = default;
    [SerializeField] private VolvoCars.Data.Velocity velocity = default;
    [SerializeField] private VolvoCars.Data.GearLeverIndication gearLeverIndication = default;
    [SerializeField] private VolvoCars.Data.DoorIsOpenR1L doorIsOpenR1L = default; // R1L stands for Row 1 Left.
    [SerializeField] private VolvoCars.Data.LampBrake lampBrake = default;

    #region Private variables not shown in the inspector
    private VolvoCars.Data.Value.Public.WheelTorque wheelTorqueValue = new VolvoCars.Data.Value.Public.WheelTorque(); // This is the value type used by the wheelTorque data item.     
    private VolvoCars.Data.Value.Public.LampGeneral lampValue = new VolvoCars.Data.Value.Public.LampGeneral(); // This is the value type used by lights/lamps
    private float totalTorque;  // The total torque requested by the user, will be split between the four wheels
    private float steeringReduction; // Used to make it easier to drive with keyboard in higher speeds
    public const float MAX_BRAKE_TORQUE = 8000; // [Nm]
    private bool brakeLightIsOn = false;
    Action<bool> doorIsOpenR1LAction; // Described more in Start()
    #endregion

    private void Start()
    {
        // Subscribe to data items this way. (There is also a SubscribeImmediate method if you don't need to be on the main thread / game loop.)
        // First define the action, i.e. what should happen when an updated value comes in:
        doorIsOpenR1LAction = isOpen =>
        {
            if (consoleMessages && Application.isPlaying)
                Debug.Log("This debug message is an example action triggered by a subscription to DoorIsOpenR1L in DemoCarController. Value: " + isOpen +
                    "\nYou can turn off this message by unchecking Console Messages in the inspector.");
        };
        // Then, add it to the subscription. In this script's OnDestroy() method we are also referencing this action when unsubscribing.
        doorIsOpenR1L.Subscribe(doorIsOpenR1LAction);

        // How to publish, example:
        // doorIsOpenR1L.Value = true;

        // End of tutorial
    }

    private void Update()
    {
        string data = udpReceive.data;
        data = data.Remove(0, 1);
        data = data.Remove(data.Length-1, 1);
        // print(data);
        string[] points = data.Split(',');

        // We are using Hand Point - 9
        float x = 10-float.Parse(points[9 * 3])/50;
        float y = float.Parse(points[9 * 3 + 1]) / 50;

        x = (x + 2)/12;
        y = (y - 7)/7;

        // If Enter is pressed, toggle the value of doorIsOpenR1L (toggle the state of the front left door).
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            doorIsOpenR1L.Value = !doorIsOpenR1L.Value;
        }
        
        // Driving inputs 
        // float rawSteeringInput = Input.GetAxis("Horizontal");
        // float rawForwardInput = Input.GetAxis("Vertical");
        float rawSteeringInput = x;
        float rawForwardInput = y;
        float parkInput = Input.GetAxis("Jump");

        // Steering
        steeringReduction = 1 - Mathf.Min(Mathf.Abs(velocity.Value) / 30f, 0.85f);
        userSteeringInput.Value = rawSteeringInput * steeringReduction;

        #region Wheel torques 

        if (parkInput > 0) { // Park request ("hand brake")
            if (Mathf.Abs(velocity.Value) > 5f / 3.6f) { 
                totalTorque = -MAX_BRAKE_TORQUE; // Regular brakes
            } else {
                totalTorque = -9000; // Parking brake and/or gear P
                propulsiveDirection.Value = 0;
                gearLeverIndication.Value = 0;
            }

        } else if (propulsiveDirection.Value == 1) { // Forward

            if (rawForwardInput >= 0 && velocity.Value > -1.5f) {
                totalTorque = Mathf.Min(availableForwardTorque.Evaluate(Mathf.Abs(velocity.Value)), -1800 + 7900 * rawForwardInput - 9500 * rawForwardInput * rawForwardInput + 9200 * rawForwardInput * rawForwardInput * rawForwardInput);
            } else {
                totalTorque = -Mathf.Abs(rawForwardInput) * MAX_BRAKE_TORQUE;
                if (Mathf.Abs(velocity.Value) < 0.01f && brakeToReverse) {
                    propulsiveDirection.Value = -1;
                    gearLeverIndication.Value = 1;
                }
            }

        } else if (propulsiveDirection.Value == -1) { // Reverse
            if (rawForwardInput <= 0 && velocity.Value < 1.5f) {
                float absInput = Mathf.Abs(rawForwardInput);
                totalTorque = Mathf.Min(availableReverseTorque.Evaluate(Mathf.Abs(velocity.Value)), -1800 + 7900 * absInput - 9500 * absInput * absInput + 9200 * absInput * absInput * absInput);
            } else {
                totalTorque = -Mathf.Abs(rawForwardInput) * MAX_BRAKE_TORQUE;
                if (Mathf.Abs(velocity.Value) < 0.01f) {
                    propulsiveDirection.Value = 1;
                    gearLeverIndication.Value = 3;
                }
            }

        } else { // No direction (such as neutral gear or P)
            totalTorque = 0;
            if (Mathf.Abs(velocity.Value) < 1f) {
                if (rawForwardInput > 0) {
                    propulsiveDirection.Value = 1;
                    gearLeverIndication.Value = 3;
                } else if (rawForwardInput < 0 && brakeToReverse) {
                    propulsiveDirection.Value = -1;
                    gearLeverIndication.Value = 1;
                }
            } else if(gearLeverIndication.Value == 0) {
                totalTorque = -9000; 
            }
        }

        ApplyWheelTorques(totalTorque);
        #endregion

        #region Lights
        bool userBraking = (rawForwardInput < 0 && propulsiveDirection.Value == 1) || (rawForwardInput > 0 && propulsiveDirection.Value == -1);
        if (userBraking && !brakeLightIsOn) {
            lampValue.r = 1; lampValue.g = 0; lampValue.b = 0;
            lampValue.intensity = 1;
            lampBrake.Value = lampValue;
            brakeLightIsOn = true;
        }else if (!userBraking && brakeLightIsOn) {
            lampValue.intensity = 0;
            lampBrake.Value = lampValue;
            brakeLightIsOn = false;
        }
        #endregion

    }

    private void OnDestroy()
    {
        doorIsOpenR1L.Unsubscribe(doorIsOpenR1LAction);
    }

    private void ApplyWheelTorques(float totalWheelTorque)
    {
        // Set the torque values for the four wheels.
        wheelTorqueValue.fL = 1.4f * totalWheelTorque / 4f;
        wheelTorqueValue.fR = 1.4f * totalWheelTorque / 4f;
        wheelTorqueValue.rL = 0.6f * totalWheelTorque / 4f;
        wheelTorqueValue.rR = 0.6f * totalWheelTorque / 4f;

        // Update the wheel torque data item with the new values. This is accessible to other scripts, such as chassis dynamics.
        wheelTorque.Value = wheelTorqueValue;
    }

}
