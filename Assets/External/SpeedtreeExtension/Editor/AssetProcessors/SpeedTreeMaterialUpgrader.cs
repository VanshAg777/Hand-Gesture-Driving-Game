using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class SpeedTreeMaterialUpgrader : MaterialUpgrader
{
    const string k_OriginalSpeedTreeShaderName = "HDRP/Nature/SpeedTree8";
    const string k_UpgradedSpeedTreeShaderName = "HDRP/Nature/SpeedTree8_Opaque";

    public bool ShouldUpgradeMaterial(Material m)
    {
        return m.name.ToUpper().Contains("BARK") &&
            m.shader.name == k_OriginalSpeedTreeShaderName;
    }

    public SpeedTreeMaterialUpgrader()
    {
        RenameShader(k_OriginalSpeedTreeShaderName, k_UpgradedSpeedTreeShaderName);
    }
}

public class SpeedTreeMaterialPostProcessor : AssetPostprocessor
{
    SpeedTreeMaterialUpgrader m_Upgrader = new SpeedTreeMaterialUpgrader();

    public override int GetPostprocessOrder() => 1; // Ensure we run after the official speed tree post processor

    void OnPostprocessSpeedTree(GameObject speedtree)
    {
        LODGroup lg = speedtree.GetComponent<LODGroup>();
        LOD[] lods = lg.GetLODs();
        for (int l = 0; l < lods.Length; l++)
        {
            LOD lod = lods[l];
            foreach (var r in lod.renderers)
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m_Upgrader.ShouldUpgradeMaterial(m))
                    {
                        m_Upgrader.Upgrade(m, MaterialUpgrader.UpgradeFlags.None);
                    }
                }
            }
        }
    }
}