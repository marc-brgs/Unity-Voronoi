using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeIterativeConvexHull : MonoBehaviour
{
    private VisualizerController3D controller;
    
    public void StartVisualizer(HashSet<Vector3> points)
    {
        controller = GetComponent<VisualizerController3D>();

        HalfEdgeData3 convexHull = new HalfEdgeData3();
        
        IterativeHullAlgorithm3D.BuildFirstTetrahedron(points, convexHull);
        
        StartCoroutine(GenerateHull(points, convexHull));
    }



    private IEnumerator GenerateHull(HashSet<Vector3> points, HalfEdgeData3 convexHull)
    {
        controller.DisplayMeshMain(convexHull.faces);
        controller.HideAllVisiblePoints(convexHull.verts);

        yield return new WaitForSeconds(1f);

        List<Vector3> pointsToAdd = new List<Vector3>(points);

        foreach (Vector3 p in pointsToAdd)
        {
            bool isWithinHull = _Intersections.PointWithinConvexHull(p, convexHull);

            if (isWithinHull)
            {
                points.Remove(p);

                controller.HideVisiblePoint(p);

                continue;
            }

            
            controller.DisplayActivePoint(p);

            HashSet<HalfEdgeFace3> visibleTriangles = null;
            HashSet<HalfEdge3> borderEdges = null;

            IterativeHullAlgorithm3D.FindVisibleTrianglesAndBorderEdgesFromPoint(p, convexHull, out visibleTriangles, out borderEdges);

            foreach (HalfEdgeFace3 triangle in visibleTriangles)
            {
                convexHull.DeleteFace(triangle);
            }

            controller.DisplayMeshMain(convexHull.faces);
            controller.HideAllVisiblePoints(convexHull.verts);

            yield return new WaitForSeconds(2f);


            List<HalfEdgeFace3> visibleTrianglesList = new List<HalfEdgeFace3>(visibleTriangles);

            for (int i = 0; i < visibleTrianglesList.Count; i++)
            {
                visibleTriangles.Remove(visibleTrianglesList[i]);

                yield return new WaitForSeconds(0.5f);
            }

            
            HashSet<HalfEdge3> newEdges = new HashSet<HalfEdge3>();

            foreach (HalfEdge3 borderEdge in borderEdges)
            {
                Vector3 p1 = borderEdge.prevEdge.v.position;
                Vector3 p2 = borderEdge.v.position;

                HalfEdgeFace3 newTriangle = convexHull.AddTriangle(p2, p1, p);


                controller.DisplayMeshMain(convexHull.faces);

                yield return new WaitForSeconds(0.5f);

                HalfEdge3 edgeToConnect = newTriangle.edge.nextEdge;

                edgeToConnect.oppositeEdge = borderEdge;
                borderEdge.oppositeEdge = edgeToConnect;

                HalfEdge3 e1 = newTriangle.edge;
                HalfEdge3 e3 = newTriangle.edge.nextEdge.nextEdge;

                newEdges.Add(e1);
                newEdges.Add(e3);
            }

            foreach (HalfEdge3 e in newEdges)
            {
                if (e.oppositeEdge != null)
                {
                    continue;
                }

                convexHull.TryFindOppositeEdge(e, newEdges);
            }

            controller.HideVisiblePoint(p);
        }


        controller.HideActivePoint();
        
        yield return null;
    }
}
