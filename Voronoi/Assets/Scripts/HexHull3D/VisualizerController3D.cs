using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class VisualizerController3D : MonoBehaviour
{
    public MeshFilter displayMeshHere;
    public MeshFilter displayOtherMeshHere;

    public GameObject pointObj;
    public GameObject pointActiveObj;

    private HashSet<GameObject> allPoints = new HashSet<GameObject>();

    public Normalizer3 normalizer;

    public HashSet<HalfEdgeFace3> meshData;



    void Awake()
	{
        pointObj.SetActive(false);
        pointActiveObj.SetActive(false); 

        displayMeshHere.mesh = null;
        displayOtherMeshHere.mesh = null;

        StartConvexHull();
    }

    private void StartConvexHull()
    {
        HashSet<Vector3> points_Unity = GenerateRandomPoints3D(seed: Random.Range(0, 100000), halfCubeSize: 1f, numberOfPoints: 50);
        
        foreach (Vector3 p in points_Unity)
        {
            GameObject newPoint = Instantiate(pointObj, p, Quaternion.identity);

            newPoint.SetActive(true);

            allPoints.Add(newPoint);
        }
        
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));
        
        normalizer = new Normalizer3(new List<MyVector3>(points));

        VisualizeIterativeConvexHull visualizeThisAlgorithm = GetComponent<VisualizeIterativeConvexHull>();

        visualizeThisAlgorithm.StartVisualizer(points);
    }



    private void OnDrawGizmos()
    {
        if (meshData == null)
        {
            return;
        }

        Gizmos.color = Color.black;

        foreach (HalfEdgeFace3 f in meshData)
        {

            Vector3 p1 = f.edge.v.position.ToVector3();
            Vector3 p2 = f.edge.nextEdge.v.position.ToVector3();
            Vector3 p3 = f.edge.prevEdge.v.position.ToVector3();


            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p1);
        }
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

        this.meshData = meshDataUnNormalized;

        DisplayMesh(meshDataUnNormalized, displayMeshHere);
    }

    public void DisplayMeshOther(HashSet<HalfEdgeFace3> meshData)
    {
        HashSet<HalfEdgeFace3> meshDataUnNormalized = normalizer.UnNormalize(meshData);

        DisplayMesh(meshDataUnNormalized, displayOtherMeshHere);
        
        meshData = normalizer.Normalize(meshDataUnNormalized);
    }
    
    public void DisplayActivePoint(MyVector3 pos)
    {
        pointActiveObj.SetActive(true);
        pointActiveObj.transform.position = pos.ToVector3();
    }
    
    public void HideActivePoint()
    {
        pointActiveObj.SetActive(false);
    }
    
    public void HideVisiblePoint(MyVector3 pos)
    {
        Vector3 pos_unNormalized = normalizer.UnNormalize(pos).ToVector3();

        foreach (GameObject go in allPoints)
        {
            if (!go.activeInHierarchy)
            {
                continue;
            }

            if (Mathf.Abs(Vector3.Magnitude(pos_unNormalized - go.transform.position)) < 0.0001f)
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
    
    public static HashSet<Vector3> GenerateRandomPoints3D(int seed, float halfCubeSize, int numberOfPoints)
    {
        HashSet<Vector3> randomPoints = new HashSet<Vector3>();

        //Generate random numbers with a seed
        Random.InitState(seed);

        float max = halfCubeSize;
        float min = -halfCubeSize;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float randomX = Random.Range(min, max);
            float randomY = Random.Range(min, max);
            float randomZ = Random.Range(min, max);

            randomPoints.Add(new Vector3(randomX, randomY, randomZ));
        }

        return randomPoints;
    }
}
