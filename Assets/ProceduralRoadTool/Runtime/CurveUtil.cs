using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace ProceduralRoadTool
{
    public static class CurveUtil
    {
        #region Sampling Position, Tangent, Rotation on Curve

        public static Vector3 SamplePositionUniform(CinemachineSmoothPath path, float position)
        {
            return path.EvaluatePositionAtUnit(position, CinemachinePathBase.PositionUnits.Distance);
        }

        public static Vector3 SampleTangentUniform(CinemachineSmoothPath path, float position)
        {
            return path.EvaluateTangentAtUnit(position, CinemachinePathBase.PositionUnits.Distance);
        }

        public static Quaternion SampleRotationUniform(CinemachineSmoothPath path, float position)
        {
            return path.EvaluateOrientationAtUnit(position, CinemachinePathBase.PositionUnits.Distance);
        }
        
        public static Vector3 SamplePositionPathUnits(CinemachineSmoothPath path, float position)
        {
            return path.EvaluatePositionAtUnit(position, CinemachinePathBase.PositionUnits.PathUnits);
        }

        public static Vector3 SampleTangentPathUnits(CinemachineSmoothPath path, float position)
        {
            return path.EvaluateTangentAtUnit(position, CinemachinePathBase.PositionUnits.PathUnits);
        }

        public static Quaternion SampleRotationPathUnits(CinemachineSmoothPath path, float position)
        {
            return path.EvaluateOrientationAtUnit(position, CinemachinePathBase.PositionUnits.PathUnits);
        }

        public static Vector3 SamplePositionRelative(CinemachineSmoothPath path, float position)
        {
            return path.EvaluatePosition(position);
        }

        public static Quaternion SampleRotationRelative(CinemachineSmoothPath path, float position)
        {
            return path.EvaluateOrientation(position);
        }
        
        #endregion
        
        #region General Curve Methods
        public static float GetNearestPointOnCurve(CinemachineSmoothPath path, Vector3 worldPos, int start = 0, int search = -1, int steps = 1024)
        {
            float curvePos = path.FindClosestPoint(worldPos, start, search, steps);
            Vector3 position = path.EvaluatePositionAtUnit(curvePos, CinemachinePathBase.PositionUnits.PathUnits);
            
            return curvePos;
        }


        public static Vector3 GetNearestPositionOnCurve(CinemachineSmoothPath path, Vector3 worldPos, int start = 0, int search = -1, int steps = 1024)
        {
            float curvePos = path.FindClosestPoint(worldPos, start, search, steps);
            Vector3 position = path.EvaluatePositionAtUnit(curvePos, CinemachinePathBase.PositionUnits.PathUnits);
            
            return position;
        }

        public static void SnapSmoothPathToTerrains(CinemachineSmoothPath path, LayerMask terrainLayer, Transform transform)
        {
            int len = path.m_Waypoints.Length;
            CinemachineSmoothPath.Waypoint[] waypoints = (CinemachineSmoothPath.Waypoint[]) path.m_Waypoints.Clone();
            path.m_Waypoints = new CinemachineSmoothPath.Waypoint[len];

            for (int i = 0; i < len; i++)
            {
                path.m_Waypoints[i] = waypoints[i];

                Vector3 localToWorld = MeshHelpers.TransformLocalToWorld(path.m_Waypoints[i].position, transform);
                Vector3 snappedPosition = MeshHelpers.SnapObjectToTerrain(terrainLayer, localToWorld);
                Vector3 worldToLocal = MeshHelpers.TransformWorldToLocal(snappedPosition, transform);

                path.m_Waypoints[i].position = worldToLocal;
            }
                
            path.InvalidateDistanceCache();
        }
        
        #endregion
    }
}