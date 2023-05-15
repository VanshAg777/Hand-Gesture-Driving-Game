using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoadTool
{
    public static class RandomUtil
    {
        public static float RandomPerlin(Vector2 minMax)
        {
            return Mathf.PerlinNoise(minMax.x, minMax.y);
        }

        public static float RandomFloat(Vector2 minMax)
        {
            return Random.Range(minMax.x, minMax.y);
        }
        
        public static float RandomFloat(int seed, Vector2 minMax)
        {
            Random.InitState(seed);
            return Random.Range(minMax.x, minMax.y);
        }

        public static Vector3 RandVector3(int seed, Vector3 minMax)
        {
            float noiseX = RandomUtil.RandomFloat(seed, new Vector2(-minMax.x, minMax.x));
            float noiseY = RandomUtil.RandomFloat(seed + 38403, new Vector2(-minMax.y, minMax.y));
            float noiseZ = RandomUtil.RandomFloat(seed + 12893, new Vector2(-minMax.z, minMax.z));
            
            return new Vector3(noiseX,noiseY,noiseZ);
        }

        public static int RandomInt(Vector2 minMax)
        {
            return Mathf.FloorToInt(Random.Range(minMax.x, minMax.y));
        }
        
        public static int RandomInt(int seed, Vector2 minMax)
        {
            Random.InitState(seed);
            return Mathf.FloorToInt(Random.Range(minMax.x, minMax.y));
        }
    }
}