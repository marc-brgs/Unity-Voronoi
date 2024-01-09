using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    public class MyMesh
    {
        public List<MyVector3> vertices;
        public List<MyVector3> normals;
        public List<int> triangles;

        public string meshName;

        public enum MeshStyle
        {
            HardEdges,
            SoftEdges,
            HardAndSoftEdges
        }


        public MyMesh(string meshName = null)
        {
            this.meshName = meshName;
        
            vertices = new List<MyVector3>();
            normals = new List<MyVector3>();
            triangles = new List<int>();
        }

        public void AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, MeshStyle meshStyle)
        {
            int index1 = AddVertexAndReturnIndex(v1, meshStyle);
            int index2 = AddVertexAndReturnIndex(v2, meshStyle);
            int index3 = AddVertexAndReturnIndex(v3, meshStyle);

            AddTrianglePositions(index1, index2, index3);
        }

        public int AddVertexAndReturnIndex(MyMeshVertex v, MeshStyle meshStyle)
        {
            int vertexPosInList = -1;

            if (meshStyle == MeshStyle.SoftEdges || meshStyle == MeshStyle.HardAndSoftEdges)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    MyVector3 thisPos = vertices[i];
                   
                    if (thisPos.Equals(v.position))
                    {
                        MyVector3 thisNormal = normals[i];

                        if (meshStyle == MeshStyle.HardAndSoftEdges && thisNormal.Equals(v.normal))
                        {
                            vertexPosInList = i;

                            return vertexPosInList;
                        }
                        
                        if (meshStyle == MeshStyle.SoftEdges)
                        {
                            vertexPosInList = i;

                            return vertexPosInList;
                        }
                    }
                }
            }

            vertices.Add(v.position);
            normals.Add(v.normal);

            vertexPosInList = vertices.Count - 1;

            return vertexPosInList;
        }
        public void AddTrianglePositions(int index_1, int index_2, int index_3)
        {
            triangles.Add(index_1);
            triangles.Add(index_2);
            triangles.Add(index_3);
        }
        
        
        public Mesh ConvertToUnityMesh(bool generateNormals, string meshName = null)
        {
            Mesh mesh = new Mesh();

            //MyVector3 to Vector3
            Vector3[] vertices_Unity = vertices.Select(x => x.ToVector3()).ToArray();
          
            mesh.vertices = vertices_Unity;

            mesh.SetTriangles(triangles, 0);

            //Generate normals
            if (normals.Count == 0 || generateNormals)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                //MyVector3 to Vector3
                Vector3[] normals_Unity = normals.Select(x => x.ToVector3()).ToArray();

                mesh.normals = normals_Unity;
            }

            if (meshName != null)
            {
                mesh.name = meshName;
            }
            else
            {
                if (this.meshName != null)
                {
                    mesh.name = this.meshName;
                }
            }

            

            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
