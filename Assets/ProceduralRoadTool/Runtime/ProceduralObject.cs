using com.unity.testtrack.physics;
using com.unity.testtrack.terrainsystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static com.unity.testtrack.terrainsystem.MeshExclusionZoneImpl;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace ProceduralRoadTool
{
    [System.Serializable]
    public class ProceduralObjectBaseParameters
    {
        [Header("Regenerate")]
        public bool regenerate = true;
        
        [Header("Placement Along Curve")] 
        public float distanceFromCenter = 1.0f;
        public float spacingDistance = 1.0f;
        
        [Header("Offsets")]
        public Vector3 positionOffset = Vector3.zero;
        public float localScale = 1;
        public Vector3 rotationOffset = Vector3.zero;

        [Header("Randomization")] 
        [Range(0, 100)] public int seed = 0;
        public Vector3 positionNoiseFac = Vector3.zero;
        public Vector2 scaleNoiseFac = Vector2.zero;
        public Vector3 rotationNoiseFac = Vector3.zero;
    }
    
	[System.Serializable]
    public class ProceduralObjectRoadParameters : ProceduralObjectBaseParameters
    {
        [Header("Toggles")] 
        public bool orientWithCurve = true;
        public bool snapToTerrain;
        public bool deformWithCurve;
        public Axis deformAxis = Axis.z;

        [Header("Spawning Rules Params")] 
        [Range(0,1)] public float startOffsetPosition = 0.0f;
        [Range(0,1)] public float endOffsetPosition = 1.0f;
        [Range(0,1)] public float spawnPosition = 0.5f;
        [Range(0,1)] public float randomThreshold = 0.5f;
        [Range(0,1)] public float curveSpawnThreshold = 0.5f;
        public float angleDiffThreshold = 0.5f;

        [Header("Mesh Generation Params")]
        public Material material;
        public Shape shape;
        public bool useRoadSettings = true;
        public int roadSegments = 60;
        public float roadWidth = 2.0f;
        public bool isHidden = false;
        public PhysicMaterial physicMaterial;
        public List<PhysicalProperty> physicalProperties = new List<PhysicalProperty>();

        [Header("Terrain splatting Params")]
        public AlphaSplatDefinition splatDefinition;
        public Material splatMaterial;

        [Header("Scattering exclusion Params")]
        public Resolutions ExclusionZoneresolution = Resolutions._128x128;
        public Material ExclusionZoneMaterial = null;
        public List<ScatteringRule> ExclusionZoneFilters = new List<ScatteringRule>();
        public bool ExclusionZoneIncludeSubRules = true;

        [Header("Curve Type")]
        public RuleType ruleType = RuleType.SpawnAlongCurve;
    }

    [System.Serializable]
    public class ProceduralObjectTemplate
    {
        public string name;
        public ProceduralObjectRoadParameters settings;
        
        [Header("Prefabs and Variations")]
        public GameObject[] prefabs;        
    }


    public class ProceduralObject : MonoBehaviour
    {
        public bool regenerateFlag = true;
        public GameObject generator; //Object that generated this procedural object
        public new string tag;
        public int seed;
        public GameObject obj;

        public int initSeed;
        public Vector3 initPosition;
        public Vector3 initLocalScale;
        public Quaternion initRotation;

        public Orientation orientation; //more for position randomization
        public bool hasProceduralMesh = false;

        public ProceduralObject(GameObject generator, int seed, string tag, GameObject obj, Orientation orientation)
        {
            this.generator = generator;
            this.initSeed = seed;
            this.seed = this.initSeed;
            this.tag = tag;
            this.obj = obj;
            this.initPosition = obj.transform.position;
            this.initLocalScale = obj.transform.localScale;
            this.initRotation = obj.transform.rotation;
            this.orientation = orientation;
            this.hasProceduralMesh = false;
        }

        public virtual void InitObject(GameObject generator, int seed, string tag, Orientation orientation)
        {
            this.generator = generator;
            this.initSeed = seed;
            this.seed = this.initSeed;
            this.tag = tag;
            this.obj = this.gameObject;
            this.initPosition = obj.transform.position;
            this.initLocalScale = obj.transform.localScale;
            this.initRotation = obj.transform.rotation;
            this.orientation = orientation;
        }
        
        public virtual void UpdateTransform(ProceduralObjectBaseParameters settings)
        {
            if (this.obj == null)
                return;

            UpdateFlags(settings);
            UpdateSeed(settings);
            UpdatePosition(settings);
            UpdateScale(settings);
            UpdateRotation(settings);
        }

        public virtual void UpdateFlags(ProceduralObjectBaseParameters settings)
        {
            this.regenerateFlag = settings.regenerate;
        }
        
        public virtual void UpdateSeed(ProceduralObjectBaseParameters settings)
        {
            this.seed = this.initSeed + settings.seed;
        }

        public virtual void UpdatePosition(ProceduralObjectBaseParameters settings)
        {
            Vector3 translation = settings.positionOffset;
            Vector3 noiseOffset = settings.positionNoiseFac;
            
            Vector3 translatedPos = initPosition;
            translatedPos += orientation.forward * translation.z;
            translatedPos += orientation.side * translation.x;
            translatedPos += orientation.up * translation.y;
            
            //adding noise
            float noiseX = RandomUtil.RandomFloat(seed, new Vector2(-noiseOffset.x, noiseOffset.x));
            float noiseY = RandomUtil.RandomFloat(seed + 87994, new Vector2(-noiseOffset.y, noiseOffset.y));
            float noiseZ = RandomUtil.RandomFloat(seed + 38497, new Vector2(-noiseOffset.z, noiseOffset.z));

            Vector3 finalPos = translatedPos;
            finalPos += orientation.forward * noiseZ;
            finalPos += orientation.side * noiseX;
            finalPos += orientation.up * noiseY;

            this.obj.transform.position = finalPos;
        }

        public virtual void UpdateScale(ProceduralObjectBaseParameters settings)
        {
            //adding noise
            float noise = RandomUtil.RandomFloat(seed, settings.scaleNoiseFac);
            this.obj.transform.localScale = initLocalScale * (settings.localScale + noise);
        }

        public virtual void UpdateRotation(ProceduralObjectBaseParameters settings)
        {
            Vector3 rotation = settings.rotationOffset;
            
            //adding noise
            float noiseX = RandomUtil.RandomFloat(seed, new Vector2(-settings.rotationNoiseFac.x, settings.rotationNoiseFac.x));
            float noiseY = RandomUtil.RandomFloat(seed + 29381, new Vector2(-settings.rotationNoiseFac.y, settings.rotationNoiseFac.y));
            float noiseZ = RandomUtil.RandomFloat(seed + 19280, new Vector2(-settings.rotationNoiseFac.z, settings.rotationNoiseFac.z));
            
            rotation += new Vector3(noiseX,noiseY,noiseZ);
            
            //Note : Quaternion vs quaternion are different according to unity api
            this.obj.transform.rotation = initRotation * Quaternion.Euler(rotation);
        }

    }
}