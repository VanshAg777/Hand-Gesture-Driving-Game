using Cinemachine;
using com.unity.testtrack.terrainsystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using System;
using System.Linq;
using System.Numerics;
using com.unity.testtrack.physics;
// using UnityEditor.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralRoadTool
{

	[RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadTool : MonoBehaviour
    {
        [Header("DEBUGGING")] 
        public bool showGizmos;
        
        [Header("Cinemachine Smooth Path")] 
        public CinemachineSmoothPath smoothPath;
        public bool snapSmoothPathToTerrain;

        [Header("Procedural Mesh Generation")] 
        public Shape shape;
        public int roadSegments = 60;
        public float roadWidth = 2.0f;
        protected ProceduralMesh m_proceduralMesh;


        [Header("Templating Parameters")] 
        public LayerMask terrainLayer;
        public RoadTemplate roadTemplate;
        public List<ProceduralObjectTemplate> templates;


        protected bool m_saveMeshToPrefab = false;
        protected Dictionary<string, List<ProceduralObject>> m_proceduralObjects = new Dictionary<string, List<ProceduralObject>>();
        protected Dictionary<string, ProceduralCurve> m_proceduralCurves = new Dictionary<string, ProceduralCurve>();


        private void Start()
        {
            if(!m_saveMeshToPrefab) GenerateProceduralMesh();
        }

        void OnValidate()
        {

            if (roadTemplate)
                templates = roadTemplate.templates;

            ProceduralObjectsTransformUpdate();

        }
        
        #region Cinemachine Smooth Path

        void SnapSmoothPathToTerrain()
        {
            if (snapSmoothPathToTerrain)
            {
                int len = smoothPath.m_Waypoints.Length;
                CinemachineSmoothPath.Waypoint[] waypoints = (CinemachineSmoothPath.Waypoint[]) smoothPath.m_Waypoints.Clone();
                smoothPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[len];

                for (int i = 0; i < len; i++)
                {
                    smoothPath.m_Waypoints[i] = waypoints[i];

                    Vector3 localToWorld = MeshHelpers.TransformLocalToWorld(smoothPath.m_Waypoints[i].position, this.transform);
                    Vector3 snappedPosition = MeshHelpers.SnapObjectToTerrain(terrainLayer, localToWorld);
                    Vector3 worldToLocal = MeshHelpers.TransformWorldToLocal(snappedPosition, this.transform);

                    smoothPath.m_Waypoints[i].position = worldToLocal;
                }
                
                smoothPath.InvalidateDistanceCache();
            }
        }
        
        #endregion
        
        
        #region Procedural Generation

        void GenerateProceduralMesh()
        {
         
            m_proceduralMesh = new ProceduralMesh(shape, roadSegments, roadWidth, smoothPath, this.transform);
            m_proceduralMesh.Generate();
            this.GetComponent<MeshFilter>().mesh = m_proceduralMesh.mesh;
        }

        void GenerateFromTemplates()
        {
            var templates = roadTemplate ? roadTemplate.templates : this.templates;
            foreach (var template in templates)
            {
                if (!template.settings.regenerate) continue;

                string objectName = template.name;
                string curveName = template.name + "Curve";

                //creating curve
                CreateUniformCurvesFromCenter(curveName, template.settings.spacingDistance, 0.1f, template.settings.distanceFromCenter);

                //instantiate procedural objects
                InstantiateProceduralObject(objectName, m_proceduralCurves[curveName], template);
            }
            
            ProceduralObjectsTransformUpdate();
        }
        
        #endregion



        #region Curve Generation
        
        void CreateUniformCenterCurve(string tag, int sampleCount)
        {
            //create curve
            ProceduralCurve curve = new ProceduralCurve(tag);
            m_proceduralCurves.Add(tag, curve);
            
            
            List<CurveControlPoint> controlPoints = new List<CurveControlPoint>();

            //sample at each sample count
            for (int i = 0; i < sampleCount; i++)
            {
                float pathLen = smoothPath.PathLength;

                //sample position, tangent, and roation
                Vector3 centerUniform = CurveUtil.SamplePositionUniform(smoothPath, (float) i / (float) sampleCount * pathLen);
                Vector3 tangentUniform = CurveUtil.SampleTangentUniform(smoothPath, (float) i / (float) sampleCount * pathLen);
                Quaternion rotationUniform = CurveUtil.SampleRotationUniform(smoothPath, (float) i / (float) sampleCount * pathLen);
                
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


                //add control point
                CurveControlPoint controlPoint = new CurveControlPoint( centerUniform, rotationUniform, orientationUniform );
                controlPoints.Add(controlPoint);

            }

            //assign to curve
            curve.meshTransform = this.transform;
            curve.controlPoints = controlPoints;
        }
        

        ProceduralCurve CreateProceduralCurve(string tag)
        {
            GameObject obj = new GameObject(tag);
            ProceduralCurve curve = obj.AddComponent<ProceduralCurve>();
            CinemachineSmoothPath path = obj.AddComponent<CinemachineSmoothPath>();
            curve.path = path;
            m_proceduralCurves.Add(tag, curve);

            ParentProceduralObject(tag, obj);
            return curve;
        }
        
        void CreateUniformCurvesFromCenter(string tag, float minimumDistance, float precisionFac, float distanceFromCenter)
        {

            ProceduralCurve curve = CreateProceduralCurve(tag);
            
            List<CurveControlPoint> controlPoints = new List<CurveControlPoint>();
            int currControlPointIndex = 0;
            float pathLen = smoothPath.PathLength;
            
            for (float i = 0; i < pathLen; i+= precisionFac)
            {
                
                //curve samples ( world space)
                Vector3 centerUniform = CurveUtil.SamplePositionUniform(smoothPath, i);
                Quaternion rotationUniform = CurveUtil.SampleRotationUniform(smoothPath, i);

                //directions (uniform)
                Vector3 fwdUniform = rotationUniform * Vector3.forward;
                Vector3 upUniform = new Vector3(0, 1, 0);
                Vector3 normalUniform = Vector3.Cross(fwdUniform, Vector3.up);
                upUniform = Vector3.Cross(normalUniform, fwdUniform);

                fwdUniform = Vector3.Normalize(fwdUniform);
                upUniform = Vector3.Normalize(upUniform);
                normalUniform = Vector3.Normalize(normalUniform);
                
                //directions from quaternion
                Vector3 rotForward, rotUp, rotSide;
                rotForward = rotationUniform * Vector3.forward;
                rotUp = rotationUniform * Vector3.up;
                rotSide = rotationUniform * Vector3.left;
                Orientation orientationUniform = new Orientation(rotForward, rotUp, rotSide);

                //mult by road width
                Vector3 projectedPoint = new Vector3();
                projectedPoint = centerUniform + rotSide * (roadWidth * distanceFromCenter);

                //check MinimumDistance
                if (currControlPointIndex == 0)
                {
                    //create new control point 
                    CurveControlPoint point = new CurveControlPoint(projectedPoint, rotationUniform, orientationUniform);
                    
                    //increment current index
                    currControlPointIndex++;
                    
                    //push to list
                    controlPoints.Add(point);
                }
                else if (Vector3.Distance(projectedPoint, controlPoints[currControlPointIndex - 1].worldPosition) > minimumDistance)
                {
                    //create new control point 
                    CurveControlPoint point = new CurveControlPoint(projectedPoint, rotationUniform, orientationUniform);
                    
                    //increment current index
                    currControlPointIndex++;
                    
                    //push to list
                    controlPoints.Add(point);
                }
            }
            
            //assign control points to curve
            curve.controlPoints = controlPoints;
            curve.CreatePathFromControlPoints(smoothPath.Looped);
        }



        void ResetProceduralCurves(Dictionary<string, ProceduralCurve> proceduralCurves)
        {
            foreach (var p in proceduralCurves.Values)
            {
                DestroyImmediate(p.gameObject);
            }
            proceduralCurves.Clear();
        }
        
        #endregion
        
        #region Object generation

        void AddProceduralObjectComponent(int seed, string tag, Orientation orientation, ref GameObject obj)
        {
            ProceduralObject component = obj.AddComponent(typeof(ProceduralObject)) as ProceduralObject;
            component.InitObject(this.gameObject, seed, tag, orientation);
        }

        void ParentProceduralObject(string tag, GameObject obj)
        {
            var parent = this.transform.Find(tag);
            if (parent)
            {
                obj.transform.parent = parent;
            }
            else
            {
                GameObject empty = new GameObject(tag);
                empty.transform.parent = this.transform;
                obj.transform.parent = empty.transform;
            }
        }
        
        void SpawnProceduralObjects(string tag, GameObject[] prefabs, List<SpawnParams> spawnParams, ProceduralObjectTemplate template, ProceduralCurve curve)
        {
            if (prefabs.Length == 0) return;

            foreach (var spawn in spawnParams)
            {
                
                int selection = Random.Range(0, prefabs.Length);
                
                Vector3 position = template.settings.snapToTerrain ? MeshHelpers.SnapObjectToTerrain(terrainLayer.value, spawn.position) : spawn.position;
                Quaternion rotation = template.settings.orientWithCurve ? spawn.rotation : Quaternion.identity;
                position = template.settings.deformWithCurve ? Vector3.zero : position;
                
                GameObject proceduralObject = null;
#if UNITY_EDITOR
                proceduralObject = PrefabUtility.InstantiatePrefab(prefabs[selection]) as GameObject;
                proceduralObject.transform.SetPositionAndRotation(position, rotation);
#else
                proceduralObject = Instantiate(prefabs[selection], position, rotation) as GameObject;
#endif
                // proceduralObject.transform.parent = this.transform;
                DeformProceduralObject(template, curve, proceduralObject, spawn.index);
                ParentProceduralObject(template.name, proceduralObject);
                AddProceduralObjectComponent(spawnParams.IndexOf(spawn), tag, spawn.orientation, ref proceduralObject);
                AddProceduralObject(tag, proceduralObject.GetComponent<ProceduralObject>());
            
            }

        }

        #region Deformation

        void DeformProceduralObject(ProceduralObjectTemplate template, ProceduralCurve curve, GameObject obj, int index)
        {
            var settings = template.settings;
            if (settings.deformWithCurve)
            {
                float meshLength = MeshHelpers.GetMeshLength(settings.deformAxis, obj, true);
                MeshHelpers.DeformMeshAlongCurve(settings.deformAxis, curve.path, index, obj, meshLength);
            }
        }
    
        #endregion


        void InstantiateProceduralObject(string tag, ProceduralCurve curve, ProceduralObjectTemplate template)
        {
            switch (template.settings.ruleType)
            {
                case RuleType.SpawnAlongCurve:
                    SpawnWithOffset(tag, curve, template);
                    break;
                case RuleType.CurveSpawnAtOptimalPosition:
                    SpawnAtOptimalPositionAlongCurvature(tag, curve, template);
                    break;
                case RuleType.SpawnAtCurvature:
                    SpawnAtCurvature(tag, curve, template);
                    break;
                case RuleType.CurveRandomSpawn:
                    SpawnRandomAlongCurve(tag, curve, template);
                    break;
                case RuleType.GenerateMeshAlongCurve:
                    GenerateMeshAlongCurve(tag, curve, template);
                    break;
                case RuleType.SpawnAtPosition:
                    SpawnAtPosition(tag, curve, template);
                    break;
            }
        }

#endregion
        
#region Spawning Rules
        
        //Spawning rules
        void SpawnAlongCurve(string tag, ProceduralCurve curve, ProceduralObjectTemplate template) 
        { 
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            for (int i = 0; i < points.Count; i++)
            {
                SpawnParams p = new SpawnParams(i, points[i].worldPosition, points[i].rotation, points[i].orientation);
                spawnParams.Add(p);
            }
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }
        
        void SpawnAtCurvature(string tag, ProceduralCurve curve, ProceduralObjectTemplate template) 
        { 
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            int pointCount = points.Count;
            int startPointPosition = Mathf.FloorToInt(template.settings.startOffsetPosition * (float)pointCount);
            int endPointPosition = Mathf.FloorToInt(template.settings.endOffsetPosition * (float)pointCount);

            for (int i = startPointPosition; i < endPointPosition; i++)
            {
                if (points[i].curveCoeff > template.settings.curveSpawnThreshold)
                {
                    SpawnParams p = new SpawnParams(i, points[i].worldPosition, points[i].rotation, points[i].orientation);
                    spawnParams.Add(p);
                }
            }
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }
        
        void SpawnAtOptimalPositionAlongCurvature(string tag, ProceduralCurve curve, ProceduralObjectTemplate template) 
        { 
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            int pointCount = points.Count;
            int startPointPosition = Mathf.FloorToInt(template.settings.startOffsetPosition * (float)pointCount);
            int endPointPosition = Mathf.FloorToInt(template.settings.endOffsetPosition * (float)pointCount);
            var sequence = new List<CurveControlPoint>();
            for (int i = startPointPosition; i < endPointPosition; i++)
            {
                
                //logic spawn a poll above curve threshold
                if (points[i].angleDiff < template.settings.angleDiffThreshold)
                {
                    sequence.Add(points[i]);
                }
                else if (sequence.Count > 0)
                {
                    SpawnParams p = new SpawnParams(i, points[i].worldPosition, points[i].rotation, points[i].orientation);
                    spawnParams.Add(p);
                    sequence.Clear();
                }
            }
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }
        
        void SpawnRandomAlongCurve(string tag, ProceduralCurve curve, ProceduralObjectTemplate template) 
        { 
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            int pointCount = points.Count;
            int startPointPosition = Mathf.FloorToInt(template.settings.startOffsetPosition * (float)pointCount);
            int endPointPosition = Mathf.FloorToInt(template.settings.endOffsetPosition * (float)pointCount);
            for (int i = startPointPosition; i < endPointPosition; i++)
            {
                float r = Random.value;
                if (r >= template.settings.randomThreshold)
                {
                    SpawnParams p = new SpawnParams(i, points[i].worldPosition, points[i].rotation, points[i].orientation);
                    spawnParams.Add(p);
                }
            }
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }

        void SpawnAtPosition(string tag, ProceduralCurve curve, ProceduralObjectTemplate template)
        {
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            int pointCount = points.Count;
            float positionAlongCurve = template.settings.spawnPosition;
            var selectedPoint = points[Mathf.FloorToInt(positionAlongCurve * (float) (pointCount - 1))];


            SpawnParams p = new SpawnParams(0, selectedPoint.worldPosition, selectedPoint.rotation, selectedPoint.orientation);
            spawnParams.Add(p);
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }

        void SpawnWithOffset(string tag, ProceduralCurve curve, ProceduralObjectTemplate template)
        {
            //calculate amount
            
            List<SpawnParams> spawnParams = new List<SpawnParams>();
            var points = curve.controlPoints;
            int pointCount = points.Count;
            int startPointPosition = Mathf.FloorToInt(template.settings.startOffsetPosition * (float) pointCount);
            int endPointPosition = Mathf.FloorToInt(template.settings.endOffsetPosition * (float)pointCount);

            for (int i = startPointPosition; i < endPointPosition; i++)
            {
                SpawnParams p = new SpawnParams(i, points[i].worldPosition, points[i].rotation, points[i].orientation);
                spawnParams.Add(p);
            }
                
            SpawnProceduralObjects(tag, template.prefabs, spawnParams, template, curve);
        }

        private Shader GetDefautlShader()
        {
            var rpa = GetCurrentRenderPipeline();
            if (rpa != null && rpa.GetType().ToString().Contains("HighDefinition"))
                return Shader.Find("HDRP/Lit");
            else if (rpa != null)
                return Shader.Find("Universal Render Pipeline/Lit");
            else
                return Shader.Find("Standard");
        }

        public RenderPipelineAsset GetCurrentRenderPipeline()
        {
            return QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel()) is RenderPipelineAsset qualityAsset ? qualityAsset : GraphicsSettings.renderPipelineAsset;
        }

        void GenerateMeshAlongCurve(string tag, ProceduralCurve curve, ProceduralObjectTemplate template)
        {

            //Validate the parameters
            if (template.settings.shape == null)
            {
                Debug.LogError("Error: No shap provided to curve " + template.name + ", please provide a shape to generate mesh");
                return;
            }

            //Create a new GameObject for our mesh
            GameObject proceduralObject = new GameObject();
            proceduralObject.transform.SetParent(this.transform, false);
            proceduralObject.name = template.name;

            //Add the procedural components
            AddProceduralObjectComponent(0, tag, new Orientation(proceduralObject.transform.forward, proceduralObject.transform.up, proceduralObject.transform.right), ref proceduralObject);
            AddProceduralObject(tag, proceduralObject.GetComponent<ProceduralObject>());

            var points = curve.controlPoints;
            int pointCount = points.Count;
            int startPointPosition = Mathf.FloorToInt(template.settings.startOffsetPosition * (float)pointCount);
            int endPointPosition = Mathf.FloorToInt(template.settings.endOffsetPosition * (float)pointCount);
            pointCount = endPointPosition - startPointPosition;

            //Convert the curve to a spline
            var local_smootPath = proceduralObject.AddComponent<CinemachineSmoothPath>();
            local_smootPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[pointCount];
            int currentIdex = 0;
            for (int i = startPointPosition; i < endPointPosition; i++, currentIdex++)
                local_smootPath.m_Waypoints[currentIdex].position = curve.controlPoints[i].worldPosition - this.transform.position;
            local_smootPath.m_Looped = smoothPath.Looped;
            local_smootPath.InvalidateDistanceCache();

            //Generate the mesh
            var myMesh = new ProceduralMesh(template.settings.shape, 
                template.settings.useRoadSettings ? roadSegments : template.settings.roadSegments,
                template.settings.useRoadSettings ? roadWidth : template.settings.roadWidth, 
                local_smootPath, 
                proceduralObject.transform);
            myMesh.Generate();

            //We have a dynamic mesh, so mark it as such
            proceduralObject.GetComponent<ProceduralObject>().hasProceduralMesh = true;

            //Add the mesh to the object
            proceduralObject.AddComponent<MeshFilter>().mesh = myMesh.mesh;
            proceduralObject.AddComponent<MeshRenderer>().sharedMaterial = template.settings.material == null ? new Material(GetDefautlShader()) : template.settings.material;

            if (template.settings.splatDefinition != null && template.settings.splatMaterial != null)
            {
                //Add the splatmap painter object
                var painter = proceduralObject.AddComponent<SplatmapPainter>();
                painter.m_splatDefinition = template.settings.splatDefinition;
                painter.m_material = template.settings.splatMaterial;
            }

            if (template.settings.ExclusionZoneMaterial != null)
            {
                var zone = proceduralObject.AddComponent<MeshExclusionZoneImpl>();
                zone.m_material = template.settings.ExclusionZoneMaterial;
                zone.m_resolution = template.settings.ExclusionZoneresolution;
                zone.m_includeSubRules = template.settings.ExclusionZoneIncludeSubRules;
                zone.m_filters = template.settings.ExclusionZoneFilters;
            }

            if (template.settings.physicalProperties.Count > 0)
			{
                var physicalExt = proceduralObject.AddComponent<PhysicalMaterialExtension>();
                physicalExt._physicalProperties = template.settings.physicalProperties;
            }

            if (template.settings.physicMaterial != null)
            {
                var collider = proceduralObject.GetComponent<Collider>();
                if (collider == null)
                    collider = proceduralObject.AddComponent<MeshCollider>();
                collider.sharedMaterial = template.settings.physicMaterial;
            }

            if (proceduralObject.GetComponent<Collider>() == null) //If no collider disable the object else disable only the renderer
                proceduralObject.SetActive(!template.settings.isHidden);
            else
                proceduralObject.GetComponent<MeshRenderer>().enabled = !template.settings.isHidden; 
        }

#endregion

#region Procedural Objects

        void ProceduralObjectsTransformUpdate()
        {
            if (m_proceduralObjects.Count > 0)
            {
                var templates = roadTemplate ? roadTemplate.templates : this.templates;
                foreach (var template in templates)
                {
                    UpdateObjectsTransform(template.name, template.settings);
                }
            }
        }

        //general function to update all transforms of certain type of P. object
        void UpdateObjectsTransform(string tag, ProceduralObjectRoadParameters settings)
        {
            if (m_proceduralObjects.ContainsKey(tag))
            {
                foreach (var p in m_proceduralObjects[tag])
                {
                    p.UpdateTransform(settings);
                }
            }
        }
        
        void AddProceduralCurve(string tag, ProceduralCurve obj)
        {
            if (tag == null || obj == null) return;            
            
            //if it doesnt exist yet, then add that lane
            if (!m_proceduralCurves.ContainsKey(tag))
            {
                m_proceduralCurves.Add(tag, obj);
            }

            //add object to that specific late
            m_proceduralCurves[tag] = obj;
        }
        
        void AddProceduralObject(string tag, ProceduralObject obj)
        {
            if (tag == null || obj == null) return;            
            
            //if it doesnt exist yet, then add that lane
            if (!m_proceduralObjects.ContainsKey(tag))
            {
                m_proceduralObjects.Add(tag, new List<ProceduralObject>());
            }

            //add object to that specific late
            m_proceduralObjects[tag].Add(obj);
        }

        void RemoveProceduralObject(ProceduralObject obj)
        {
            if (obj == null) return;
            
            if (!m_proceduralObjects.ContainsKey(obj.tag))
            {
                Debug.LogWarning("Procedural Object cant be removed because there is no corresponding list for its tag!");
            }
            else
            {
                m_proceduralObjects[obj.tag].Remove(obj);
            }
        }

        public void ResetProceduralObjects(Dictionary<string,List<ProceduralObject>> objects)
        {

            foreach (var list in objects.Values)
            {
                foreach (var p in list)
                {
                    if (p.regenerateFlag)
                        DestroyImmediate(p.obj);
                }
            }

            objects.Clear();
        }

        public void DeleteChildren(GameObject root)
        {
            List<GameObject> obj = new List<GameObject>();
            foreach (Transform child in root.transform)
            {
                obj.Add(child.gameObject);
            }

            foreach (var o in obj)
            {
                DestroyImmediate(o.gameObject);
            }

        }


        void DebugProceduralObjectsNames(GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (child.childCount > 0)
                {
                    foreach (Transform baby in child.transform)
                    {
                        Debug.Log($"From : {child.gameObject.name} >>> {baby.gameObject.name}");
                        
                    }
                }
                Debug.Log($"{child.gameObject.name}");
            }
        }
        
        void DebugProceduralObjectsParam(GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (child.childCount > 0)
                {
                    foreach (Transform baby in child.transform)
                    {
                        if (baby.gameObject.TryGetComponent(out ProceduralObject component))
                        {
                            Debug.Log($"From : {child.gameObject.name} >>> {baby.gameObject.name} >> {component.regenerateFlag}");
                        }
                    }
                }
                Debug.Log($"{child.gameObject.name}");
            }
        }

        void DebugProceduralObjects()
        {
            foreach (var key in m_proceduralObjects.Keys)
            {
                Debug.Log($"Lane {key} has {m_proceduralObjects[key].Count} procedural objects");
            }
        }
        
#endregion

#region Editor Functions

        public void ReGenerate()
        {
            Reset();

            CheckChildProceduralCurves(this.gameObject);
            CheckChildProceduralObjects(this.gameObject);
            SnapSmoothPathToTerrain();
            GenerateProceduralMesh();
            GenerateFromTemplates();
            ProceduralObjectsTransformUpdate();

#if UNITY_EDITOR
            var prefabType = PrefabUtility.GetPrefabAssetType(this.gameObject);
            var editorstage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (editorstage != null) //Are we in prefab edit mode
            {
                var assetPath = editorstage != null ? editorstage.assetPath : AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject));

                blastAttachedObjects(assetPath);
                AttachedObjects(assetPath);

                if (prefabType == PrefabAssetType.Regular && PrefabUtility.IsPartOfPrefabInstance(this.gameObject))
                    PrefabUtility.ApplyPrefabInstance(this.gameObject, InteractionMode.AutomatedAction);
            }
            else if (prefabType == PrefabAssetType.Regular)
            {
                var assetPath = editorstage != null ? editorstage.assetPath : AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(this.gameObject));

                blastAttachedObjects(assetPath);
                AttachedObjects(assetPath);

                if (prefabType == PrefabAssetType.Regular && PrefabUtility.IsPartOfPrefabInstance(this.gameObject))
                {
                    PrefabUtility.ApplyPrefabInstance(this.gameObject, InteractionMode.AutomatedAction);
                }
            }

            EditorUtility.SetDirty(this.gameObject);
#endif
        }

        public void Reset()
        {
            var originalRoot = this.gameObject;
            var currentRoot = this.gameObject;

#if UNITY_EDITOR
            bool unpacked = false;
			string assetPath = null;
			var prefabType = PrefabUtility.GetPrefabAssetType(currentRoot);
			var editorstage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (editorstage != null) //Are we in prefab edit mode
            {
                assetPath = editorstage.assetPath;
                currentRoot = editorstage.prefabContentsRoot;
                var t = currentRoot.GetComponent<RoadTool>();
                if (t != null)
                    t.m_proceduralCurves = m_proceduralCurves;
            }
            else if (prefabType == PrefabAssetType.Regular)
            {
                assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(currentRoot));
                PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                unpacked = true;
                
                var t = currentRoot.GetComponent<RoadTool>();
                if (t != null && false)
                {
                    t.m_proceduralCurves = m_proceduralCurves;
                    t.templates = templates;
                }
			}
#endif

            CheckChildProceduralCurves(currentRoot);
			CheckChildProceduralObjects(currentRoot);
            ResetProceduralCurves(m_proceduralCurves);
            ResetProceduralObjects(m_proceduralObjects);

#if UNITY_EDITOR
            if(true)
            {
                if (unpacked)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(currentRoot, assetPath, InteractionMode.UserAction);
                }
                if (editorstage != null)
                {
                    blastAttachedObjects(assetPath);
                    if (prefabType == PrefabAssetType.Regular)
                        PrefabUtility.SaveAsPrefabAsset(currentRoot, assetPath);
                
                }
            }
#endif
            
        }

#if UNITY_EDITOR
        private void blastAttachedObjects(string path)
        {
            var pObjects = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
            // Delete any old sub objects inside a main asset.
            UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object obj in assets)
            {
                bool regenerateFlag = true;
                if (obj is GameObject)
                {
                    var g = (GameObject) obj;
                    if (g.TryGetComponent(out ProceduralObject component))
                    {
                        regenerateFlag = component.regenerateFlag;
                    }
                }
                if (obj == null || (!UnityEditor.AssetDatabase.IsMainAsset(obj) && !(obj is GameObject) &&
                                    !(obj is Component) && regenerateFlag))
                {
                    UnityEngine.Object.DestroyImmediate(obj, true);
                }
            }
        }

        private void AttachedObjects(string path)
        {
            if (m_saveMeshToPrefab)
            {
                AssetDatabase.AddObjectToAsset(this.GetComponent<MeshFilter>().sharedMesh, path);

                //Update our collider with the right mesh
                if (this.GetComponent<MeshCollider>() != null)
                    this.GetComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().sharedMesh;

                foreach (var list in m_proceduralObjects.Values)
                {
                    foreach (var p in list.Where(p => p.hasProceduralMesh))
                    {
                        var mf = p.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null)
                            AssetDatabase.AddObjectToAsset(p.GetComponent<MeshFilter>().sharedMesh, path);
                    }
                }
            }
        }
#endif

        public void CheckChildProceduralCurves(GameObject root)
        {
            var objects = root.GetComponentsInChildren<ProceduralCurve>(true);
            m_proceduralCurves.Clear();
            m_proceduralCurves = new Dictionary<string, ProceduralCurve>();

            foreach (var child in objects)
            {
                AddProceduralCurve(child.name, child);
            }
        }
        
        public void CheckChildProceduralObjects(GameObject root)
        {
            var objects = root.GetComponentsInChildren<ProceduralObject>(true);
            m_proceduralObjects.Clear();
            m_proceduralObjects = new Dictionary<string, List<ProceduralObject>>();

            foreach (var child in objects)
            {
                AddProceduralObject(child.tag, child);
            }
        }

#endregion

#region Gizmos
        private void OnDrawGizmos()
        {
            if (m_proceduralCurves.Count == 0 || !showGizmos)
                return;
            
            foreach (var p in m_proceduralCurves.Values)
            {
                p.DrawGizmos();
            }
        }

#endregion
    }
}
