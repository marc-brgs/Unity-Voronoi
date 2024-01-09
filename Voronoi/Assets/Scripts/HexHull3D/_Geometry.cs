using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _Geometry
    {
        public static float GetSignedDistanceFromPointToPlane(MyVector3 pointPos, Plane3 plane)
        {
            float distance = MyVector3.Dot(plane.normal, pointPos - plane.pos);

            return distance;
        }

        public static bool IsPointOutsidePlane(MyVector3 pointPos, Plane3 plane) 
        {
            float distance = GetSignedDistanceFromPointToPlane(pointPos, plane);
            
            float epsilon = MathUtility.EPSILON;

            if (distance > 0f + epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static MyVector3 GetClosestPointOnLine(Edge3 e, MyVector3 p, bool withinSegment)
        {
            MyVector3 a = e.p1;
            MyVector3 b = e.p2;

            MyVector3 ab = b - a;
            MyVector3 ap = p - a;

            float distance = MyVector3.Dot(ap, ab) / MyVector3.SqrMagnitude(ab);


            float epsilon = MathUtility.EPSILON;

            if (withinSegment && distance < 0f - epsilon)
            {
                return a;
            }
            else if (withinSegment && distance > 1f + epsilon)
            {
                return b;
            }
            else
            {
                return a + ab * distance;
            }
        }
    }
}
