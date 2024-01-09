using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public class HalfEdgeData3
    {
        public HashSet<HalfEdgeVertex3> verts; 

        public HashSet<HalfEdgeFace3> faces;

        public HashSet<HalfEdge3> edges;

        public enum ConnectOppositeEdges
        {
            No,
            Fast,
            Slow
        }


        public HalfEdgeData3()
        {
            this.verts = new HashSet<HalfEdgeVertex3>();

            this.faces = new HashSet<HalfEdgeFace3>();

            this.edges = new HashSet<HalfEdge3>();
        }
        
        public HalfEdgeData3(MyMesh mesh, ConnectOppositeEdges connectOppositeEdges) : this()
        {
            //Loop through all triangles in the mesh
            List<int> triangles = mesh.triangles;

            List<MyVector3> vertices = mesh.vertices;
            List<MyVector3> normals = mesh.normals;

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int index1 = triangles[i + 0];
                int index2 = triangles[i + 1];
                int index3 = triangles[i + 2];

                MyVector3 p1 = vertices[index1];
                MyVector3 p2 = vertices[index2];
                MyVector3 p3 = vertices[index3];

                MyVector3 n1 = normals[index1];
                MyVector3 n2 = normals[index2];
                MyVector3 n3 = normals[index3];

                MyMeshVertex v1 = new MyMeshVertex(p1, n1);
                MyMeshVertex v2 = new MyMeshVertex(p2, n2);
                MyMeshVertex v3 = new MyMeshVertex(p3, n3);

                AddTriangle(v1, v2, v3);
            }

            if (connectOppositeEdges == ConnectOppositeEdges.Fast)
            {
                ConnectAllEdgesFast();
            }
            else if (connectOppositeEdges == ConnectOppositeEdges.Slow)
            {
                ConnectAllEdgesSlow();
            }
        }

        public HashSet<HalfEdge3> GetUniqueEdges()
        {
            HashSet<HalfEdge3> uniqueEdges = new HashSet<HalfEdge3>();

            foreach (HalfEdge3 e in edges)
            {
                MyVector3 p1 = e.v.position;
                MyVector3 p2 = e.prevEdge.v.position;

                bool isInList = false;

                //TODO: Use a dictionary to make this searcg faster
                foreach (HalfEdge3 uniqueEdge in uniqueEdges)
                {
                    MyVector3 p1_test = uniqueEdge.v.position;
                    MyVector3 p2_test = uniqueEdge.prevEdge.v.position;

                    if ((p1.Equals(p1_test) && p2.Equals(p2_test)) || (p2.Equals(p1_test) && p1.Equals(p2_test)))
                    {
                        isInList = true;

                        break;
                    }
                }

                if (!isInList)
                {
                    uniqueEdges.Add(e);
                }
            }

            return uniqueEdges;
        }


        public void ConnectAllEdgesSlow()
        {
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge == null)
                {
                    TryFindOppositeEdge(e);
                }
            }
        }
        
        public void ConnectAllEdgesFast()
        {
            Dictionary<Edge3, HalfEdge3> edgeLookup = new Dictionary<Edge3, HalfEdge3>();
            
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                //Each edge points TO a vertex
                MyVector3 p2 = e.v.position;
                MyVector3 p1 = e.prevEdge.v.position;

                edgeLookup.Add(new Edge3(p1, p2), e);
            }

            //Connect edges
            foreach (HalfEdge3 e in edges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                MyVector3 p1 = e.v.position;
                MyVector3 p2 = e.prevEdge.v.position;

                Edge3 edgeToLookup = new Edge3(p1, p2);

                HalfEdge3 eOther = null;

                edgeLookup.TryGetValue(edgeToLookup, out eOther);

                if (eOther != null)
                {
                    e.oppositeEdge = eOther;

                    eOther.oppositeEdge = e;
                }
            }
        }

        public void TryFindOppositeEdge(HalfEdge3 e)
        {
            TryFindOppositeEdge(e, edges);
        }

        public void TryFindOppositeEdge(HalfEdge3 e, HashSet<HalfEdge3> otherEdges)
        {
            MyVector3 pTo = e.prevEdge.v.position;
            MyVector3 pFrom = e.v.position;

            foreach (HalfEdge3 eOther in otherEdges)
            {
                if (eOther.oppositeEdge != null)
                {
                    continue;
                }

                if (eOther.v.position.Equals(pTo) && eOther.prevEdge.v.position.Equals(pFrom))
                {
                    e.oppositeEdge = eOther;

                    eOther.oppositeEdge = e;

                    break;
                }
            }
        }

        public void MergeMesh(HalfEdgeData3 otherMesh)
        {
            this.verts.UnionWith(otherMesh.verts);
            this.faces.UnionWith(otherMesh.faces);
            this.edges.UnionWith(otherMesh.edges);
        }

        public MyMesh ConvertToMyMesh(string meshName, MyMesh.MeshStyle meshStyle)
        {
            MyMesh myMesh = new MyMesh(meshName);
        
            foreach (HalfEdgeFace3 f in faces)
            {
                HalfEdgeVertex3 v1 = f.edge.v;
                HalfEdgeVertex3 v2 = f.edge.nextEdge.v;
                HalfEdgeVertex3 v3 = f.edge.nextEdge.nextEdge.v;

                MyMeshVertex my_v1 = new MyMeshVertex(v1.position, v1.normal);
                MyMeshVertex my_v2 = new MyMeshVertex(v2.position, v2.normal);
                MyMeshVertex my_v3 = new MyMeshVertex(v3.position, v3.normal);

                myMesh.AddTriangle(my_v1, my_v2, my_v3, meshStyle);
            }

            return myMesh;
        }

        public static MyMesh ConvertToMyMesh(string meshName, HashSet<HalfEdgeFace3> faces, MyMesh.MeshStyle meshStyle)
        {
            MyMesh myMesh = new MyMesh(meshName);

            foreach (HalfEdgeFace3 f in faces)
            {
                HalfEdgeVertex3 v1 = f.edge.v;
                HalfEdgeVertex3 v2 = f.edge.nextEdge.v;
                HalfEdgeVertex3 v3 = f.edge.nextEdge.nextEdge.v;

                MyMeshVertex my_v1 = new MyMeshVertex(v1.position, v1.normal);
                MyMeshVertex my_v2 = new MyMeshVertex(v2.position, v2.normal);
                MyMeshVertex my_v3 = new MyMeshVertex(v3.position, v3.normal);

                myMesh.AddTriangle(my_v1, my_v2, my_v3, meshStyle);
            }

            return myMesh;
        }

        public static HalfEdgeData3 GenerateHalfEdgeDataFromFaces(HashSet<HalfEdgeFace3> faces)
        {
            HalfEdgeData3 meshData = new HalfEdgeData3();

            HashSet<HalfEdge3> edges = new HashSet<HalfEdge3>();

            HashSet<HalfEdgeVertex3> verts = new HashSet<HalfEdgeVertex3>();

            foreach (HalfEdgeFace3 f in faces)
            {
                List<HalfEdge3> edgesInFace = f.GetEdges();

                foreach (HalfEdge3 e in edgesInFace)
                {
                    edges.Add(e);
                    verts.Add(e.v);
                }
            }

            meshData.faces = faces;
            meshData.edges = edges;
            meshData.verts = verts;

            return meshData;
        }

        public HalfEdgeFace3 AddTriangle(MyVector3 p1, MyVector3 p2, MyVector3 p3, bool findOppositeEdge = false)
        {
            MyVector3 normal = MyVector3.Normalize(MyVector3.Cross(p3 - p2, p1 - p2));

            MyMeshVertex v1 = new MyMeshVertex(p1, normal);
            MyMeshVertex v2 = new MyMeshVertex(p2, normal);
            MyMeshVertex v3 = new MyMeshVertex(p3, normal);

            HalfEdgeFace3 f = AddTriangle(v1, v2, v3);

            return f;
        }

        public HalfEdgeFace3 AddTriangle(MyMeshVertex v1, MyMeshVertex v2, MyMeshVertex v3, bool findOppositeEdge = false)
        {
            HalfEdgeVertex3 half_v1 = new HalfEdgeVertex3(v1.position, v1.normal);
            HalfEdgeVertex3 half_v2 = new HalfEdgeVertex3(v2.position, v2.normal);
            HalfEdgeVertex3 half_v3 = new HalfEdgeVertex3(v3.position, v3.normal);

            HalfEdge3 e_to_v1 = new HalfEdge3(half_v1);
            HalfEdge3 e_to_v2 = new HalfEdge3(half_v2);
            HalfEdge3 e_to_v3 = new HalfEdge3(half_v3);

            HalfEdgeFace3 f = new HalfEdgeFace3(e_to_v1);

            e_to_v1.nextEdge = e_to_v2;
            e_to_v2.nextEdge = e_to_v3;
            e_to_v3.nextEdge = e_to_v1;

            e_to_v1.prevEdge = e_to_v3;
            e_to_v2.prevEdge = e_to_v1;
            e_to_v3.prevEdge = e_to_v2;

            half_v1.edge = e_to_v2;
            half_v2.edge = e_to_v3;
            half_v3.edge = e_to_v1;

            e_to_v1.face = f;
            e_to_v2.face = f;
            e_to_v3.face = f;

            if (findOppositeEdge)
            {
                TryFindOppositeEdge(e_to_v1);
                TryFindOppositeEdge(e_to_v2);
                TryFindOppositeEdge(e_to_v3);
            }


            this.verts.Add(half_v1);
            this.verts.Add(half_v2);
            this.verts.Add(half_v3);

            this.edges.Add(e_to_v1);
            this.edges.Add(e_to_v2);
            this.edges.Add(e_to_v3);

            this.faces.Add(f);

            return f;
        }

        public void DeleteFace(HalfEdgeFace3 f)
        {
            List<HalfEdge3> edgesToRemove = f.GetEdges();

            if (edgesToRemove == null)
            {
                Debug.LogWarning("This face can't be deleted because the edges are not fully connected");

                return;
            }

            foreach (HalfEdge3 edgeToRemove in edgesToRemove)
            {
                if (edgeToRemove.oppositeEdge != null)
                {
                    edgeToRemove.oppositeEdge.oppositeEdge = null;
                }

                this.edges.Remove(edgeToRemove);
                this.verts.Remove(edgeToRemove.v);

                edgeToRemove.face = null;
            }

            this.faces.Remove(f);
        }

        public HashSet<HalfEdge3> ContractTriangleHalfEdge(HalfEdge3 e, MyVector3 mergePos, System.Diagnostics.Stopwatch timer = null)
        {
            HalfEdgeVertex3 v1 = e.prevEdge.v;
            HalfEdgeVertex3 v2 = e.v;

            HashSet<HalfEdge3> edgesGoingToVertex_v1 = v1.GetEdgesPointingToVertex(this);
            HashSet<HalfEdge3> edgesGoingToVertex_v2 = v2.GetEdgesPointingToVertex(this);
            RemoveTriangleAndConnectOppositeSides(e);

            if (e.oppositeEdge != null)
            {
                RemoveTriangleAndConnectOppositeSides(e.oppositeEdge);
            }
            HashSet<HalfEdge3> edgesPointingToVertex = new HashSet<HalfEdge3>();

            if (edgesGoingToVertex_v1 != null)
            {
                foreach (HalfEdge3 edgeToV in edgesGoingToVertex_v1)
                {
                    if (edgeToV.face == null)
                    {
                        continue;
                    }
                
                    edgeToV.v.position = mergePos;

                    edgesPointingToVertex.Add(edgeToV);
                }
            }
            if (edgesGoingToVertex_v2 != null)
            {
                foreach (HalfEdge3 edgeToV in edgesGoingToVertex_v2)
                {
                    if (edgeToV.face == null)
                    {
                        continue;
                    }

                    edgeToV.v.position = mergePos;

                    edgesPointingToVertex.Add(edgeToV);
                }
            }


            return edgesPointingToVertex;
        }

        private void RemoveTriangleAndConnectOppositeSides(HalfEdge3 e)
        {
            HalfEdge3 e_AB = e;
            HalfEdge3 e_BC = e.nextEdge;
            HalfEdge3 e_CA = e.nextEdge.nextEdge;

            HalfEdgeFace3 f_ABC = e.face;

            DeleteFace(f_ABC);
            if (e_BC.oppositeEdge != null)
            {
                e_BC.oppositeEdge.oppositeEdge = e_CA.oppositeEdge;
            }
            if (e_CA.oppositeEdge != null)
            {
                e_CA.oppositeEdge.oppositeEdge = e_BC.oppositeEdge;
            }
        }
    }



    public class HalfEdgeVertex3
    {
        public MyVector3 position;
        public MyVector3 normal;

        public HalfEdge3 edge;



        public HalfEdgeVertex3(MyVector3 position)
        {
            this.position = position;
        }

        public HalfEdgeVertex3(MyVector3 position, MyVector3 normal)
        {
            this.position = position;

            this.normal = normal;
        }


        public HashSet<HalfEdge3> GetEdgesPointingToVertex(HalfEdgeData3 meshData)
        {
            HashSet<HalfEdge3> allEdgesGoingToVertex = new HashSet<HalfEdge3>();

            HalfEdge3 currentEdge = this.edge.prevEdge;


            int safety = 0;

            do
            {
                allEdgesGoingToVertex.Add(currentEdge);

                HalfEdge3 oppositeEdge = currentEdge.oppositeEdge;

                if (oppositeEdge == null)
                {
                    Debug.LogWarning("We cant rotate around this vertex because there are holes in the mesh");

                    allEdgesGoingToVertex.Clear();

                    break;
                }


                currentEdge = oppositeEdge.prevEdge;

                safety += 1;

                if (safety > 1000)
                {
                    Debug.LogWarning("Stuck in infinite loop when getting all edges around a vertex");

                    allEdgesGoingToVertex.Clear();

                    break;
                }
            }
            while (currentEdge != this.edge.prevEdge);


            if (allEdgesGoingToVertex.Count == 0 && meshData != null)
            {
                HashSet<HalfEdge3> edges = meshData.edges;

                foreach (HalfEdge3 e in edges)
                {
                    //An edge points TO a vertex
                    if (e.v.position.Equals(position))
                    {
                        allEdgesGoingToVertex.Add(e);
                    }
                }
            }


            return allEdgesGoingToVertex;
        }
    }



    public class HalfEdgeFace3
    {
        public HalfEdge3 edge;

        public HalfEdgeFace3(HalfEdge3 edge)
        {
            this.edge = edge;
        }

        public List<HalfEdge3> GetEdges()
        {
            List<HalfEdge3> allEdges = new List<HalfEdge3>();
        
            HalfEdge3 currentEdge = this.edge;

            int safety = 0;

            do
            {
                allEdges.Add(currentEdge);

                currentEdge = currentEdge.nextEdge;

                safety += 1;

                if (safety > 100000)
                {
                    Debug.LogWarning("Stuck in infinite loop when getting all edges from a face");

                    return null;
                }
            }
            while (currentEdge != this.edge);

            return allEdges;
        }
    }

    public class HalfEdge3
    {
        public HalfEdgeVertex3 v;
        public HalfEdgeFace3 face;
        public HalfEdge3 nextEdge;
        public HalfEdge3 oppositeEdge;
        public HalfEdge3 prevEdge;



        public HalfEdge3(HalfEdgeVertex3 v)
        {
            this.v = v;
        }

        public float Length()
        {
            MyVector3 p2 = v.position;
            MyVector3 p1 = prevEdge.v.position;

            float length = MyVector3.Distance(p1, p2);

            return length;
        }

        public float SqrLength()
        {
            MyVector3 p2 = v.position;
            MyVector3 p1 = prevEdge.v.position;

            float length = MyVector3.SqrDistance(p1, p2);

            return length;
        }
    }
}
