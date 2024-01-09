using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Habrador_Computational_Geometry
{
    public static class _GenerateMesh
    {
        public static HashSet<Triangle2> GenerateGrid(float width, int cells)
        {
            HashSet<Triangle2> grid = MeshAlgorithms.Grid.GenerateGrid(width, cells);

            return grid;
        }

        public static HashSet<Triangle2> Circle(MyVector2 center, float radius, int resolution)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.Circle(center, radius, resolution);

            return triangles;
        }

        public static HashSet<Triangle2> CircleHollow(MyVector2 center, float innerRadius, int resolution, float width)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.CircleHollow(center, innerRadius, resolution, width);

            return triangles;
        }

        public static HashSet<Triangle2> LineSegment(MyVector2 p1, MyVector2 p2, float width)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.LineSegment(p1, p2, width);

            return triangles;
        }
        
        public static HashSet<Triangle2> ConnectedLineSegments(List<MyVector2> points, float width, bool isConnected)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.ConnectedLineSegments(points, width, isConnected);

            return triangles;
        }

        public static HashSet<Triangle2> Arrow(MyVector2 p1, MyVector2 p2, float lineWidth, float arrowSize)
        {
            HashSet<Triangle2> triangles = MeshAlgorithms.Shapes.Arrow(p1, p2, lineWidth, arrowSize);

            return triangles;
        }
    }
}
