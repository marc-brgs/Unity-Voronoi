using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class MathUtility
    {
        public const float EPSILON = 0.00001f;

        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }
        
        public static float Det2(float x1, float x2, float y1, float y2)
        {
            return x1 * y2 - y1 * x2;
        }

    }
}
