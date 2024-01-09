using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
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
