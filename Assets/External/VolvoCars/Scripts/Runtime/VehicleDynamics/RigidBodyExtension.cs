using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyExtension : MonoBehaviour
{
    public Transform centerOfMass;
    [HideInInspector] public float acceleration { get; private set; } = 0;

    private Rigidbody rigidbodyComponent;

    private float fixedDeltaTime;
    private int fixedUpdateCounter = 0;
    private const int filterTimeSpanFrames = 10;

    private float velocityWas = 0;
    private float[] accelerations = new float[filterTimeSpanFrames];

    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
        fixedDeltaTime = Time.fixedDeltaTime;

        if (centerOfMass != null && GetComponent<Rigidbody>() != null) {
            rigidbodyComponent.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }
    }

    private void FixedUpdate()
    {
        fixedUpdateCounter = ++fixedUpdateCounter % filterTimeSpanFrames;

        float velocity = transform.InverseTransformVector(rigidbodyComponent.velocity).z;
        float momentaryAcceleration = (velocity - velocityWas) / fixedDeltaTime;
        accelerations[fixedUpdateCounter] = momentaryAcceleration;
        acceleration = GetFilteredMean(accelerations, accelerations.Length);
        velocityWas = velocity;
    }

    private float GetFilteredMean(float[] array, int arrayLength)
    {
        float minVal = float.MaxValue;
        float maxVal = float.MinValue;
        float sum = 0;

        for (int i = 0; i < arrayLength; i++) {
            if (array[i] < minVal)
                minVal = array[i];
            if (array[i] > maxVal)
                maxVal = array[i];
            sum += array[i];
        }

        return (sum - minVal - maxVal) / (filterTimeSpanFrames - 2f);
    }

}
