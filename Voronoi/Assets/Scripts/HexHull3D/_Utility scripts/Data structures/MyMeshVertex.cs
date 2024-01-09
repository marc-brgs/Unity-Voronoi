using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MyMeshVertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector3 uv;

    public MyMeshVertex(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;

        this.uv = default;
    }
}