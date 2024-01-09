using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public enum LeftOnRight
    {
        Left, On, Right
    }
    

    public static class _Geometry
    {
        public static bool IsTriangleOrientedClockwise(MyVector2 p1, MyVector2 p2, MyVector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        public static float GetPointInRelationToVectorValue(MyVector2 a, MyVector2 b, MyVector2 p)
        {
            float x1 = a.x - p.x;
            float x2 = a.y - p.y;
            float y1 = b.x - p.x;
            float y2 = b.y - p.y;

            float determinant = MathUtility.Det2(x1, x2, y1, y2);

            return determinant;
        }

        public static float GetSignedDistanceFromPointToPlane(MyVector3 pointPos, Plane3 plane)
        {
            float distance = MyVector3.Dot(plane.normal, pointPos - plane.pos);

            return distance;
        }

        public static bool IsPointOutsidePlane(MyVector3 pointPos, Plane3 plane) 
        {
            float distance = GetSignedDistanceFromPointToPlane(pointPos, plane);

            //To avoid floating point precision issues we can add a small value
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

        public static MyVector3 CalculateTriangleNormal(MyVector3 p1, MyVector3 p2, MyVector3 p3, bool shouldNormalize = true)
        {
            MyVector3 normal = MyVector3.Cross(p3 - p2, p1 - p2);

            if (shouldNormalize)
            {
                normal = MyVector3.Normalize(normal);
            }

            return normal;
        }
    }
}
