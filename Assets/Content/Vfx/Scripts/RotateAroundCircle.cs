using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class RotateAroundCircle : MonoBehaviour
{
    public bool rotate;
    public Transform center;
    public float rotateSpeed = 1.0f;
    public float rotateRadius = 1.0f;

    [SerializeField] protected Vector3 lastPosition = new Vector3(0,0,0);
    [SerializeField] protected float distance;

    [SerializeField] protected VisualEffect vfx;

    // Update is called once per frame
    void Update()
    {
        if (transform)
        {
            Rotate();
            ComputeSlip();
            SetConstants();
        }
        
    }

    void Rotate()
    {
        if(rotate)
            this.transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);
        
    }
    
    void ComputeSlip()
    {
        distance = Vector3.Distance(this.transform.position, lastPosition);
        lastPosition = this.transform.position;
    }

    void SetConstants()
    {
        if (vfx)
        {
            vfx.SetFloat("_SlipAmount", distance);
        }
    }
}
