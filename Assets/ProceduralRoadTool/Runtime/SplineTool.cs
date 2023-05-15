using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Cinemachine;
using ProceduralRoadTool;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;    

namespace ProceduralRoadTool
{


    //todo add -X,-Y,-Z support
    public enum Axis
    {
        x,
        y,
        z,
        minus_x,
        minus_y,
        minus_z
    }


    [System.Serializable]
    public class VertexDeformData
    {
        public Vector3 meshVertex;
        public float meshLength;
        public float min;
        public float max;
        public float startOffset; //on curve
        public float endOffset; //on curve

        public VertexDeformData(Vector3 meshVertex, float meshLength, float min, float max, float startOffset,
            float endOffset)
        {
            this.meshVertex = meshVertex;
            this.meshLength = meshLength;
            this.min = min;
            this.max = max;
            this.startOffset = startOffset;
            this.endOffset = endOffset;
        }
    }


    [RequireComponent(typeof(CinemachineSmoothPath))]
    public class SplineTool : MonoBehaviour
    {
        [Header("Gizmos")] 
        public bool showGizmos;
        public bool updateRealTime;

        [Header("Terrain Snapping")]
        public bool snapSmoothPathToTerrain;
        public bool snapObjectsToTerrain;
        public LayerMask terrainLayer;


        [Header("Parameters")] 
        public CinemachineSmoothPath path;
        public Axis deformationAxis = Axis.z;

        [Header("Mesh Params")] 
        public GameObject baseMesh;
        public GameObject startMesh;
        public GameObject endMesh;

        protected int m_iterations = 1;
        protected float m_curveLength;
        protected float m_lengthPerUnit;

        [Header("Procedural Object Parameters")] 
        public ProceduralObjectBaseParameters parameters;
        
        
        protected List<GameObject> m_objects = new List<GameObject>();
        protected List<ProceduralObject> m_proceduralObjects = new List<ProceduralObject>();

        public void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.update += GenerateEditor;
#endif
            
            ProceduralObjectsTransformUpdate();
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            EditorApplication.update -= GenerateEditor;
#endif
        }

        public void Generate()
        {
            Reset();
            SnapSmoothPathToTerrain();
            CalculateLength();
            PlaceObjects();
            DeformMeshes();
            UpdateColliderBoundingBox();
            SnapObjectsToTerrain();
        }

        public void GenerateEditor()
        {
            if (updateRealTime)
            {
                Reset();
                SnapSmoothPathToTerrain();
                CalculateLength();
                PlaceObjects();
                DeformMeshes();
                UpdateColliderBoundingBox();
                SnapObjectsToTerrain();
            }
        }
        
        void ProceduralObjectsTransformUpdate()
        {
            if (m_proceduralObjects.Count > 0)
            {
                UpdateObjectsTransform(parameters);
            }
        }

        //general function to update all transforms of certain type of P. object
        void UpdateObjectsTransform(ProceduralObjectBaseParameters settings)
        {

            foreach (var p in m_proceduralObjects)
            {
                p.UpdateTransform(settings);
            }
        }

        void AddProceduralObject(ProceduralObject obj)
        {
            if (tag == null || obj == null) return;

            m_proceduralObjects.Add(obj);
        }
        
        void AddProceduralObjectComponent(int seed, string tag, Orientation orientation, ref GameObject obj)
        {
            ProceduralObject component = obj.AddComponent(typeof(ProceduralObject)) as ProceduralObject;
            component.InitObject(this.gameObject, seed, tag, orientation);
        }

        public void Reset(bool immediate = true)
        {
            ResetProceduralObjects();
            DeleteChildren(this.gameObject, immediate);
        }

        public void DeleteChildren(GameObject root, bool immediate = true)
        {
            List<GameObject> obj = new List<GameObject>();
            foreach (Transform child in root.transform)
            {
                obj.Add(child.gameObject);
            }

            foreach (var o in obj)
            {
                if(immediate)
                    DestroyImmediate(o.gameObject);
                else
                    Destroy(o.gameObject);
            }
        }

        public void ResetProceduralObjects()
        {
            foreach (var p in m_proceduralObjects)
            {
                if (p.regenerateFlag)
                {
                    DestroyImmediate(p.obj);
                }
            }

            m_proceduralObjects.Clear();
        }

        
        public void CalculateLength()
        {
            //distance between max and min vertex
            float vertexLength = MeshHelpers.GetMeshLength(deformationAxis, baseMesh, true);
            
            m_curveLength = path.PathLength;
            m_iterations = Mathf.FloorToInt(m_curveLength / vertexLength);
            m_lengthPerUnit = m_curveLength / (float) m_iterations;
        }

        void SnapSmoothPathToTerrain()
        {
            if (snapSmoothPathToTerrain)
            {
                CurveUtil.SnapSmoothPathToTerrains(path, terrainLayer, transform);
            }
        }

        void SnapObjectsToTerrain()
        {
            if (snapObjectsToTerrain)
            {
                foreach (var o in m_objects)
                {
                    o.transform.position = MeshHelpers.SnapObjectToTerrain(terrainLayer, o.transform.position);
                }
            }
        }

        public void PlaceObjects()
        {
            m_objects.Clear();
            for (int i = 0; i < m_iterations; i++)
            {
                GameObject spawnMesh = baseMesh;

                //start
                if (i == 0 && startMesh) { spawnMesh = startMesh; }

                //end
                if (i == m_iterations - 1 && endMesh) { spawnMesh = endMesh; }

                float positionAlongPath = i * m_lengthPerUnit;
                Vector3 positionStart =
                    path.EvaluatePositionAtUnit(positionAlongPath, CinemachinePathBase.PositionUnits.Distance);
                Vector3 positionCenter = path.EvaluatePositionAtUnit(positionAlongPath + (m_lengthPerUnit / 2.0f),
                    CinemachinePathBase.PositionUnits.Distance);
                Vector3 positionEnd = path.EvaluatePositionAtUnit(positionAlongPath + m_lengthPerUnit,
                    CinemachinePathBase.PositionUnits.Distance);

                Vector3 position =
                    path.EvaluatePositionAtUnit(positionAlongPath, CinemachinePathBase.PositionUnits.Distance);
                Quaternion rotation =
                    path.EvaluateOrientationAtUnit(positionAlongPath, CinemachinePathBase.PositionUnits.Distance);

                GameObject obj = null;
// #if UNITY_EDITOR
                // obj = PrefabUtility.InstantiatePrefab(spawnMesh) as GameObject;
                // obj.transform.SetPositionAndRotation(position, rotation);
// #else
                obj = Instantiate(spawnMesh, Vector3.zero, quaternion.identity) as GameObject;
// #endif
                obj.transform.parent = this.transform;
                
                //orientation
                Quaternion rotationUniform = CurveUtil.SampleRotationUniform(path, (float) i / (float) m_iterations * path.PathLength);
                
                //directions (uniform)
                Vector3 fwdUniform = rotationUniform * Vector3.forward;
                Vector3 upUniform = new Vector3(0, 1, 0);
                Vector3 normalUniform = Vector3.Cross(fwdUniform, Vector3.up);
                upUniform = Vector3.Cross(normalUniform, fwdUniform);

                //orientations
                fwdUniform = Vector3.Normalize(fwdUniform);
                upUniform = Vector3.Normalize(upUniform);
                normalUniform = Vector3.Normalize(normalUniform);
                Orientation orientationUniform = new Orientation(fwdUniform, upUniform, normalUniform);
                
                AddProceduralObjectComponent(i, "Procedural Object", orientationUniform, ref obj);
                AddProceduralObject(obj.GetComponent<ProceduralObject>());
                m_objects.Add(obj);

            }
        }

        public void DeformMeshes()
        {
            for (int i = 0; i < m_objects.Count; i++)
            {
                MeshHelpers.DeformMeshAlongCurve(deformationAxis,path, i, m_objects[i], m_lengthPerUnit);
            }
        }
        

        void UpdateColliderBoundingBox()
        {
            foreach (var o in m_objects)
            {
                //adjust the bounding box of the box collider
                if (o.TryGetComponent(out BoxCollider col))
                {
                    var newbound = o.GetComponent<MeshFilter>().sharedMesh.bounds;
                    col.size = newbound.size;
                    col.center = newbound.center;
                }
            }
        }



        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            CalculateLength();

            for (int i = 0; i < m_iterations; i++)
            {
                float positionAlongPath = i * m_lengthPerUnit;

                Vector3 positionStart =
                    path.EvaluatePositionAtUnit(positionAlongPath, CinemachinePathBase.PositionUnits.Distance);
                Vector3 positionEnd = path.EvaluatePositionAtUnit(positionAlongPath + m_lengthPerUnit,
                    CinemachinePathBase.PositionUnits.Distance);

                //position start
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(positionStart, 0.15f);
            }

        }
    }
}