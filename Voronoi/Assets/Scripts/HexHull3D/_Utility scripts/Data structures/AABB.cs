using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AABB3
{
    public Vector3 max;
    public Vector3 min;

    public AABB3(List<Vector3> points)
    {
        Vector3 p1 = points[0];

        this.min = p1;
        this.max = p1;

        if (points.Count == 1)
        {
            return;
        }
        
        for (int i = 1; i < points.Count; i++)
        {
            Vector3 p = points[i];

            //x
            if (p.x < min.x)
            {
                min.x = p.x;
            }
            else if (p.x > max.x)
            {
                max.x = p.x;
            }

            //y
            if (p.y < min.y)
            {
                min.y = p.y;
            }
            else if (p.y > max.y)
            {
                max.y = p.y;
            }

            //z
            if (p.z < min.z)
            {
                min.z = p.z;
            }
            else if (p.z > max.z)
            {
                max.z = p.z;
            }
        }
    }
}
