using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public struct Edge3
    {
        public MyVector3 p1;
        public MyVector3 p2;

        public Edge3(MyVector3 p1, MyVector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
