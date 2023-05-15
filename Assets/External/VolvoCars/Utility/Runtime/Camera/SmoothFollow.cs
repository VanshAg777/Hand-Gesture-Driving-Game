using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{

    [SerializeField] private InfoText info = new InfoText("Makes this transform smoothly follow and look " +
        "at the provided target. Adjust the characteristics with the parameters below.");

    public LayerMask terrainLayer;
    public float fov = 50;
    public float minHeightFromTerrain = 1;
    public float collisionDamping = 3;
    
    public Transform target;
    public Vector3 targetOffset = new Vector3();
    public float distance = 3;
    public float rotationY = 0;
    public float height = 1;
    public float heightDamping = 3;
    public float rotationDamping = 3;

    private float m_deltaTime;
    private Camera m_camera;

    private void Awake()
    {
        m_camera = Camera.main;
    }

    private void LateUpdate()
    {
        //set fov
        if(m_camera) m_camera.fieldOfView = fov;
        
        m_deltaTime = Time.deltaTime;
        
        //terrain collision
        Vector3 pointOnTerrain = PointOnTerrain(this.transform.position);
        float elevationFromCar = pointOnTerrain.y - target.position.y;
        
        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y + rotationY;
        float wantedHeight = target.position.y + targetOffset.y + height + (Mathf.Clamp(elevationFromCar,0, float.MaxValue));
        // float wantedHeight = target.position.y + targetOffset.y + height;
        
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;
        

        
        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * m_deltaTime);
        
        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * m_deltaTime);
        
        // Convert the angle into a rotation
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        
        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = target.TransformPoint(targetOffset);
        transform.position -= currentRotation * Vector3.forward * distance;
        
        // Set the height of the camera
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        
        // Always look at the target
        transform.LookAt(target.TransformPoint(targetOffset));
        
    }

    void TerrainCollision()
    {
        Vector3 pointOnTerrain = PointOnTerrain(this.transform.position);
        float terrainDist = Vector3.Distance(pointOnTerrain, this.transform.position);
        float currentHeight = transform.position.y;
        float wantedHeight = pointOnTerrain.y + minHeightFromTerrain;

        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, collisionDamping * Time.deltaTime);

        if (terrainDist < minHeightFromTerrain)
        {
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        }
    }

    Vector3 PointOnTerrain(Vector3 origin)
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << terrainLayer;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
        {
            return hit.point;
        }
        
        if (Physics.Raycast(origin, Vector3.up, out hit, Mathf.Infinity, terrainLayer))
        {
            return hit.point;
        }
        
        return origin;
    }
}