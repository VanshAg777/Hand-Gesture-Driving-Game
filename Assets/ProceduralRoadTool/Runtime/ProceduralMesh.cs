using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace ProceduralRoadTool
{
    [System.Serializable]
    public class ProceduralMesh
    {
        //general mesh params
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        //shape and path
        public Shape shape;
        protected int m_segments;
        protected float m_roadWidth;
        protected CinemachineSmoothPath m_smoothPath;
        protected Transform m_transform;

        public ProceduralMesh(Shape shape, int segments = 10, float roadWidth = 1.0f, CinemachineSmoothPath smoothPath = null, Transform transform = null)
        {
            this.shape = shape;
            this.m_segments = segments;
            this.m_roadWidth = roadWidth;
            this.m_smoothPath = smoothPath;
            this.m_transform = transform;
            
            CreateMesh();
            UpdateMesh();
        }
        
        public void CreateMesh()
        {
            mesh = new Mesh();
        }

        public void UpdateMesh()
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

        }

        public void Generate()
        {
            CreateVertices();
            CreateTriangles();
            CreateUVs();
            UpdateMesh();
        }

        public void CreateVertices()
        {
            if (shape == null)
                return;

            //vertices
            int vertexCount = (m_segments + 1) * shape.vertices.Length;
            vertices = new Vector3[vertexCount];

            int vert = 0;
            for (int z = 0; z <= m_segments; z++)
            {
                int waypointCount = m_smoothPath.m_Waypoints.Length;

                Vector3 centerRelative = m_smoothPath.EvaluatePosition((float) z / (float) m_segments * waypointCount);
                Quaternion rotationRelative = m_smoothPath.EvaluateOrientation((float) z / (float) m_segments * waypointCount);

                //directions (relative)
                Vector3 fwd = rotationRelative * Vector3.forward;
                Vector3 up = new Vector3(0, 1, 0);
                Vector3 normal = Vector3.Cross(fwd, Vector3.up);
                up = Vector3.Cross(normal, fwd);

                //normalized
                fwd = Vector3.Normalize(fwd);
                up = Vector3.Normalize(up);
                normal = Vector3.Normalize(normal);
                Orientation orientation = new Orientation(fwd, up, normal);

                //directions from quaternion
                Vector3 rotForward, rotUp, rotSide;
                rotForward = rotationRelative * Vector3.forward;
                rotUp = rotationRelative * Vector3.up;
                rotSide = rotationRelative * Vector3.left;

                
                //loop through each vertex of the shape
                for (int s = 0; s < shape.vertices.Length; s++)
                {
                    //apply rotation to shape                    
                    Vector2 shapeVert = shape.vertices[s].point;
                    Vector3 shapeVert3D = new Vector3(shapeVert.x, shapeVert.y, 0) * m_roadWidth;
                    shapeVert3D = rotationRelative * shapeVert3D;
                    Vector3 positionToCurve = shapeVert3D + centerRelative;
                    Vector3 localPosition = MeshHelpers.TransformWorldToLocal(positionToCurve, this.m_transform);
                    
                    vertices[vert + s] = localPosition;
                }

                vert += shape.vertices.Length;
            }

            
        }

        public void CreateTriangles()
        {
            if (shape == null)
                return;

            int quadCount = shape.vertexCount - 1;
            
            triangles = new int[quadCount * m_segments * 6];
            int vert = 0;
            int tris = 0;
            
            for (int z = 0; z < m_segments; z++)
            {
                //length
                for (int x = 0; x < quadCount; x++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + quadCount + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + quadCount + 1;
                    triangles[tris + 5] = vert + quadCount + 2;

                    vert++;
                    tris += 6;
                }

                vert++;
            }

        }

        public void CreateUVs() 
        {
            if (shape == null || shape.vertices.Length <= 0)
                return;

            int nbUVs = (m_segments + 1) * shape.vertices.Length;
            uvs = new Vector2[nbUVs];

			int vert = 0;
			for (int z = 0; z < m_segments; z++)
			{
				for (int s = 0; s < shape.vertices.Length; s++)
				{
					uvs[vert + s].Set(shape.vertices[s].uv.x, shape.vertices[s].uv.y);
				}

				vert += shape.vertices.Length;
			}
		}
        
    }
}