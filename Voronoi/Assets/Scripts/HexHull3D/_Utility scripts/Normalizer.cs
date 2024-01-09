using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //To avoid floating point precision issues, it's common to normalize all data to range 0-1
    public class Normalizer2
    {
        private float dMax;

        private AABB2 boundingBox;


        public Normalizer2(List<MyVector2> points)
        {
            this.boundingBox = new AABB2(points);

            this.dMax = CalculateDMax(this.boundingBox);
        }


        //From "A fast algorithm for constructing Delaunay triangulations in the plane" by Sloan
        //boundingBox is the rectangle that covers all original points before normalization
        public float CalculateDMax(AABB2 boundingBox)
        {
            float dMax = Mathf.Max(boundingBox.max.x - boundingBox.min.x, boundingBox.max.y - boundingBox.min.y);

            return dMax;
        }



        //
        // Normalize stuff
        //

        //MyVector2
        public MyVector2 Normalize(MyVector2 point)
        {
            float x = (point.x - boundingBox.min.x) / dMax;
            float y = (point.y - boundingBox.min.y) / dMax;

            MyVector2 pNormalized = new MyVector2(x, y);

            return pNormalized;
        }

        //List<MyVector2>
        public List<MyVector2> Normalize(List<MyVector2> points)
        {
            List<MyVector2> normalizedPoints = new List<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }

        //HashSet<MyVector2> 
        public HashSet<MyVector2> Normalize(HashSet<MyVector2> points)
        {
            HashSet<MyVector2> normalizedPoints = new HashSet<MyVector2>();

            foreach (MyVector2 p in points)
            {
                normalizedPoints.Add(Normalize(p));
            }

            return normalizedPoints;
        }
        public MyVector2 UnNormalize(MyVector2 point)
        {
            float x = (point.x * dMax) + boundingBox.min.x;
            float y = (point.y * dMax) + boundingBox.min.y;

            MyVector2 pUnNormalized = new MyVector2(x, y);

            return pUnNormalized;
        }
    }



    public class Normalizer3
    {
        private float dMax;

        private AABB3 boundingBox;


        public Normalizer3(List<MyVector3> points)
        {
            this.boundingBox = new AABB3(points);

            this.dMax = CalculateDMax(this.boundingBox);
        }


        public float CalculateDMax(AABB3 aabb)
        {
            float dMax = Mathf.Max(aabb.max.x - aabb.min.x, Mathf.Max(aabb.max.y - aabb.min.y, aabb.max.z - aabb.min.z));

            return dMax;
        }


        public MyVector3 Normalize(MyVector3 point)
        {
            float x = (point.x - boundingBox.min.x) / dMax;
            float y = (point.y - boundingBox.min.y) / dMax;
            float z = (point.z - boundingBox.min.z) / dMax;

            MyVector3 pNormalized = new MyVector3(x, y, z);

            return pNormalized;
        }


        //HashSet<HalfEdgeFace3>
        public HashSet<HalfEdgeFace3> Normalize(HashSet<HalfEdgeFace3> data)
        {
            foreach (HalfEdgeFace3 f in data)
            {
                //TODO: This will generate a new list for each face, so maybe better to put the code from the method here
                List<HalfEdge3> edges = f.GetEdges();

                if (edges == null)
                {
                    continue;
                }

                foreach (HalfEdge3 e in edges)
                {
                    HalfEdgeVertex3 v = e.v;

                    v.position = Normalize(v.position);
                }
            }

            return data;
        }
        
        public MyVector3 UnNormalize(MyVector3 point)
        {
            float x = (point.x * dMax) + boundingBox.min.x;
            float y = (point.y * dMax) + boundingBox.min.y;
            float z = (point.z * dMax) + boundingBox.min.z;

            MyVector3 pUnNormalized = new MyVector3(x, y, z);

            return pUnNormalized;
        }

        //HashSet<HalfEdgeFace3>
        public HashSet<HalfEdgeFace3> UnNormalize(HashSet<HalfEdgeFace3> data)
        {
            foreach (HalfEdgeFace3 f in data)
            {
                //TODO: This will generate a new list for each face, so maybe better to put the code from the method here
                List<HalfEdge3> edges = f.GetEdges();

                if (edges == null)
                {
                    continue;
                }

                foreach (HalfEdge3 e in edges)
                {
                    HalfEdgeVertex3 v = e.v;

                    v.position = UnNormalize(v.position);
                }
            }

            return data;
        }
    }
}
