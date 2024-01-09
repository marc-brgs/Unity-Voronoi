using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _Intersections
    {
        public static bool PointWithinConvexHull(MyVector3 point, HalfEdgeData3 convexHull)
        {
            bool isInside = true;

            float epsilon = MathUtility.EPSILON;
            foreach (HalfEdgeFace3 triangle in convexHull.faces)
            {
                Plane3 plane = new Plane3(triangle.edge.v.position, triangle.edge.v.normal);

                float distance = _Geometry.GetSignedDistanceFromPointToPlane(point, plane);
                
                if (distance > 0f + epsilon)
                {
                    isInside = false;

                    break;
                }
            }

            return isInside;
        }
    }
}
