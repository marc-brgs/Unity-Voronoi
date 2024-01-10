using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DelaunayVoronoi
{
    public class DelaunayTriangulator
    {
        private double MaxX { get; set; }
        private double MaxY { get; set; }
        private double MinX { get; set; }
        private double MinY { get; set; }

        // Init points, MaxX, MaxY, MinX, MinY
        public IEnumerable<Point> ConvertAndInitialize(List<GameObject> gameObjects)
        {
            // Coordonn�es du rectangle englobant
            MaxX = gameObjects.Max(go => go.transform.position.x);
            MaxY = gameObjects.Max(go => go.transform.position.y);
            MinX = gameObjects.Min(go => go.transform.position.x);
            MinY = gameObjects.Min(go => go.transform.position.y);

            List<Point> points = new List<Point>();

            // Convertir les GameObjects en Points
            foreach (GameObject go in gameObjects)
            {
                points.Add(new Point(go.transform.position.x, go.transform.position.y));
            }

            return points;
        }

        public HashSet<Triangle> BowyerWatson(IEnumerable<Point> points)
        {
            Triangle superTriangle = GenerateSuperTriangle();
            HashSet<Triangle> triangulation = new HashSet<Triangle>(new List<Triangle> { superTriangle });

            foreach (Point point in points)
            {
                // Trouve les triangles dont le cercle circonscrit contient le point (ne v�rifie pas le crit�re de Delaunay)
                List<Triangle> badTriangles = FindBadTriangles(point, triangulation);
                List<Edge> polygon = FindHoleBoundaries(badTriangles);

                // Supprime les triangles adjacents au vertices des mauvais triangles
                foreach (Triangle triangle in badTriangles)
                {
                    foreach (Point vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                // Pour chaque ar�te du polygone form� par les ar�tes des mauvais triangles, 
                // cr�e un nouveau triangle avec le point en cours de traitement
                foreach (Edge edge in polygon)
                {
                    Triangle triangle = new Triangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            // Supprime les triangles ayant au moins une ar�te en commun avec le super-triangle
            triangulation.RemoveWhere(o => o.Vertices.Any(v => superTriangle.Vertices.Contains(v)));
            return triangulation;
        }

        private List<Edge> FindHoleBoundaries(List<Triangle> badTriangles)
        {
            List<Edge> edges = new List<Edge>();

            // Ajoute toutes les ar�tes des mauvais triangles � la liste
            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }

            // S�lectionne les ar�tes qui n'ont qu'une seule occurrence dans le groupe.
            // Ces ar�tes sont celles qui ne sont partag�es avec aucun autre des mauvais triangles.
            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());

            // Retourne la fronti�re du trou
            return boundaryEdges.ToList();
        }

        private Triangle GenerateSuperTriangle()
        {
            // C
            // | \
            // A---B
            double epsilon = 1; // Ne peut pas �tre � 0 sinon 3 points seront collin�aires
            Point A = new Point(MinX - epsilon, MinY - epsilon);
            Point B = new Point(MinX + 2*(MaxX - MinX) + 3 * epsilon, MinY - epsilon);
            Point C = new Point(MinX - epsilon, MinY + 2 * (MaxY - MinY) + 3 * epsilon);
            return new Triangle(A, B, C);
        }

        // Trouve les triangles dont le cercle circonscrit contient le point
        private List<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircleCirconscrit(point));
            return new List<Triangle>(badTriangles);
        }
    }
}