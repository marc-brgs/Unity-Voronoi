using System.Collections.Generic;
using UnityEngine;

public class Normalizer3
{
    private float dMax;

    private AABB3 boundingBox;


    public Normalizer3(List<Vector3> points)
    {
        this.boundingBox = new AABB3(points);

        this.dMax = CalculateDMax(this.boundingBox);
    }


    public float CalculateDMax(AABB3 aabb)
    {
        float dMax = Mathf.Max(aabb.max.x - aabb.min.x, Mathf.Max(aabb.max.y - aabb.min.y, aabb.max.z - aabb.min.z));

        return dMax;
    }
    
    public Vector3 UnNormalize(Vector3 point)
    {
        float x = (point.x * dMax) + boundingBox.min.x;
        float y = (point.y * dMax) + boundingBox.min.y;
        float z = (point.z * dMax) + boundingBox.min.z;

        Vector3 pUnNormalized = new Vector3(x, y, z);

        return pUnNormalized;
    }
}
