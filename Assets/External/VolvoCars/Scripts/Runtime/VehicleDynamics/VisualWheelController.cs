using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualWheelController : MonoBehaviour
{
    [SerializeField] InfoText angleInfo = new InfoText("Angles below in degrees.");
    [Range(0, 10)]
    public float casterAngle = 7;
    [Range(5, 15)]
    public float steeringAxisInclination = 10;
    [Range(-0.05f, 0.05f)]
    public float scrubRadius = -0.02f;
    [Range(0, 0.15f)]
    public float trail = 0.07f;

    Vector3 newWheelPos;
    Quaternion newWheelRot;
    WheelCollider wheelCollider;
    GameObject spinningArt, nonSpinningArt;
    bool updateNonSpinning = false;
    Vector3 posVector = new Vector3();
    Vector3 steeringAxis = new Vector3();
    float forwardRotation = 0;

    Quaternion originalSpinningArtLocalRotation = new Quaternion();
    Quaternion originalNonSpinningArtLocalRotation = new Quaternion();

    Vector3 steeringAxisPoint = new Vector3();
    float wheelRadius;
    float sideMultiplier = 1; // -1 for right, set to 1 for left in Start;
    Vector3 tmpVector = new Vector3(0,1,0);

    private void Start()
    {
        wheelCollider = transform.parent.GetComponent<WheelCollider>();
        if (wheelCollider == null)
            Debug.LogError("Wheel collider is missing.");

        wheelRadius = wheelCollider.radius;

        spinningArt = transform.GetChild(0).gameObject;
        if(transform.childCount > 1) {
            nonSpinningArt = transform.GetChild(1).gameObject;
            originalNonSpinningArtLocalRotation = nonSpinningArt.transform.localRotation;
            updateNonSpinning = true;
        }
        originalSpinningArtLocalRotation = spinningArt.transform.localRotation;
        
        sideMultiplier = transform.InverseTransformPoint(wheelCollider.attachedRigidbody.transform.position).x >= 0 ? 1 : -1;

    }

    void Update()
    {
        // Update position
        wheelCollider.GetWorldPose(out newWheelPos, out newWheelRot);
        transform.position = newWheelPos;
        posVector.y = transform.localPosition.y;
        transform.localPosition = posVector;

        // Set temporarily to nominal rotation
        spinningArt.transform.localRotation = originalSpinningArtLocalRotation;
        if(updateNonSpinning)
            nonSpinningArt.transform.localRotation = originalNonSpinningArtLocalRotation;

        // Steering rotation
        UpdateSteeringAxis();
        spinningArt.transform.Rotate(steeringAxis, wheelCollider.steerAngle, Space.World);
        if(updateNonSpinning)
            nonSpinningArt.transform.Rotate(steeringAxis, wheelCollider.steerAngle, Space.World);
        
        // Spinning rotation
        if (Mathf.Abs(wheelCollider.rpm) > 10 || !wheelCollider.isGrounded){
            forwardRotation += wheelCollider.rpm * 6 * Time.deltaTime;
        } else {
            float velocity = wheelCollider.attachedRigidbody.transform.InverseTransformVector(wheelCollider.attachedRigidbody.velocity).z;
            forwardRotation += (velocity * Time.deltaTime) / (2 * Mathf.PI * wheelRadius) * 360f;
        }
        forwardRotation = forwardRotation % 360f;
        spinningArt.transform.Rotate(spinningArt.transform.right, forwardRotation, Space.World);

    }

    private void UpdateSteeringAxis()
    {
        steeringAxisPoint = transform.TransformPoint(sideMultiplier * scrubRadius, -wheelRadius, trail);
        tmpVector.x = Mathf.Tan(sideMultiplier * steeringAxisInclination * Mathf.Deg2Rad);
        tmpVector.z = Mathf.Tan(-casterAngle * Mathf.Deg2Rad);
        steeringAxis = (transform.TransformPoint(tmpVector) - steeringAxisPoint).normalized;
    }


    private void OnDrawGizmosSelected()
    {
        wheelCollider = transform.parent.GetComponent<WheelCollider>();
        wheelRadius = wheelCollider.radius;

        // Draw the wheel's vertical axis
        Debug.DrawLine(transform.position + transform.up * wheelRadius, transform.position - transform.up * wheelRadius, Color.white);

        // Draw the steering axis
        sideMultiplier = transform.InverseTransformPoint(wheelCollider.attachedRigidbody.transform.position).x >= 0 ? 1 : -1;
        UpdateSteeringAxis();
        Debug.DrawRay(steeringAxisPoint, steeringAxis * 2*wheelRadius, Color.green);
        Debug.DrawLine(steeringAxisPoint, transform.position - transform.up* wheelRadius, Color.magenta);
    }
}
