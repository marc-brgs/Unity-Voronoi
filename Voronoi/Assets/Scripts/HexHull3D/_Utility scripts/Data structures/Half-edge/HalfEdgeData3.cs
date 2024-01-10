using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeData3
{
    public HashSet<HalfEdgeVertex3> verts = new(); 
    public HashSet<HalfEdgeFace3> faces = new();
    public HashSet<HalfEdge3> edges = new();


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
    
    public void TryFindOppositeEdge(HalfEdge3 e)
    {
        TryFindOppositeEdge(e, edges);
    }

    public void TryFindOppositeEdge(HalfEdge3 e, HashSet<HalfEdge3> otherEdges)
    {
        Vector3 pTo = e.prevEdge.v.position;
        Vector3 pFrom = e.v.position;

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

    public HalfEdgeFace3 AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3, bool findOppositeEdge = false)
    {
        Vector3 normal = Vector3.Normalize(Vector3.Cross(p3 - p2, p1 - p2));

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
}



public class HalfEdgeVertex3
{
    public Vector3 position;
    public Vector3 normal;

    public HalfEdge3 edge;

    public HalfEdgeVertex3(Vector3 position, Vector3 normal)
    {
        this.position = position;

        this.normal = normal;
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
    
        HalfEdge3 currentEdge = edge;

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
        while (currentEdge != edge);

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
}
