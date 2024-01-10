using System.Collections.Generic;
using UnityEngine;

public class VisualizerController3D : MonoBehaviour
{
    public MeshFilter displayMeshHere;

    public GameObject pointObj;
    public GameObject pointActiveObj;

    private readonly HashSet<GameObject> allPoints = new();
    public bool isSphere;
    public int numberOfPoints = 50;

    void Awake()
	{
        pointObj.SetActive(false);
        pointActiveObj.SetActive(false); 

        displayMeshHere.mesh = null;

        StartConvexHull();
    }

    private void StartConvexHull()
    {
        HashSet<Vector3> points_Unity = GenerateRandomPoints3D(seed: Random.Range(0, 100000), halfCubeSize: 5f, numberOfPoints: numberOfPoints, isSphere: isSphere);
        
        foreach (Vector3 p in points_Unity)
        {
            GameObject newPoint = Instantiate(pointObj, p, Quaternion.identity);

            newPoint.SetActive(true);

            allPoints.Add(newPoint);
        }

        VisualizeIterativeConvexHull visualizeThisAlgorithm = GetComponent<VisualizeIterativeConvexHull>();

        visualizeThisAlgorithm.StartVisualizer(points_Unity);
    }
    
    public void DisplayMesh(HashSet<HalfEdgeFace3> meshDataUnNormalized, MeshFilter mf)
    {
        MyMesh myMesh = HalfEdgeData3.ConvertToMyMesh("Main visualization mesh", meshDataUnNormalized, MyMesh.MeshStyle.HardEdges);

        Mesh mesh = myMesh.ConvertToUnityMesh(generateNormals: true);

        mf.mesh = mesh;
    }

    public void DisplayMeshMain(HashSet<HalfEdgeFace3> meshData)
    {
        HashSet<HalfEdgeFace3> meshDataUnNormalized = meshData;

        DisplayMesh(meshDataUnNormalized, displayMeshHere);
    }
    
    public void DisplayActivePoint(Vector3 pos)
    {
        pointActiveObj.SetActive(true);
        pointActiveObj.transform.position = pos;
    }
    
    public void HideActivePoint()
    {
        pointActiveObj.SetActive(false);
    }
    
    public void HideVisiblePoint(Vector3 pos)
    {
        foreach (GameObject go in allPoints)
        {
            if (!go.activeInHierarchy)
            {
                continue;
            }

            if (Mathf.Abs(Vector3.Magnitude(pos - go.transform.position)) < 0.0001f)
            {
                go.SetActive(false);

                break;
            }
        }
    }
    
    public void HideAllVisiblePoints(HashSet<HalfEdgeVertex3> verts)
    {
        foreach (HalfEdgeVertex3 v in verts)
        {
            HideVisiblePoint(v.position);
        }
    }

    private static HashSet<Vector3> GenerateRandomPoints3D(int seed, float halfCubeSize, int numberOfPoints, bool isSphere)
    {
        HashSet<Vector3> randomPoints = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = halfCubeSize;
        float min = -halfCubeSize;

        for (int i = 0; i < numberOfPoints; i++)
        {
            if (!isSphere)
            {
                float randomX = Random.Range(min, max);
                float randomY = Random.Range(min, max);
                float randomZ = Random.Range(min, max);

                randomPoints.Add(new Vector3(randomX, randomY, randomZ));
            }
            else
            {
                randomPoints.Add(Random.insideUnitSphere * halfCubeSize);
            }
        }

        return randomPoints;
    }
}
