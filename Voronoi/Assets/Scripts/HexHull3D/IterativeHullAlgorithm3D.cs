using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class IterativeHullAlgorithm3D
    {
        public static void FindVisibleTrianglesAndBorderEdgesFromPoint(MyVector3 p, HalfEdgeData3 convexHull, out HashSet<HalfEdgeFace3> visibleTriangles, out HashSet<HalfEdge3> borderEdges)
        {
            borderEdges = new HashSet<HalfEdge3>();
            visibleTriangles = new HashSet<HalfEdgeFace3>();
            HalfEdgeFace3 visibleTriangle = FindVisibleTriangleFromPoint(p, convexHull.faces);

            if (visibleTriangle == null)
            {
                Debug.LogWarning("Couldn't find a visible triangle so will ignore the point");

                return;
            }


            Queue<HalfEdgeFace3> trianglesToFloodFrom = new Queue<HalfEdgeFace3>();

            trianglesToFloodFrom.Enqueue(visibleTriangle);

            List<HalfEdge3> edgesToCross = new List<HalfEdge3>();

            int safety = 0;

            while (true)
            {
                if (trianglesToFloodFrom.Count == 0)
                {
                    break;
                }

                HalfEdgeFace3 triangleToFloodFrom = trianglesToFloodFrom.Dequeue();

                visibleTriangles.Add(triangleToFloodFrom);

                edgesToCross.Clear();

                edgesToCross.Add(triangleToFloodFrom.edge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge);
                edgesToCross.Add(triangleToFloodFrom.edge.nextEdge.nextEdge);

                foreach (HalfEdge3 edgeToCross in edgesToCross)
                {
                    HalfEdge3 oppositeEdge = edgeToCross.oppositeEdge;

                    if (oppositeEdge == null)
                    {
                        Debug.LogWarning("Found an opposite edge which is null");

                        break;
                    }

                    HalfEdgeFace3 oppositeTriangle = oppositeEdge.face;

                    if (trianglesToFloodFrom.Contains(oppositeTriangle) || visibleTriangles.Contains(oppositeTriangle))
                    {
                        continue;
                    }
                    
                    Plane3 plane = new Plane3(oppositeTriangle.edge.v.position, oppositeTriangle.edge.v.normal);

                    bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                    if (isPointOutsidePlane)
                    {
                        trianglesToFloodFrom.Enqueue(oppositeTriangle);
                    }
                    else
                    {
                        borderEdges.Add(oppositeEdge);
                    }
                }


                safety += 1;

                if (safety > 50000)
                {
                    Debug.Log("Stuck in infinite loop when flood-filling visible triangles");

                    break;
                }
            }
        }

        private static HalfEdgeFace3 FindVisibleTriangleFromPoint(MyVector3 p, HashSet<HalfEdgeFace3> triangles)
        {
            HalfEdgeFace3 visibleTriangle = null;

            foreach (HalfEdgeFace3 triangle in triangles)
            {
                Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

                bool isPointOutsidePlane = _Geometry.IsPointOutsidePlane(p, plane);

                if (isPointOutsidePlane)
                {
                    visibleTriangle = triangle;

                    break;
                }
            }

            return visibleTriangle;
        }
        
        public static void BuildFirstTetrahedron(HashSet<MyVector3> points, HalfEdgeData3 convexHull)
        {
            Edge3 eFurthestApart = FindEdgeFurthestApart(points);

            points.Remove(eFurthestApart.p1);
            points.Remove(eFurthestApart.p2);


            MyVector3 pointFurthestAway = FindPointFurthestFromEdge(eFurthestApart, points);

            points.Remove(pointFurthestAway);
            
            MyVector3 p1 = eFurthestApart.p1;
            MyVector3 p2 = eFurthestApart.p2;
            MyVector3 p3 = pointFurthestAway;

            convexHull.AddTriangle(p1, p2, p3);
            convexHull.AddTriangle(p1, p3, p2);
            
            List<HalfEdgeFace3> triangles = new List<HalfEdgeFace3>(convexHull.faces);

            HalfEdgeFace3 triangle = triangles[0];

            Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

            MyVector3 p4 = FindPointFurthestAwayFromPlane(points, plane);

            points.Remove(p4);
            
            HalfEdgeFace3 triangleToRemove = triangles[0];
            HalfEdgeFace3 triangleToKeep = triangles[1];

            if (_Geometry.GetSignedDistanceFromPointToPlane(p4, plane) < 0f)
            {
                triangleToRemove = triangles[1];
                triangleToKeep = triangles[0];
            }
            
            convexHull.DeleteFace(triangleToRemove);

            MyVector3 p1_opposite = triangleToKeep.edge.v.position;
            MyVector3 p2_opposite = triangleToKeep.edge.nextEdge.v.position;
            MyVector3 p3_opposite = triangleToKeep.edge.nextEdge.nextEdge.v.position;

            convexHull.AddTriangle(p1_opposite, p3_opposite, p4);
            convexHull.AddTriangle(p3_opposite, p2_opposite, p4);
            convexHull.AddTriangle(p2_opposite, p1_opposite, p4);

            convexHull.ConnectAllEdgesSlow();

        }

       
        private static MyVector3 FindPointFurthestAwayFromPlane(HashSet<MyVector3> points, Plane3 plane)
        {
            MyVector3 bestPoint = default;

            float bestDistance = -Mathf.Infinity;

            foreach (MyVector3 p in points)
            {
                float distance = _Geometry.GetSignedDistanceFromPointToPlane(p, plane);

                float epsilon = MathUtility.EPSILON;

                if (distance > -epsilon && distance < epsilon)
                {
                    continue;
                }

                if (distance < 0f) distance *= -1f;

                if (distance > bestDistance)
                {
                    bestDistance = distance;

                    bestPoint = p;
                }
            }

            return bestPoint;
        }

        private static Edge3 FindEdgeFurthestApart(HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);


            MyVector3 maxX = points[0];
            MyVector3 minX = points[0];
            MyVector3 maxY = points[0];
            MyVector3 minY = points[0];
            MyVector3 maxZ = points[0];
            MyVector3 minZ = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 p = points[i];
            
                if (p.x > maxX.x)
                {
                    maxX = p;
                }
                if (p.x < minX.x)
                {
                    minX = p;
                }

                if (p.y > maxY.y)
                {
                    maxY = p;
                }
                if (p.y < minY.y)
                {
                    minY = p;
                }

                if (p.z > maxZ.z)
                {
                    maxZ = p;
                }
                if (p.z < minZ.z)
                {
                    minZ = p;
                }
            }

            HashSet<MyVector3> extremePointsHashSet = new HashSet<MyVector3>();

            extremePointsHashSet.Add(maxX);
            extremePointsHashSet.Add(minX);
            extremePointsHashSet.Add(maxY);
            extremePointsHashSet.Add(minY);
            extremePointsHashSet.Add(maxZ);
            extremePointsHashSet.Add(minZ);

            points = new List<MyVector3>(extremePointsHashSet);


            List<Edge3> pointCombinations = new List<Edge3>();

            for (int i = 0; i < points.Count; i++)
            {
                MyVector3 p1 = points[i];

                for (int j = i + 1; j < points.Count; j++)
                {
                    MyVector3 p2 = points[j];

                    Edge3 e = new Edge3(p1, p2);

                    pointCombinations.Add(e);
                }
            }

            Edge3 eFurthestApart = pointCombinations[0];

            float maxDistanceBetween = MyVector3.SqrDistance(eFurthestApart.p1, eFurthestApart.p2);

            for (int i = 1; i < pointCombinations.Count; i++)
            {
                Edge3 e = pointCombinations[i];

                float distanceBetween = MyVector3.SqrDistance(e.p1, e.p2);

                if (distanceBetween > maxDistanceBetween)
                {
                    maxDistanceBetween = distanceBetween;

                    eFurthestApart = e;
                }
            }

            return eFurthestApart;
        }


        private static MyVector3 FindPointFurthestFromEdge(Edge3 edge, HashSet<MyVector3> pointsHashSet)
        {
            List<MyVector3> points = new List<MyVector3>(pointsHashSet);

            MyVector3 pointFurthestAway = points[0];

            MyVector3 closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, pointFurthestAway, withinSegment: false);

            float maxDistSqr = MyVector3.SqrDistance(pointFurthestAway, closestPointOnLine);

            for (int i = 1; i < points.Count; i++)
            {
                MyVector3 thisPoint = points[i];
                
                closestPointOnLine = _Geometry.GetClosestPointOnLine(edge, thisPoint, withinSegment: false);

                float distSqr = MyVector3.SqrDistance(thisPoint, closestPointOnLine);

                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;

                    pointFurthestAway = thisPoint;
                }
            }


            return pointFurthestAway;
        }
    }
}