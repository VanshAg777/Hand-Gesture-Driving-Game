using com.unity.testtrack.terrainsystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProceduralRoadTool
{

    [CustomEditor(typeof(RoadTool))]
    public class RoadToolEditor : Editor
    {
        private ScatteringRulesBridge[] m_terrainBridges;
        private void OnEnable()
		{
            m_terrainBridges = GameObject.FindObjectsOfType<ScatteringRulesBridge>();
        }
		public override void OnInspectorGUI()
        {
            RoadTool targetObj = ((RoadTool)target);


            if (GUILayout.Button("Re-Generate"))
            {
                EditorUtility.DisplayProgressBar("Generating the road", "Re-generating the road", 0);
                targetObj.ReGenerate();
                EditorUtility.ClearProgressBar();
            }

            if (GUILayout.Button("Reset"))
            {
                targetObj.Reset();
            }

            UpdateTerrainGUI();

            base.OnInspectorGUI();
        }

        void UpdateTerrainGUI()
		{
            if (m_terrainBridges != null && m_terrainBridges.Length > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Update Exclusions"))
                {
                    //Look for any terrain component
                    int i = 0;
                    foreach (var bridge in m_terrainBridges.Where(bridge => bridge != null && bridge.GetComponent<Terrain>() != null))
                    {
                        EditorUtility.DisplayProgressBar("Update Exclusions...", "Updating the terrain exclusion zones", i / m_terrainBridges.Length);

                        bridge.OnEnterToolMode(bridge.GetComponent<Terrain>());
                        bridge.UpdateExclusionZones();
                        bridge.EditorUpdate();
                        bridge.OnExitToolMode();
                        i++;
                    }
                    EditorUtility.ClearProgressBar();
                }
                if (GUILayout.Button("Update Splatmaps"))
                {
                    //Look for any terrain component
                    int i = 0;
                    foreach (var bridge in m_terrainBridges.Where(bridge => bridge != null && bridge.GetComponent<Terrain>() != null))
                    {
                        EditorUtility.DisplayProgressBar("Update Splatmaps...", "Updating the terrain splat maps", i / m_terrainBridges.Length);
                        bridge.OnEnterToolMode(bridge.GetComponent<Terrain>());
                        bridge.UpdateSplatmaps();
                        bridge.EditorUpdate();
                        bridge.OnExitToolMode();
                        i++;
                    }
                    EditorUtility.ClearProgressBar();
                }
                GUILayout.EndHorizontal();
            }
        }
        
    }

}
