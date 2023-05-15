using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralRoadTool
{

    [System.Serializable]
    public class Vertex
    {
        public Vector2 point;
        public Vector2 uv;
    }
    
    [CreateAssetMenu(fileName = "Shape", menuName = "ScriptableObjects/Shapes", order = 1)]
    public class Shape : ScriptableObject
    {
        public Vertex[] vertices;
        public int vertexCount => vertices.Length;
    }

}


