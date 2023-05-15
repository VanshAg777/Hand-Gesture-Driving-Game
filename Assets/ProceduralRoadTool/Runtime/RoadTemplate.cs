using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoadTool
{
    [CreateAssetMenu(fileName = "RoadTemplate_NAME", menuName = "ScriptableObjects/RoadTemplate", order = 1)]
    public class RoadTemplate : ScriptableObject
    {
        public List<ProceduralObjectTemplate> templates;
    }

}
