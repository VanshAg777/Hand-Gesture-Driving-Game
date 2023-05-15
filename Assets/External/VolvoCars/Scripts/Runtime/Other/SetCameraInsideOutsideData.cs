using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraInsideOutsideData : MonoBehaviour
{
    public string triggerColliderName = "BodyCollider";

    [Header("Data")]
    public VolvoCars.Data.CameraIsInsideCar cameraIsInsideCar;

    private void OnTriggerEnter(Collider other)
    {
        bool changeValue = false;

        if(other.name == triggerColliderName) {
            changeValue = true;
        }else if (other.transform.parent != null) {
            if(other.transform.parent.name == triggerColliderName) {
                changeValue = true;
            }
        }

        if (changeValue) {
            cameraIsInsideCar.Value = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool changeValue = false;

        if (other.name == triggerColliderName) {
            changeValue = true;
        } else if (other.transform.parent != null) {
            if (other.transform.parent.name == triggerColliderName) {
                changeValue = true;
            }
        }

        if (changeValue) {
            cameraIsInsideCar.Value = false;
        }
    }

}
