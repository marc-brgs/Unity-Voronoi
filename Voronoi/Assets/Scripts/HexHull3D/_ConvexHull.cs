using System.Collections;
using System.Collections.Generic;
using Habrador_Computational_Geometry;
using UnityEngine;

public static class _ConvexHull
{
    public static List<MyVector2> JarvisMarch_2D(HashSet<MyVector2> points)
    {
        List<MyVector2> pointsList = new List<MyVector2>(points);

        if (!CanFormConvexHull_2d(pointsList))
        {
            return null;
        }
        
        List<MyVector2> pointsOnHull = JarvisMarchAlgorithm2D.GenerateConvexHull(pointsList);

        return pointsOnHull;
    }
    private static bool CanFormConvexHull_2d(List<MyVector2> points)
    {
        if (points.Count < 3)
        {
            Debug.Log("Too few points co calculate a convex hull");

            return false;
        }
        
        AABB2 rectangle = new AABB2(points);

        if (!rectangle.IsRectangleARectangle())
        {
            Debug.Log("The points cant form a convex hull");

            return false;
        }

        return true;
    }
}
