using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWithOffset : MonoBehaviour
{
    
    public Transform target;
    public Vector3 positionOffset = new Vector3();
    public Vector3 rotationOffset = new Vector3();
    public bool mouseLook = false;

    private Vector3 mouseLookContribution = new Vector3();
    float rotationX = 0;
    float rotationY = 0;
    float sensitivity = 15;

    private void OnEnable()
    {
        mouseLookContribution = Vector3.zero;
        rotationX = 0;
        rotationY = 0;
    }

    private void LateUpdate()
    {
        transform.position = target.TransformPoint(positionOffset);
        

        if (mouseLook) {
            if (Input.GetMouseButton(1)) // If right mouse button is down
       {
                    rotationX +=  Input.GetAxis("Mouse X") * sensitivity;
                    rotationY += Input.GetAxis("Mouse Y") * sensitivity;
                    rotationY = Mathf.Clamp(rotationY, -120, 120);
                    mouseLookContribution = new Vector3(-rotationY, rotationX, 0);
            }
        }
        
        transform.rotation = target.transform.rotation * Quaternion.Euler(rotationOffset + mouseLookContribution);
    }
}
