using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarReset : MonoBehaviour
{
    public GameObject car;
    public Rigidbody rb;
    private Vector3 m_initPosition;
    private Quaternion m_initRotation;

    private void Start()
    {
        RecordInitialTransform();
    }

    private void Update()
    {
        float parkInput = Input.GetAxis("Car Reset");
        if (parkInput != 0)
        {
            ResetCar();
        }
    }

    public void RecordInitialTransform()
    {
        m_initPosition = car.transform.position;
        m_initRotation = car.transform.rotation; 
    }

    public void ResetCar()
    {
        rb.Sleep();
        car.transform.position = m_initPosition;
        car.transform.rotation = m_initRotation;
        rb.WakeUp();
    }
}