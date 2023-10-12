using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class PointManager : MonoBehaviour
{
    public GameObject controlPointPrefab; 
    private List<Vector3> controlPoints = new List<Vector3>(); 
    private bool inputEnabled = true;
    private LineRenderer lineRenderer;
    private bool polygonClosed = false;
    public Button cButton;
    public Button pButton; 
    
    public GameObject pointManagerObject;
    
    // Déplacer un point
    private List<GameObject> controlPointsObjects = new List<GameObject>();
    private GameObject bezierLine;
    private bool isHold = false;
    private GameObject closestPoint;
    private int closestIndex = 0;
    private bool isDrawned = false;
    private string lastMethod = "casteljau";
    
    // Lissage
    private int step = 100;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // Utilise la référence pour appeler les méthodes appropriées
        pButton.onClick.AddListener(() =>
        {
            ClearBezier();
            GeneratePascale(controlPoints);
            lastMethod = "pascale";
            isDrawned = true;
        });
        
        cButton.onClick.AddListener(() =>
        {
            ClearBezier();
            GenerateCasteljau(controlPoints);
            lastMethod = "casteljau";
            isDrawned = true;
        });
    }

    private void Update()
    {
        // Vérifie si la détection des clics est activée
        if (inputEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 screenPosition = Input.mousePosition;

                // Convertit la position du clic de l'écran à la position dans le monde
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));

                // Ajoute le point de contrôle à la liste
                controlPoints.Add(worldPosition);
                controlPointsObjects.Add(CreateControlPoint(worldPosition));
                UpdateLineRenderer();
            }
            if (Input.GetMouseButtonDown(1))
            {
                inputEnabled = false;
                cButton.gameObject.SetActive(true);
                pButton.gameObject.SetActive(true);
                ClosePolygon();
            }
        }

        // Déplacement d'un point
        if (isDrawned)
        {
            // Ajout d'un point
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
            {
                Vector3 screenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                
                float minDistance = 1000f;
                float minDistance2 = 1000f;
                
                int closestIndex2 = 0;
                int i = 0;
                foreach (var controlPointObj in controlPointsObjects)
                {
                    if (Vector3.Distance(controlPointObj.transform.position, worldPosition) < minDistance)
                    {
                        closestIndex2 = closestIndex;
                        minDistance2 = minDistance;
                        closestIndex = i;
                        minDistance = Vector3.Distance(controlPointObj.transform.position, worldPosition);
                    }
                    else if (Vector3.Distance(controlPointObj.transform.position, worldPosition) < minDistance2)
                    {
                        closestIndex2 = i;
                        minDistance2 = Vector3.Distance(controlPointObj.transform.position, worldPosition);
                    }
                    i++;
                }

                int minIndex = closestIndex < closestIndex2 ? closestIndex : closestIndex2;
                if (minIndex == 0 && (closestIndex == controlPoints.Count-1 || closestIndex2 == controlPoints.Count-1))
                    minIndex = controlPoints.Count-1; // point entre le premier et le dernier
                controlPoints.Insert(minIndex+1, worldPosition);
                controlPointsObjects.Insert(minIndex+1, CreateControlPoint(worldPosition));
                LiveRefresh();
            }
            else if (Input.GetMouseButtonDown(0) && !isHold)
            {
                Vector3 screenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));

                float minDistance = 1000f;
                
                int i = 0;
                foreach (var controlPointObj in controlPointsObjects)
                {
                    if (Vector3.Distance(controlPointObj.transform.position, worldPosition) < minDistance)
                    {
                        closestIndex = i;
                        minDistance = Vector3.Distance(controlPointObj.transform.position, worldPosition);
                    }
                    i++;
                }
                
                if (minDistance < 2f)
                {
                    isHold = true;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isHold)
            {
                isHold = false;
            }

            if (isHold)
            {
                Vector3 screenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                
                controlPoints[closestIndex] = worldPosition;
                controlPointsObjects[closestIndex].transform.position = worldPosition;
                LiveRefresh();
            }
            
            // Supprimer un point
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Vector3 screenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                
                float minDistance = 1000f;
                
                int i = 0;
                foreach (var controlPointObj in controlPointsObjects)
                {
                    if (Vector3.Distance(controlPointObj.transform.position, worldPosition) < minDistance)
                    {
                        closestIndex = i;
                        minDistance = Vector3.Distance(controlPointObj.transform.position, worldPosition);
                    }
                    i++;
                }

                controlPoints.RemoveAt(closestIndex);
                Destroy(controlPointsObjects[closestIndex]);
                controlPointsObjects.RemoveAt(closestIndex);
                LiveRefresh();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (step >= 100) step = 100;
                else if (step > 10) step += 10;
                else step++;
                
                Debug.Log(step);
                LiveRefresh();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (step <= 2) step = 2;
                else if (step < 10) step--;
                else if (step < 20) step -= 2;
                else step -= 10;
                
                Debug.Log(step);
                LiveRefresh();
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExtrudeBezierCurve(controlPoints, 5, 2f);
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                List<Vector3> pathPoints = new List<Vector3>();
                pathPoints.Add(new Vector3(0f, 0f, 0f));
                pathPoints.Add(new Vector3(0f, 0f, 1f));
                pathPoints.Add(new Vector3(0f, 1f, 2f));
                pathPoints.Add(new Vector3(0f, 0f, 3f));
                pathPoints.Add(new Vector3(0f, 1f, 4f));
                pathPoints.Add(new Vector3(0f, 0f, 5f));
                pathPoints.Add(new Vector3(0f, 1f, 6f));
                GeneralizedExtrudeBezierCurve(controlPoints, pathPoints);
            }
            
        }


        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearPoints();
            ClearBezier();
            inputEnabled = true;
            cButton.gameObject.SetActive(false);
            pButton.gameObject.SetActive(false);
            controlPoints.Clear();
            controlPointsObjects.Clear();
            UpdateLineRenderer();
            polygonClosed = false;
        }
        
    }

    public void ExtrudeBezierCurve(List<Vector3> controlPoints, float height, float scale)
    {
        // Calcule le nombre de points sur la courbe de Bézier
        int numPoints = step;
        Vector3[] bezierPoints = new Vector3[numPoints];

        // Parcourt les valeurs de paramètre t de 0 à 1 et calcule les points sur la courbe de Bézier
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            Vector3 point = DeCasteljau(controlPoints, t);
            bezierPoints[i] = point;
        }

        // Crée une liste de vertices pour le maillage de l'extrusion
        List<Vector3> vertices = new List<Vector3>();

        // Ajoute les vertices pour la base inférieure
        for (int i = 0; i < numPoints; i++)
        {
            vertices.Add(bezierPoints[i]);
        }

        // Ajoute les vertices pour la base supérieure en effectuant l'agrandissement ou la réduction
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 scaledPoint = bezierPoints[i] + Vector3.forward * height;
            Vector3 scaledVector = (scaledPoint - bezierPoints[i]) * scale;
            vertices.Add(bezierPoints[i] + scaledVector);
        }

        // Crée une liste de triangles pour le maillage de l'extrusion
        List<int> triangles = new List<int>();

        // Ajoute les triangles pour les côtés de l'extrusion
        for (int i = 0; i < numPoints - 1; i++)
        {
            // Triangle 1
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + numPoints);

            // Triangle 2
            triangles.Add(i + numPoints);
            triangles.Add(i + 1);
            triangles.Add(i + numPoints + 1);
        }

        // Ferme l'extrusion en ajoutant les triangles reliant les derniers points
        // Triangle 1
        triangles.Add(numPoints - 1);
        triangles.Add(0);
        triangles.Add(numPoints - 1 + numPoints);

        // Triangle 2
        triangles.Add(numPoints - 1 + numPoints);
        triangles.Add(0);
        triangles.Add(numPoints);

        // Crée un GameObject pour contenir le maillage de l'extrusion
        GameObject extrusionObject = new GameObject("Extrusion");
        MeshRenderer meshRenderer = extrusionObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = extrusionObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Assigne les vertices et triangles au maillage
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        // Recalcule les normales et les tangentes du maillage
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        // Assigne le maillage au composant MeshFilter
        meshFilter.mesh = mesh;
        
        // Inverse extrusion normals
        var indices = mesh.triangles;
        var triangleCount = indices.Length / 3;
        for(var i = 0; i < triangleCount; i++)
        {
            var tmp = indices[i*3];
            indices[i*3] = indices[i*3 + 1];
            indices[i*3 + 1] = tmp;
        }
        mesh.triangles = indices;
        var normals = mesh.normals;
        for(var n = 0; n < normals.Length; n++)
        {
            normals[n] = -normals[n];
        }
        mesh.normals = normals;

        // Crée un nouveau matériau
        Material material = new Material(Shader.Find("Standard"));

        // Applique une couleur au matériau
        material.color = Color.red;

        // Assigner le matériau au MeshRenderer
        meshRenderer.material = material;

        // Ajoute les deux faces manquantes

        // Crée une liste de vertices pour les faces
        List<Vector3> faceVertices = new List<Vector3>();

        // Ajoute les vertices pour la face inférieure
        for (int i = 0; i < numPoints; i++)
        {
            faceVertices.Add(bezierPoints[i]);
        }

        // Ajoute les vertices pour la face supérieure
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 scaledPoint = bezierPoints[i] + Vector3.forward * height;
            Vector3 scaledVector = (scaledPoint - bezierPoints[i]) * scale;
            faceVertices.Add(bezierPoints[i] + scaledVector);
        }

        // Crée une liste de triangles pour les faces
        List<int> faceTriangles = new List<int>();

        // Ajoute les triangles pour la face inférieure
        for (int i = 1; i < numPoints - 1; i++)
        {
            faceTriangles.Add(0);
            faceTriangles.Add(i);
            faceTriangles.Add(i + 1);
        }

        // Ajoute les triangles pour la face supérieure
        int startIndex = numPoints;
        for (int i = startIndex + 1; i < startIndex + numPoints - 1; i++)
        {
            faceTriangles.Add(startIndex);
            faceTriangles.Add(i + 1);
            faceTriangles.Add(i);
        }

        // Crée un GameObject pour contenir les faces
        GameObject facesObject = new GameObject("Faces");
        MeshRenderer facesRenderer = facesObject.AddComponent<MeshRenderer>();
        MeshFilter facesFilter = facesObject.AddComponent<MeshFilter>();
        Mesh facesMesh = new Mesh();

        // Assigne les vertices et triangles au maillage des faces
        facesMesh.SetVertices(faceVertices);
        facesMesh.SetTriangles(faceTriangles, 0);

        // Recalcule les normales et les tangentes du maillage des faces
        facesMesh.RecalculateNormals();
        facesMesh.RecalculateTangents();

        // Assigne le maillage des faces au composant MeshFilter
        facesFilter.mesh = facesMesh;

        // Assigner le matériau au MeshRenderer des faces
        facesRenderer.material = material;

        // Place l'objet extrusion dans la scène
        extrusionObject.transform.position = new Vector3(0f, 0f, 0f);
    }
    
    public void GeneralizedExtrudeBezierCurve(List<Vector3> controlPoints, List<Vector3> pathPoints)
    {
        int numPoints = step;
        int numPathPoints = pathPoints.Count;
        Vector3[] bezierPoints = new Vector3[numPoints];

        // Parcourt les valeurs de paramètre t de 0 à 1 et calcule les points sur la courbe de Bézier
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            Vector3 point = DeCasteljau(controlPoints, t);
            bezierPoints[i] = point;
        }

        // Crée une liste de vertices pour le maillage de l'extrusion
        List<Vector3> vertices = new List<Vector3>();

        // Ajoute les vertices pour chaque point le long de la courbe de Bézier
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 bezierPoint = bezierPoints[i];

            for (int j = 0; j < numPathPoints; j++)
            {
                Vector3 pathPoint = pathPoints[j];
                vertices.Add(bezierPoint + pathPoint);
            }
        }

        // Crée une liste de triangles pour le maillage de l'extrusion
        List<int> triangles = new List<int>();

        // Calcule le nombre de points par ligne
        int numPointsPerLine = numPathPoints;

        // Ajoute les triangles pour chaque face de l'extrusion
        for (int i = 0; i < numPoints - 1; i++)
        {
            int startIndex = i * numPointsPerLine;

            for (int j = 0; j < numPointsPerLine - 1; j++)
            {
                // Triangle 1
                triangles.Add(startIndex + j);
                triangles.Add(startIndex + j + 1);
                triangles.Add(startIndex + j + numPointsPerLine);

                // Triangle 2
                triangles.Add(startIndex + j + numPointsPerLine);
                triangles.Add(startIndex + j + 1);
                triangles.Add(startIndex + j + numPointsPerLine + 1);
            }
        }

        // Crée un GameObject pour contenir le maillage de l'extrusion
        GameObject extrusionObject = new GameObject("Extrusion");
        MeshRenderer meshRenderer = extrusionObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = extrusionObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Assigne les vertices et triangles au maillage
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        // Recalcule les normales et les tangentes du maillage
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        // Assigne le maillage au composant MeshFilter
        meshFilter.mesh = mesh;

        // Crée un nouveau matériau
        Material material = new Material(Shader.Find("Standard"));

        // Applique une couleur au matériau
        material.color = Color.red;

        // Assigner le matériau au MeshRenderer
        meshRenderer.material = material;

        // Place l'objet extrusion dans la scène
        extrusionObject.transform.position = new Vector3(0f, 0f, 0f);
    }



    private void ClearBezier()
    {
        Destroy(bezierLine);
    }

    private void ClearPoints()
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("Point");
        
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }
    }

    private void LiveRefresh()
    {
        ClearBezier();
        if(lastMethod == "casteljau") GenerateCasteljau(controlPoints);
        else if (lastMethod == "pascale") GeneratePascale(controlPoints);
        UpdateLineRenderer();
        polygonClosed = false;
        ClosePolygon();
    }



    private Vector3 DeCasteljau(List<Vector3> controlPoints, float t)
    {
        List<Vector3> intermediatePoints = new List<Vector3>(controlPoints);

        while (intermediatePoints.Count > 1)
        {
            List<Vector3> newPoints = new List<Vector3>();

            for (int i = 0; i < intermediatePoints.Count - 1; i++)
            {
                Vector3 point = Vector3.Lerp(intermediatePoints[i], intermediatePoints[i + 1], t);
                newPoints.Add(point);
            }

            intermediatePoints = newPoints;
        }

        return intermediatePoints[0];
    }

    public void GenerateCasteljau(List<Vector3> controlPoints)
    {
        float startTime = Time.realtimeSinceStartup;
        
        if (controlPoints.Count < 2)
        {
            Debug.LogWarning("Il doit y avoir au moins deux points de contrôle pour générer une courbe de Bézier.");
            return;
        }
        
        int numPoints = step;
        Vector3[] bezierPoints = new Vector3[numPoints];

        // Parcourt les valeurs de paramètre t de 0 à 1 et calcule les points sur la courbe de Bézier
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            Vector3 point = DeCasteljau(controlPoints, t);
            bezierPoints[i] = point;
        }
        
        GameObject casteljauCurve = new GameObject("Casteljau Curve");
        LineRenderer bezierLineRenderer = casteljauCurve.AddComponent<LineRenderer>();
        bezierLineRenderer.positionCount = numPoints;
        
        bezierLineRenderer.startWidth = 0.1f;
        bezierLineRenderer.endWidth = 0.1f;
        bezierLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        bezierLineRenderer.startColor = Color.yellow;
        bezierLineRenderer.endColor = Color.yellow;
        
        bezierLineRenderer.SetPositions(bezierPoints);
        bezierLine = casteljauCurve;

        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Temps de calcul : " + elapsedTime + " secondes");
    }
    
    
    public void GeneratePascale(List<Vector3> controlPoints)
    {
        float startTime = Time.realtimeSinceStartup;
        
        if (controlPoints.Count < 2)
        {
            Debug.LogWarning("Il doit y avoir au moins deux points de contrôle pour générer une courbe de Bézier.");
            return;
        }
        
        int numPoints = step;
        Vector3[] bezierPoints = new Vector3[numPoints];

        // Calcule les coefficients binomiaux à l'aide du triangle de Pascal
        int n = controlPoints.Count - 1;
        int[] coefficients = new int[n + 1];
        for (int i = 0; i <= n; i++)
        {
            coefficients[i] = CalculateBinomialCoefficient(n, i);
        }
        
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            
            float x = 0f;
            float y = 0f;
            float z = 0f;

            for (int j = 0; j <= n; j++)
            {
                float blend = coefficients[j] * Mathf.Pow(t, j) * Mathf.Pow(1f - t, n - j);
                x += controlPoints[j].x * blend;
                y += controlPoints[j].y * blend;
                z += controlPoints[j].z * blend;
            }

            bezierPoints[i] = new Vector3(x, y, z);
        }
        
        GameObject pascaleCurve = new GameObject("Pascale Curve");
        LineRenderer bezierLineRenderer = pascaleCurve.AddComponent<LineRenderer>();
        bezierLineRenderer.positionCount = numPoints;
        
        bezierLineRenderer.startWidth = 0.1f;
        bezierLineRenderer.endWidth = 0.1f;
        bezierLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        bezierLineRenderer.startColor = Color.blue;
        bezierLineRenderer.endColor = Color.blue;
        
        bezierLineRenderer.SetPositions(bezierPoints);
        bezierLine = pascaleCurve;
        
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Temps de calcul : " + elapsedTime + " secondes");
    }
    
    
    private int CalculateBinomialCoefficient(int n, int k)
    {
        if (k > n - k)
        {
            k = n - k;
        }

        int coefficient = 1;
        for (int i = 0; i < k; i++)
        {
            coefficient = coefficient * (n - i) / (i + 1);
        }

        return coefficient;
    }


    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = controlPoints.Count;
        for (int i = 0; i < controlPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, controlPoints[i]);
        }
    }
    
    
    private void ClosePolygon()
    {
        if (polygonClosed)
            return;
        
        if (controlPoints.Count >= 2)
        {
            controlPoints.Add(controlPoints[0]);
            UpdateLineRenderer();
            polygonClosed = true;
            controlPoints.RemoveAt(controlPoints.Count - 1);
        }
    }


    private GameObject CreateControlPoint(Vector3 position)
    {
        GameObject controlPoint = Instantiate(controlPointPrefab, position, Quaternion.identity);
        return controlPoint;
    }
}
