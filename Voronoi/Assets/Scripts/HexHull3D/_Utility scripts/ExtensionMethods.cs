using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class ExtensionMethods
    {
        public static MyVector3 ToMyVector3(this Vector3 v)
        {
            return new MyVector3(v.x, v.y, v.z);
        }

        public static Vector3 ToVector3(this MyVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        
        public static MyVector3 ToMyVector3_Yis3D(this MyVector2 v, float yPos = 0f)
        {
            return new MyVector3(v.x, yPos, v.y);
        }
    }
}
