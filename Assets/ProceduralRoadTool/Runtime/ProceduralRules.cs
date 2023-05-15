using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoadTool
{
    [System.Serializable]
    public enum RuleType
    {
        SpawnAlongCurve,
        SpawnAtCurvature,
        CurveSpawnAtOptimalPosition,
        CurveRandomSpawn,
        GenerateMeshAlongCurve,
        SpawnAtPosition,
    }
    
    [System.Serializable]
    public class SpawnParams
    {
        public int index;
        public Vector3 position;
        public Quaternion rotation;
        public Orientation orientation;

        public SpawnParams(int index, Vector3 position, Quaternion rotation, Orientation orientation)
        {
            this.index = index;
            this.position = position;
            this.rotation = rotation;
            this.orientation = orientation;
        }
    }
}