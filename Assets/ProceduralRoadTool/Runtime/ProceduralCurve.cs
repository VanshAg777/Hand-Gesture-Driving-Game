using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ProceduralRoadTool
{
    [System.Serializable]
    public class Orientation
    {
        public Vector3 forward;
        public Vector3 up;
        public Vector3 side;

        public Orientation(Vector3 forward, Vector3 up, Vector3 side)
        {
            this.forward = forward;
            this.up = up;
            this.side = side;
        }
    }

    [System.Serializable]
    public class CurveControlPoint
    {
        public Vector3 worldPosition;
        public Vector3 localPositions;
        public Quaternion rotation;
        public Orientation orientation;
        public float curveCoeff;
        public float angleDiff;

        public CurveControlPoint(Vector3 wpos, Quaternion rot, Orientation o = null)
        {
            this.worldPosition = wpos;
            this.rotation = rot;
            this.orientation = o;
        }

    }
    
    public class ProceduralCurve : MonoBehaviour
    {
        public CinemachineSmoothPath path;
        
        protected string m_name;
        protected Transform m_meshTransform;
        protected float m_pathLength;
        protected List<CurveControlPoint> m_controlPoints;

        
        
        #region Properties
        public string curveName { get => m_name; set => m_name = value; }
        public Transform meshTransform { get => m_meshTransform; set => m_meshTransform = value; }
        public float pathLength { get => m_pathLength; set => m_pathLength = value; }
        public List<CurveControlPoint> controlPoints 
        { 
            get => m_controlPoints;
            set
            {
                m_controlPoints = value;
                ComputePathLength();
                ComputeCurveCoeff(value.Count / 10); //todo make it relative to path len and point count
            }
        }
        #endregion

        public ProceduralCurve(string name, CinemachineSmoothPath.Waypoint[] waypoints = null, bool isLopped = true)
        {
            this.m_name = name;
            this.m_controlPoints = new List<CurveControlPoint>();
        }

        
        
        //called after setting control points
        public void CreatePathFromControlPoints(bool looped = true)
        {
            if (!path) return;
            
            path.m_Looped = looped;

            int len = m_controlPoints.Count;
            path.m_Waypoints = new CinemachineSmoothPath.Waypoint[len];
            CinemachineSmoothPath.Waypoint[] waypoints = (CinemachineSmoothPath.Waypoint[]) path.m_Waypoints.Clone();

            for (int i = 0; i < len; i++)
            {
                path.m_Waypoints[i].position = m_controlPoints[i].worldPosition;
            }

            path.InvalidateDistanceCache();
        }
        

        public void ComputePathLength()
        {
            float len = 0.0f;
            for (int i = 0; i < m_controlPoints.Count; i++)
            {
                Vector3 p1, p2;
                p1 = m_controlPoints[i].worldPosition;
                
                //loop back to start
                if (i + 1 == m_controlPoints.Count)
                    p2 = m_controlPoints[0].worldPosition;
                else
                    p2 = m_controlPoints[i + 1].worldPosition;
                
                float distamce = Vector3.Distance(p1, p2);
                len += distamce;
            }

            m_pathLength = len;
        }
        
        
        public void ComputeCurveCoeff(int forwardSampleCount)
        {
            // Orientation[] orientations = this.orientations.ToArray();
            Vector3[] forwardVectors = new Vector3[m_controlPoints.Count];
            for (int i = 0; i < m_controlPoints.Count; i++)
            {
                forwardVectors[i] = m_controlPoints[i].orientation.forward;
            }


            for (int i = 0; i < m_controlPoints.Count; i++)
            {
                m_controlPoints[i].curveCoeff = GetTrackTurn(forwardVectors, i, forwardSampleCount);
                m_controlPoints[i].angleDiff = GetTrackTurnAngle(forwardVectors, i, forwardSampleCount);
            }
        }

        public Vector3 AverageVectors(Vector3[] samples, int startIndex, int forwardSampleCount)
        {
            Vector3 sum = Vector3.zero;
            int len = samples.Length;
            int targetIndex = startIndex + forwardSampleCount;

            if (targetIndex >= len)
            {
                for (int i = startIndex; i < targetIndex; i++)
                {
                    int index = i % len;
                    sum += samples[index];
                }
            }
            else
            {
                for (int i = startIndex; i < targetIndex; i++)
                {
                    sum += samples[i];
                }

            }

            return sum / forwardSampleCount;
        }
        

        public float DotProduct(Vector3 origin, Vector3 target)
        {
            return Vector3.Dot(origin, target);
        }

        public float GetTrackTurn(Vector3[] samples, int startIndex, int forwardSampleCount)
        {
            return DotProduct(samples[startIndex], samples[(startIndex + forwardSampleCount) % samples.Length]);
        }
        public float GetTrackTurnAngle(Vector3[] samples, int startIndex, int forwardSampleCount)
        {
            return Vector3.Angle(samples[startIndex], samples[(startIndex + forwardSampleCount) % samples.Length]);
        }

        #region Gizmos
        public void DrawGizmos()
        {
            //if (m_controlPoints == null)
            //    return;

            //if (path)
            //{
            //    for (int i = 0; i < path.m_Waypoints.Length; i++)
            //    {
            //        var w = path.m_Waypoints[i];
            //        //DRAW WORLDPOSITIONS
            //        Gizmos.color = Color.blue;
            //        Gizmos.DrawSphere(w.position, 0.25f);
            //    }
            //}
            
            //for (int i = 0; i < m_controlPoints.Count; i++)
            //{
            //    //DRAW ORIENTATIONS forward, side, up
            //    Gizmos.color = Color.cyan;
            //    Gizmos.DrawLine(m_controlPoints[i].worldPosition,
            //        m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.forward);

            //    Gizmos.color = Color.red;
            //    Gizmos.DrawLine(m_controlPoints[i].worldPosition,
            //        m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.side);

            //    Gizmos.color = Color.green;
            //    Gizmos.DrawLine(m_controlPoints[i].worldPosition,
            //        m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.up);

            //    //DRAW WORLDPOSITIONS
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawSphere(m_controlPoints[i].worldPosition, 0.025f);
                
            //    //DRAW curve coeefff
            //    Gizmos.color = Color.black;
            //    Gizmos.DrawSphere(m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.up,
            //        (m_controlPoints[i].curveCoeff * 2.0f - 1.0f) * 0.1f);
                
            //    //DRAW angle diffence
            //    Gizmos.color = Color.yellow;
            //    Gizmos.DrawLine(m_controlPoints[i].worldPosition,
            //        m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.up *
            //        (5 * (m_controlPoints[i].angleDiff / 360.0f)));
            //    Gizmos.DrawSphere(m_controlPoints[i].worldPosition + m_controlPoints[i].orientation.up *
            //        (5 * (m_controlPoints[i].angleDiff / 360.0f)), 0.1f);
            //}


        }
        
        #endregion
    }
}