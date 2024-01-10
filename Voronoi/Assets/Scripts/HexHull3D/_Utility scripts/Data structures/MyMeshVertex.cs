using UnityEngine;

public struct MyMeshVertex
{
    public Vector3 position;
    public Vector3 normal;

    public MyMeshVertex(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }
}