using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private GameObject polygon;
    [SerializeField] private GameObject window;

    private LineRenderer lrPolygon;
    private LineRenderer lrWindow;

    private bool drawingPolygon;
    private bool drawingWindow;
    private int polygonIndex;
    private int windowIndex;

    [SerializeField] private GameObject textPolygon;
    [SerializeField] private GameObject textWindow;
    
    [SerializeField] private Image img;
    private Color fillColor;

    // Voronoi
    [SerializeField] private GameObject pointPrefab;
    private List<GameObject> points = new List<GameObject>();

    private bool drawingPoints;
    [SerializeField] private LineRenderer lineRenderer;

    public enum ConvexHullMethod { Jarvis, GrahamScan };

    [SerializeField] Button btnJarvis;
    [SerializeField] Button btnGraham;
    [SerializeField] Button btnIncremental;

    private int realtime = 0;

    // Start is called before the first frame update
    void Start()
    {
        drawingPoints = true;

        btnJarvis.onClick.AddListener(() =>
        {
            Destroy(points[points.Count-1]);
            points.RemoveAt(points.Count-1);

            ComputeConvexHull(points, ConvexHullMethod.Jarvis);
            realtime = 1;
        });

        btnGraham.onClick.AddListener(() =>
        {
            Destroy(points[points.Count - 1]);
            points.RemoveAt(points.Count - 1);

            ComputeConvexHull(points, ConvexHullMethod.GrahamScan);
            realtime = 2;
        });

        btnIncremental.onClick.AddListener(() =>
        {
            Destroy(points[points.Count - 1]);
            points.RemoveAt(points.Count - 1);

            List<Vector3> l = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                l.Add(points[i].transform.position);
            }
            List<Vector3> tri = IncrementalTriangulation(l);
            Debug.Log(tri.Count);

            lineRenderer.positionCount = tri.Count;
            for (int i = 0; i < tri.Count; i++)
            {
                lineRenderer.SetPosition(i, tri[i]);
            }
            realtime = 3;
        });
    }
    
    // Update is called once per frame
    void Update()
    {
        if (drawingPoints)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 screenPosition = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 1f));
                points.Add(CreatePoint(worldPosition));

                
                if (realtime == 1)
                {
                    ComputeConvexHull(points, ConvexHullMethod.Jarvis);
                }
                else if(realtime == 2)
                {
                    ComputeConvexHull(points, ConvexHullMethod.GrahamScan);
                }
                else if (realtime == 3)
                {
                    List<Vector3> l = new List<Vector3>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        l.Add(points[i].transform.position);
                    }
                    List<Vector3> tri = IncrementalTriangulation(l);
                    Debug.Log(tri.Count);

                    lineRenderer.positionCount = tri.Count;
                    for (int i = 0; i < tri.Count; i++)
                    {
                        lineRenderer.SetPosition(i, tri[i]);
                    }
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Delete))
        {
            lineRenderer.SetPositions(Array.Empty<Vector3>());
            lineRenderer.positionCount = 0;
            drawingPoints = true;

            // Delete points
            foreach(GameObject p in points)
            {
                Destroy(p);
            }
            points.Clear();

            realtime = 0;
        }
    }

    private GameObject CreatePoint(Vector3 position)
    {
        GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
        return point;
    }


    public void ComputeConvexHull(List<GameObject> points, ConvexHullMethod method)
    {
        List<Vector3> convexHullPoints = new List<Vector3>();
        if(method == ConvexHullMethod.Jarvis)
            convexHullPoints = JarvisMarch(points);
        else if(method == ConvexHullMethod.GrahamScan) {
            convexHullPoints = GrahamScan2(points);
        }

        // Créez un LineRenderer pour afficher l'enveloppe convexe
        lineRenderer.positionCount = 0;

        // Remplissez le LineRenderer avec les points de l'enveloppe convexe
        for (int i = 0; i < convexHullPoints.Count; i++)
        {
            lineRenderer.positionCount = i+1;
            lineRenderer.SetPosition(i, convexHullPoints[i]);
        }
    }

    // Algorithme de Jarvis pour calculer l'enveloppe convexe
    private List<Vector3> JarvisMarch(List<GameObject> inputPoints)
    {
        List<Vector3> convexHull = new List<Vector3>();

        if (inputPoints.Count < 3)
            return convexHull;

        // Trouvez le point le plus à gauche comme point de départ
        GameObject startPoint = inputPoints[0];
        foreach (GameObject point in inputPoints)
        {
            if (point.transform.position.x < startPoint.transform.position.x)
            {
                startPoint = point;
            }
        }

        // Ajoutez le point de départ à l'enveloppe convexe
        convexHull.Add(startPoint.transform.position);

        GameObject currentPoint = startPoint;
        do
        {
            GameObject nextPoint = null;
            foreach (GameObject point in inputPoints)
            {
                if (point == currentPoint)
                    continue;
                if (nextPoint == null || IsClockwise(currentPoint, point, nextPoint))
                {
                    nextPoint = point;
                }
            }

            if (nextPoint != startPoint)
            {
                convexHull.Add(nextPoint.transform.position);
            }

            currentPoint = nextPoint;
        }
        while (currentPoint != startPoint);

        return convexHull;
    }

    // Vérifie si les points sont disposés dans le sens des aiguilles d'une montre
    private bool IsClockwise(GameObject a, GameObject b, GameObject c)
    {
        Vector2 ab = b.transform.position - a.transform.position;
        Vector2 ac = c.transform.position - a.transform.position;
        return (ab.x * ac.y - ab.y * ac.x) <= 0;
    }

    private Vector3 ComputeCentroid(List<GameObject> inputPoints)
    {
        Vector3 centroid = Vector3.zero;

        foreach (GameObject point in inputPoints)
        {
            centroid += point.transform.position;
        }

        centroid /= inputPoints.Count;

        return centroid;
    }

    private List<Vector3> SortPointsByPolarAngle(List<GameObject> inputPoints, Vector3 centroid)
    {
        List<Vector3> sortedPoints = inputPoints
            .Select(p => p.transform.position)
            .OrderBy(p => Mathf.Atan2(p.y - centroid.y, p.x - centroid.x))
            .ToList();

        return sortedPoints;
    }

    private bool IsConvex(LinkedListNode<Vector3> point, Vector3 centroid, LinkedList<Vector3> convexHullLinkedList)
    {
        LinkedListNode<Vector3> prevNode = point.Previous ?? convexHullLinkedList.Last;
        LinkedListNode<Vector3> nextNode = point.Next ?? convexHullLinkedList.First;

        Vector3 prev = prevNode.Value;
        Vector3 next = nextNode.Value;

        // Calculez les angles orientés
        float angle = Mathf.Atan2(point.Value.y - centroid.y, point.Value.x - centroid.x);
        float anglePrev = Mathf.Atan2(prev.y - centroid.y, prev.x - centroid.x);
        float angleNext = Mathf.Atan2(next.y - centroid.y, next.x - centroid.x);

        return anglePrev <= angle && angle <= angleNext;
    }

    // Algorithme du Graham Scan pour calculer l'enveloppe convexe.
    private List<Vector3> GrahamScan(List<GameObject> inputPoints)
    {
        List<Vector3> convexHull = new List<Vector3>();

        if (inputPoints.Count < 3)
            return convexHull;

        Vector3 barycentre = ComputeCentroid(inputPoints);
        List<Vector3> sortedPoints = SortPointsByPolarAngle(inputPoints, barycentre);

        // Suppression des points non convexes
        LinkedList<Vector3> convexHullLinkedList = new LinkedList<Vector3>(sortedPoints);

        LinkedListNode<Vector3> pivot = convexHullLinkedList.First;
        LinkedListNode<Vector3> start = pivot;

        bool advance = true;

        do
        {
            if (IsConvex(pivot, barycentre, convexHullLinkedList))
            {
                pivot = pivot.Next ?? convexHullLinkedList.First;
                advance = true;
            }
            else
            {
                start = pivot.Previous ?? convexHullLinkedList.Last;
                convexHullLinkedList.Remove(pivot);
                pivot = start;
                advance = false;
            }
        } while (pivot != start || !advance);

        convexHull.Clear();
        
        foreach (Vector3 point in convexHullLinkedList)
        {
            convexHull.Add(point);
        }

        return convexHull;
    }

    // Vérifie si les points sont disposés dans le sens trigonométrique (anti-horaire)
    private bool IsCounterClockwise(Vector3 a, Vector3 b, GameObject c)
    {
        Vector2 ab = b - a;
        Vector2 ac = c.transform.position - a;
        return (ab.x * ac.y - ab.y * ac.x) > 0;
    }

    private List<Vector3> IncrementalTriangulation(List<Vector3> inputPoints)
    {
        List<Vector3> triangulation = new List<Vector3>();

        if (inputPoints.Count < 3)
            return triangulation;

        // Triez les points en fonction de leur abscisse croissante
        inputPoints.Sort((a, b) => a.x.CompareTo(b.x));

        // Initialisez la triangulation avec les deux premiers points
        triangulation.Add(inputPoints[0]);
        triangulation.Add(inputPoints[1]);

        List<Edge> colinearEdges = new List<Edge>
        {
            new Edge(inputPoints[0], inputPoints[1]),
        };

        // Parcours des points restants un par un
        for (int i = 2; i < inputPoints.Count; i++)
        {
            Vector3 currentPoint = inputPoints[i];

            // Recherche des arêtes "vues" depuis le point courant
            List<Edge> visibleEdges = new List<Edge>();

            foreach (Edge edge in colinearEdges)
            {
                if (IsEdgeVisibleFromPoint(edge, currentPoint))
                {
                    visibleEdges.Add(edge);
                }
            }

            // Ajout de nouveaux triangles à la triangulation
            foreach (Edge edge in visibleEdges)
            {
                triangulation.Add(edge.P1);
                triangulation.Add(edge.P2);
                triangulation.Add(currentPoint);
            }

            // Ajout des nouvelles arêtes colinéaires
            List<Edge> newColinearEdges = new List<Edge>();

            foreach (Edge edge in colinearEdges)
            {
                if (!IsEdgeVisibleFromPoint(edge, currentPoint))
                {
                    newColinearEdges.Add(edge);
                }
            }

            newColinearEdges.Add(new Edge(colinearEdges.Last().P2, currentPoint));

            colinearEdges = newColinearEdges;
        }

        return triangulation;
    }

    // Vérifie si une arête est "vue" depuis un point donné
    private bool IsEdgeVisibleFromPoint(Edge edge, Vector3 point)
    {
        Vector3 v1 = edge.P1 - point;
        Vector3 v2 = edge.P2 - point;

        return Vector3.Cross(v1, v2).z < 0; // Vérifie si le produit vectoriel est négatif (sens trigonométrique)
    }



    // OLD
    
    /*
     * Determine if a point is inside of a side of polygon with clockwise normal
     * Used by Sutherland and RemplissageRectEG algorithms
     */
    private bool visible(Vector3 S, Vector3 F1, Vector3 F2)
    {
        Vector2 midToS = new Vector2(S.x - F1.x, S.y - F1.y);
        
        Vector2 n = new Vector2(-(F2.y - F1.y), F2.x - F1.x);
        Vector2 m = -n;
        
        if(Vector3.Dot(n, midToS) < 0) // dedans
            return true;
        if(Vector3.Dot(n, midToS) > 0) // dehors
            return false;
        // sur le bord de la fenêtre
        return true;
    }
    
    /*
     * Reset sprite texture used for filling to transparent
     */
    private void clearTexture(Texture2D tex)
    {
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        
        // Obtenir tous les gameobjects qui s'appellent "Line"
        GameObject[] lines = GameObject.FindGameObjectsWithTag("Line");

        // Détruire chaque gameobject
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
    }
    
    public void ChangeColor(int color=0)
    {
        switch (color)
        {
            case 0:
                fillColor = Color.yellow;
                break;
            case 1:
                fillColor = Color.magenta;
                break;
            case 2:
                fillColor = Color.green;
                break;
            case 3:
                fillColor = Color.cyan;
                break;
        }
        RemplissageRectEG();
    }
    
    // Remplissage RectEG
    public void RemplissageRectEG()
    {
        // Recover data
        Vector3[] Poly = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(Poly);
        int nb = 0;
        Vector2[] rectEG = rectangleEnglobant(Poly);
        int xmin = (int) rectEG[0].x;
        int ymin = (int) rectEG[0].y;
        int xmax = (int) rectEG[1].x;
        int ymax = (int) rectEG[1].y;

        for (int x = xmin; x < xmax; x++)
        {
            for (int y = ymin; y < ymax; y++)
            {
                if (interieur(x, y, Poly))
                {
                    affichePixel(x, y);
                    nb++;
                }
            }
        }
        
        img.sprite.texture.Apply();
        img.gameObject.SetActive(true);
    }

    /*
     * Used by RemplissageRectEG to determine x y min max for pixel loop optimization
     */
    private Vector2[] rectangleEnglobant(Vector3[] Poly)
    {
        int xmin = Screen.width, xmax = 0, ymin = Screen.height, ymax = 0;
        
        for (int i = 0; i < Poly.Length; i++)
        {
            Vector2 polyPixel = new Vector2(worldPosXToPixel(Poly[i].x), worldPosYToPixel(Poly[i].y));
            //Debug.Log("worldpos: " + Poly[i].x + " " + Poly[i].y + ", pixel: " + worldPosXToPixel(Poly[i].x) + " " + worldPosYToPixel(Poly[i].y));
            if (polyPixel.x < xmin)
              xmin = (int) polyPixel.x;
            if (polyPixel.x > xmax)
                xmax = (int) polyPixel.x;
            if (polyPixel.y < ymin)
                ymin = (int) polyPixel.y;
            if (polyPixel.y > ymax)
                ymax = (int) polyPixel.y;
        }
        
        //Debug.Log("xmin: " + xmin + ", xmax: " + xmax + ", ymin: " + ymin + ", ymax: " + ymax);
        Vector2[] rectEG = new Vector2[2];
        rectEG[0] = new Vector2(xmin, ymin); // P1
        rectEG[1] = new Vector2(xmax, ymax); // P2
        return rectEG;
    }

    /*
     * Used by RemplissageRectEG to determine if a point is inside polygon
     */
    private bool interieur(int x, int y, Vector3[] poly)
    {
        // Only working for convex polygons
        for (int i = 0; i < poly.Length-1; i++)
        {
            if(!visible(new Vector3(x, y), new Vector3(worldPosXToPixel(poly[i].x), worldPosYToPixel(poly[i].y)), new Vector3(worldPosXToPixel(poly[i+1].x), worldPosYToPixel(poly[i+1].y)))) 
                return false;
        }
        
        return true;
    }

    /*
     * Change pixel color of sprite texture used for filling display
     */
    private void affichePixel(int x, int y)
    {
        img.sprite.texture.SetPixel(x, y, fillColor);
    }
    
    /*
     * Convert world position x axis value to pixel
     */
    private int worldPosXToPixel(float v)
    {
        return (int) (((v + 5.33) * Screen.width) / 10.66);
    }
    
    /*
     * Convert world position y axis value to pixel
     */
    private int worldPosYToPixel(float v)
    {
        return (int) (((v + 3) * Screen.height) / 6);
    }
    
    /*
     * Click actions event for drawing window
     */
    private void drawWindow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            lrWindow.positionCount = windowIndex + 1;
            lrWindow.SetPosition(windowIndex, mouseWorldPosition);
            windowIndex++;
        }

        if (Input.GetMouseButtonDown(1))
        {
            lrWindow.positionCount = windowIndex + 1;
            lrWindow.SetPosition(windowIndex, lrWindow.GetPosition(0));
            windowIndex++;
            drawingWindow = false;
            drawingPolygon = true;
            textWindow.SetActive(false);
            textPolygon.SetActive(true);
        }
    }

    /*
     * Click actions event for drawing polygon
     */
    private void drawPolygon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;
            lrPolygon.positionCount = polygonIndex + 1;
            lrPolygon.SetPosition(polygonIndex, mouseWorldPosition);
            polygonIndex++;
        }

        if (lrPolygon.positionCount > 1 && Input.GetMouseButtonDown(1))
        {
            lrPolygon.positionCount = polygonIndex + 1;
            lrPolygon.SetPosition(polygonIndex, lrPolygon.GetPosition(0));
            polygonIndex++;
            drawingPolygon = false;
            textPolygon.SetActive(false);
        }
    }

    /**
     * Remplissage par ligne
     */
    public void RemplissageLigne()
    {
        // Liste des points du polygone
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);

        // Trier les points par angle polaire croissant
        points = SortPointsByPolarAngle(points);

        // Tracer une ligne entre chaque paire de points consécutifs
        for (int i = 0; i < points.Length - 1; i++)
        {
            DrawLine(points[i], points[i + 1], Color.red);
        }

        // Tracer une ligne entre le dernier et le premier point pour fermer le polygone
        DrawLine(points[points.Length - 1], points[0], Color.red);
    }

    public void RemplissageLigne2()
    {
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);
        
        // Nombre de lignes parallèles à tracer
        int numLines = 5;

        // Distance entre chaque ligne parallèle
        float lineSpacing = 0.1f;

        // Tracer les lignes parallèles
        for (int i = 0; i < numLines; i++)
        {
            // Calculer la distance entre le bord du polygone et la ligne parallèle
            float distance = lineSpacing * (i + 1);

            // Tracer une ligne parallèle pour chaque point du bord du polygone
            for (int j = 0; j < points.Length - 1; j++)
            {
                // Calculer le vecteur normal au bord du polygone
                Vector3 normal = Vector3.Cross(points[j + 1] - points[j], Vector3.forward).normalized;

                // Calculer les points de départ et d'arrivée de la ligne parallèle
                Vector3 start = points[j] + normal * distance;
                Vector3 end = points[j + 1] + normal * distance;

                // Vérifier si la ligne intersecte un bord du polygone
                Vector3 intersection = GetLineIntersection(start, end, points[j], points[j + 1]);
                if (intersection != Vector3.zero)
                {
                    // La ligne intersecte un bord du polygone, mettre à jour l'extrémité de la ligne
                    end = intersection;
                }

                // Tracer la ligne parallèle
                DrawLine(start, end, Color.green);
            }
        }
    }
    
    Vector3 GetLineIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        Vector3 intersection = Vector3.zero;

        // Calculer les vecteurs s et t
        Vector3 s = line1End - line1Start;
        Vector3 t = line2End - line2Start;

        // Calculer la valeur de u
        float u = (-t.y * s.x + s.y * t.x) / (-t.x * s.y + s.x * t.y);

        // Vérifier si les lignes s'intersectent
        if (u >= 0 && u <= 1)
        {
            // Calculer l'intersection
            intersection = line1Start + u * s;
        }

        return intersection;
    }

    
    public void RemplissageLigne3()
    {
        Vector3[] points = new Vector3[lrPolygon.positionCount];
        lrPolygon.GetPositions(points);
        points = SortPointsByPolarAngle(points);
        
        // Créer une RenderTexture temporaire pour stocker le rendu de la caméra
        RenderTexture tempRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        mainCamera.targetTexture = tempRenderTexture;
        mainCamera.Render();

        // Créer un texture2D vide pour stocker les pixels lus depuis la RenderTexture
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

        // Lire les pixels de la RenderTexture dans le texture2D
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        texture.ReadPixels(rect, (int)points[0].x, (int)points[0].y);
        texture.Apply();
        
        Color CC = Color.red, CR = Color.blue;
        int x = 0, y = 0;
        // Couleur pixel courant, droite et gauche
        Color CP, CPd, CPg;

        // Pile pour stocker les germes
        Stack<Vector2Int> p = new Stack<Vector2Int>();

        // Abscisses extrêmes droite et gauche de la ligne de balayage
        int xd, xg;

        // Empiler le germe (x,y)
        p.Push(new Vector2Int(x, y));

        // Tant qu'il y a des germes à traiter
        while (p.Count > 0)
        {
            // Récupérer le sommet de la pile
            Vector2Int point = p.Peek();
            x = point.x;
            y = point.y;

            // Dépiler le germe
            p.Pop();

            // Récupérer la couleur du pixel courant
            CP = texture.GetPixel(x, y);

            // Rechercher xd : extrême à droite
            xd = x + 1;
            CPd = texture.GetPixel(xd, y);
            while (CPd != CC && xd < texture.width)
            {
                xd++;
                CPd = texture.GetPixel(xd, y);
            }
            xd--;

            // Rechercher xg : extrême à gauche
            xg = x - 1;
            CPg = CP;
            while (CPg != CC && xg >= 0)
            {
                xg--;
                CPg = texture.GetPixel(xg, y);
            }
            xg++;

            // Tracer la ligne de balayage de xg à xd avec la couleur CR
            DrawLine(new Vector3(xg, y, 0), new Vector3(xd, y, 0), CR);

            // Rechercher de nouveaux germes sur la ligne de balayage au-dessus
            x = xd;
            CP = texture.GetPixel(x, y + 1);
            while (x > xg)
            {
                while (((CP == CC) || (CP == CR)) && (x > xg))
                {
                    x--;
                    CP = texture.GetPixel(x, y + 1);
                }
                if ((x > xg) && (CP != CC) && (CP != CR))
                {
                    // Empiler le nouveau germe au-dessus trouvé
                    p.Push(new Vector2Int(x, y + 1));
                }
                while ((CP != CC) && (x > xg))
                {
                    x--;
                    CP = texture.GetPixel(x, y + 1);
                }
            }

            // Rechercher de nouveaux germes sur la ligne de balayage au-dessous
            x = xd;
            CP = texture.GetPixel(x, y - 1);
            while (x > xg)
            {
                while (CP == CC)
                {
                    while ((CP != CC) && (x > xg))
                    {
                        x--;
                        CP = texture.GetPixel(x, y - 1);
                    }
                }
            }
        }
    }


    private List<Vector3> GrahamScan2(List<GameObject> inputPoints)
    {
        return JarvisMarch(inputPoints);
    }

    /**
     * Tri les points par angle polaire
     */
    Vector3[] SortPointsByPolarAngle(Vector3[] points)
    {
        // Trouver le point le plus à gauche (avec le plus petit abscisse)
        int minXIndex = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].x < points[minXIndex].x)
            {
                minXIndex = i;
            }
        }

        // Déplacer le point le plus à gauche en tête de liste
        (points[0], points[minXIndex]) = (points[minXIndex], points[0]);

        // Trier les points restants par angle polaire croissant
        Array.Sort(points, 1, points.Length - 1, new PolarAngleComparer(points[0]));

        return points;
    }
    
    /**
     * Remplir une ligne
     */
    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        // Créer un nouvel objet "Line"
        GameObject line = new GameObject("Line");
        line.tag = "Line";

        // Ajouter un composant LineRenderer à l'objet
        LineRenderer lr = line.AddComponent<LineRenderer>();

        // Configurer le LineRenderer
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}

public class Edge
{
    public Vector3 P1 { get; set; }
    public Vector3 P2 { get; set; }

    public Edge(Vector3 p1, Vector3 p2)
    {
        P1 = p1;
        P2 = p2;
    }
}