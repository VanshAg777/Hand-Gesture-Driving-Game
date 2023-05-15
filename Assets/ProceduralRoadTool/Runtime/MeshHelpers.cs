using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace ProceduralRoadTool
{
    public static class MeshHelpers
    {
        public static Vector3 TransformLocalToWorld(Vector3 vertex, Transform owner)
        {
            return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
        }

        public static Vector3 TransformWorldToLocal(Vector3 vertex, Transform owner)
        {
            return owner.worldToLocalMatrix.MultiplyPoint3x4(vertex);
        }
        
        public static Vector3 SnapObjectToTerrain(int layer, Vector3 origin)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, layer))
            {
                return hit.point;
            }
            if (Physics.Raycast(origin, Vector3.up, out hit, Mathf.Infinity, layer))
            {
                return hit.point;
            }

            return origin; //origin is the original position of the procedural object
        }
        
        public static Mesh CopyMesh(Mesh src)
        {
            Mesh newmesh = new Mesh();
            newmesh.vertices = src.vertices;
            newmesh.triangles = src.triangles;
            newmesh.uv = src.uv;
            newmesh.normals = src.normals;
            newmesh.colors = src.colors;
            newmesh.tangents = src.tangents;
            newmesh.name = src.name + "_deformed";

            return newmesh;
        }

        public static float GetMeshLength(Axis axis, GameObject obj, bool worldSpace)
        {
            var filter = obj.GetComponent<MeshFilter>();
            var srcMesh = filter.sharedMesh;
            
            Vector3[] vertices = srcMesh.vertices;

            float meshLength = float.MinValue;
            float minVertexPos = float.MaxValue;
            float maxVertexPos = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 pos = worldSpace ? obj.transform.TransformPoint(vertices[i]): vertices[i];
                if (pos[(int) axis % 3] <= minVertexPos)
                    minVertexPos = pos[(int) axis % 3];
                if (pos[(int) axis % 3] >= maxVertexPos)
                    maxVertexPos = pos[(int) axis % 3];

            }
            
            //distance between max and min vertex
            meshLength = (maxVertexPos - minVertexPos);
            return meshLength;
        }
        
        public static void DeformMeshAlongCurve(Axis deformAxis, CinemachineSmoothPath path, int index, GameObject obj, float lengthPerUnit)
        {
            var filter = obj.GetComponent<MeshFilter>();
            var srcMesh = filter.sharedMesh;

            float start = index * lengthPerUnit;
            float end = start + lengthPerUnit;

            //if its runnig and contains deformed, stop
            if (Application.isPlaying && srcMesh.name.ToLower().Contains("_deformed") || path == null)
                return;

            Mesh generatedMesh = new Mesh();

            //assign meshes
            generatedMesh = srcMesh;
            if (!generatedMesh.name.ToLower().Contains("_deformed"))
                generatedMesh = MeshHelpers.CopyMesh(srcMesh);

            filter.sharedMesh = generatedMesh;

            Vector3[] vertices = srcMesh.vertices;

            float vertexLength = float.MinValue;
            float minVertexPos = float.MaxValue;
            float maxVertexPos = float.MinValue;

            //assign min max position based on axis
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 pos = vertices[i];
                if (pos[(int) deformAxis % 3] <= minVertexPos)
                    minVertexPos = pos[(int) deformAxis % 3];
                if (pos[(int) deformAxis % 3] >= maxVertexPos)
                    maxVertexPos = pos[(int) deformAxis % 3];

            }

            //distance between max and min vertex
            vertexLength = (maxVertexPos - minVertexPos);


            for (int i = 0; i < vertices.Length; i++)
            {
                VertexDeformData data = new VertexDeformData(vertices[i], vertexLength, minVertexPos, maxVertexPos, start, end);
                vertices[i] = TransformVertexToCurve(deformAxis, path, data, lengthPerUnit);

            }

            generatedMesh.vertices = vertices;
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateBounds();
        }


        public static Vector3 TransformVertexToCurve(Axis deformAxis, CinemachineSmoothPath path, VertexDeformData vertex, float lengthPerUnit)
        {
            float curveLength = path.PathLength;
            float remappedStartEnd = CommonUtil.RemapFloat(vertex.meshVertex[(int) deformAxis % 3], vertex.min, vertex.max, 0, lengthPerUnit);
            float unitPos = vertex.startOffset + remappedStartEnd;

            Vector3 splinePoint = path.EvaluatePositionAtUnit(unitPos / curveLength, CinemachinePathBase.PositionUnits.Normalized);
            Vector3 futureSplinePoint = path.EvaluatePositionAtUnit((unitPos + 0.01f) / curveLength, CinemachinePathBase.PositionUnits.Normalized);


            Vector3 forwardVector = futureSplinePoint - splinePoint;
            Quaternion imaginaryPlaneRotation = path.EvaluateOrientationAtUnit(unitPos, CinemachinePathBase.PositionUnits.Distance);

            Vector3 pointWithinPlane = new Vector3(vertex.meshVertex.x, vertex.meshVertex.y, 0f);

            switch (deformAxis)
            {
                case Axis.x:
                    pointWithinPlane = new Vector3(vertex.meshVertex.y, vertex.meshVertex.z, 0f);
                    break;
                case Axis.y:
                    pointWithinPlane = new Vector3(vertex.meshVertex.z, vertex.meshVertex.x, 0f);
                    break;
                case Axis.z:
                    pointWithinPlane = new Vector3(vertex.meshVertex.x, vertex.meshVertex.y, 0f);
                    break;
                case Axis.minus_x:
                    pointWithinPlane = new Vector3(-vertex.meshVertex.y, -vertex.meshVertex.z, 0f);
                    break;
                case Axis.minus_y:
                    pointWithinPlane = new Vector3(-vertex.meshVertex.z, -vertex.meshVertex.x, 0f);
                    break;
                case Axis.minus_z:
                    pointWithinPlane = new Vector3(-vertex.meshVertex.x, vertex.meshVertex.y, 0f);
                    break;
            }
            

            return splinePoint + imaginaryPlaneRotation * pointWithinPlane;
        }

        public static Vector3 TransformVertexToNearestCurvePoint(Axis deformAxis, CinemachineSmoothPath path, GameObject obj, VertexDeformData v)
        {

            Vector3 localToWorld = obj.transform.TransformPoint(v.meshVertex);
            float nearestPoint = CurveUtil.GetNearestPointOnCurve(path, localToWorld);

            Vector3 splinePoint = CurveUtil.SamplePositionPathUnits(path, nearestPoint);
            splinePoint = obj.transform.InverseTransformPoint(splinePoint);
            Quaternion imaginaryPlaneRotation = CurveUtil.SampleRotationPathUnits(path, nearestPoint);
            
            Vector3 pointWithinPlane = new Vector3(v.meshVertex.x, v.meshVertex.y, 0f);

            switch (deformAxis)
            {
                case Axis.x:
                    pointWithinPlane = new Vector3(v.meshVertex.y, v.meshVertex.z, 0f);
                    break;
                case Axis.y:
                    pointWithinPlane = new Vector3(v.meshVertex.z, v.meshVertex.x, 0f);
                    break;
                case Axis.z:
                    pointWithinPlane = new Vector3(v.meshVertex.x, v.meshVertex.y, 0f);
                    break;
                case Axis.minus_x:
                    pointWithinPlane = new Vector3(-v.meshVertex.y, -v.meshVertex.z, 0f);
                    break;
                case Axis.minus_y:
                    pointWithinPlane = new Vector3(-v.meshVertex.z, -v.meshVertex.x, 0f);
                    break;
                case Axis.minus_z:
                    pointWithinPlane = new Vector3(-v.meshVertex.x, v.meshVertex.y, 0f);
                    break;
            }

            return splinePoint + imaginaryPlaneRotation * pointWithinPlane;
        }
        
    }
}