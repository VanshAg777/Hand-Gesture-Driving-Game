using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelColliderExtension : MonoBehaviour
{
    [SerializeField] private InfoText info = new InfoText("A script attempting to make the wheel colliders feel a bit more round when driving over obstacles.");

    private int numberOfRays = 13;
    private float maxAngle = 115;

    private WheelCollider wheelCollider;
    private float originalWheelRadius;

    private RaycastHit[] raycastHits = new RaycastHit[2];
    private RaycastHit[] mainRaycastHits = new RaycastHit[2];
    private RaycastHit emptyHit = new RaycastHit();

    float mainOffset;
    Vector3 hitPoint;

    //Performance
    int iStart = 0;
    int iStep = 1;

    void Start()
    {
        wheelCollider = gameObject.GetComponent<WheelCollider>();
        originalWheelRadius = wheelCollider.radius;
    }


    void FixedUpdate()
    {
        mainOffset = GetMainOffset();
        hitPoint = GetCollisionData(mainOffset, out float hitAngle, out float hitDistance);

        wheelCollider.radius = originalWheelRadius + (originalWheelRadius - hitDistance);

        float reactionTorque = originalWheelRadius * Mathf.Sin(hitAngle / 180f * Mathf.PI) * 4500f;
        reactionTorque = Mathf.Abs(reactionTorque) < 0.5f ? 0 : reactionTorque;
        wheelCollider.attachedRigidbody.AddRelativeForce(0, 0, reactionTorque / originalWheelRadius);
    }

    /// <summary>
    /// Returns the measured distance from the wheel collider transform's origin to the ground, minus the visual wheel's radius. 
    /// </summary>
    /// <returns></returns>
    private float GetMainOffset()
    {
        // Straight down, main raycast
        float offset = wheelCollider.suspensionDistance;
        for (int i = 0; i < mainRaycastHits.Length; i++) {
            mainRaycastHits[i] = emptyHit;
        }
        if (Physics.RaycastNonAlloc(transform.position, -wheelCollider.transform.up, mainRaycastHits, originalWheelRadius + wheelCollider.suspensionDistance, ~0, QueryTriggerInteraction.Ignore) > 0) {
            float measuredDistance = float.PositiveInfinity;
            for (int i = 0; i < mainRaycastHits.Length; i++) {
                if (mainRaycastHits[i].collider != null) {

                    if ((mainRaycastHits[i].distance) < measuredDistance) {
                        measuredDistance = mainRaycastHits[i].distance;
                        offset = measuredDistance - originalWheelRadius;
                    }
                }
            }
        }
        return offset;
    }

    private Vector3 GetCollisionData(float mainOffset, out float hitAngle, out float hitDistance, bool drawRays = false)
    {
        Vector3 definingHitPoint = transform.position - wheelCollider.transform.up * (mainOffset + originalWheelRadius);
        float angleStep = maxAngle / (numberOfRays - 1);
        float angle = 999;
        hitAngle = 0;
        hitDistance = originalWheelRadius;
        int iHit = 0;
        Vector3 rayDirection = new Vector3();


        bool refineMeasurement = false;
        if (wheelCollider.radius > (originalWheelRadius + 0.01f) || drawRays) {
            iStart = 0;
            iStep = 1;
            refineMeasurement = true;
        } else {
            iStart++;
            iStart = iStart > 2 ? 0 : iStart;
            iStep = 3;
        }

        for (int i = iStart; i < numberOfRays; i += iStep) {
            angle = (float)i * angleStep + (90f - maxAngle / 2f);
            if (Mathf.Abs(angle - 90) < 0.001f) {
                rayDirection = -wheelCollider.transform.up;
            } else {
                rayDirection = Quaternion.AngleAxis(wheelCollider.steerAngle, wheelCollider.transform.up) * Quaternion.AngleAxis(angle, wheelCollider.transform.right) * wheelCollider.transform.forward;
            }
            for (int iClear = 0; iClear < raycastHits.Length; iClear++) {
                raycastHits[iClear] = emptyHit;
            }
            if (Physics.RaycastNonAlloc(transform.position - wheelCollider.transform.up * mainOffset, rayDirection, raycastHits, originalWheelRadius, ~0, QueryTriggerInteraction.Ignore) > 0) {
                if (EvaluateRaycastHits(angle, ref hitAngle, ref hitDistance, ref definingHitPoint)) {
                    iHit = i;
                }
            } else if (drawRays) {
                Debug.DrawRay(transform.position - wheelCollider.transform.up * mainOffset, rayDirection * originalWheelRadius, Color.green);
            }
        }


        // Refine around hit
        if (refineMeasurement) {
            for (float i = -1; i <= 1; i++) {
                if (i != 0) {
                    angle = (float)iHit * angleStep + i * (angleStep / 2f) + (90f - maxAngle / 2f);
                    rayDirection = Quaternion.AngleAxis(wheelCollider.steerAngle, wheelCollider.transform.up) * Quaternion.AngleAxis(angle, wheelCollider.transform.right) * wheelCollider.transform.forward;
                    for (int iClear = 0; iClear < raycastHits.Length; iClear++) {
                        raycastHits[iClear] = emptyHit;
                    }
                    if (Physics.RaycastNonAlloc(transform.position - wheelCollider.transform.up * mainOffset, rayDirection, raycastHits, originalWheelRadius, ~0, QueryTriggerInteraction.Ignore) > 0) {
                        EvaluateRaycastHits(angle, ref hitAngle, ref hitDistance, ref definingHitPoint);
                    } else if (drawRays) {
                        Debug.DrawRay(transform.position - wheelCollider.transform.up * mainOffset, rayDirection * originalWheelRadius, Color.green);
                    }
                }
            }
        }

        return definingHitPoint;
    }

    private bool EvaluateRaycastHits(float angle, ref float hitAngle, ref float hitDistance, ref Vector3 definingHitPoint)
    {
        float previousAngle = hitAngle;
        for (int j = 0; j < raycastHits.Length; j++) {
            if (raycastHits[j].collider != null) {
                if (raycastHits[j].collider.name != "BodyCollider") {

                    if (raycastHits[j].distance < hitDistance) {
                        definingHitPoint = raycastHits[j].point;
                        hitAngle = angle - 90f;
                        hitDistance = raycastHits[j].distance;
                        break;
                    }

                }
            }
        }
        return hitAngle != previousAngle;
    }

    private void OnValidate()
    {
        if (numberOfRays <= 2) {
            numberOfRays = 3;
        }
        if (numberOfRays % 2 == 0) {
            numberOfRays++;
        }

        Mathf.Clamp(maxAngle, 0f, 170f);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) {
            Debug.DrawLine(transform.position - wheelCollider.transform.up * mainOffset, hitPoint, Color.red);
            Debug.DrawRay(transform.position, -wheelCollider.transform.up * mainOffset, Color.yellow);
            Debug.DrawRay(transform.position - wheelCollider.transform.up * (mainOffset + originalWheelRadius), -wheelCollider.transform.up * (wheelCollider.suspensionDistance - mainOffset), Color.white);
        }
    }

}
