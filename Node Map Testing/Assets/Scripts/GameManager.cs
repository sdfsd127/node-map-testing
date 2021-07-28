using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject lineObjectPrefab;
    [SerializeField] GameObject meshObjectPrefab;
    [SerializeField] GameObject multiLineObjectPrefab;
    [SerializeField] Shader meshShader;

    public GameObject mapObject;
    private GameObject[] nodes;

    public bool produceEndMap;
    public bool mutateMap;

    private void Start()
    {
        // Get the nodes on the map, random or sequenced, and use their positions
        GetNodes();

        if (produceEndMap)
            CreateFullMap();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            CreateFullMap();
        }
    }

    private void CreateFullMap()
    {
        RemoveOldMap();

        Vector2[] positions = Triangulation.NodesToPositions(nodes);
        Shape newShape = Triangulation.DelaunayTriangulate(positions);
        ConnectionData[] connections = RemoveDoubleConnections(CreateConnections(newShape));
        MapData newMap = new MapData(newShape, connections);

        newMap.CreateRouteLength();
        newMap.CreateRouteColours();
        if (mutateMap)
            newMap.MutateRoutes();
        newMap.DisplayRoutes(multiLineObjectPrefab);
    }

    private void GetNodes()
    {
        nodes = mapObject.GetComponent<NodeSequence>().nodes;
    }

    private void RemoveOldMap()
    {
        GameObject oldMap = GameObject.Find("Parent MultiLine Object");
        if (oldMap != null)
            Destroy(oldMap);
    }

    private ConnectionData[] CreateConnections(Shape shapeData)
    {
        List<ConnectionData> connections = new List<ConnectionData>();

        for (int i = 0; i < shapeData.m_triangles.Length; i++)
        {
            TriangleData currentTriangle = shapeData.m_triangles[i];

            // Connect AB
            connections.Add(new ConnectionData(currentTriangle.positionA, currentTriangle.positionB));

            // Connect AC
            connections.Add(new ConnectionData(currentTriangle.positionA, currentTriangle.positionC));

            // Connect BC
            connections.Add(new ConnectionData(currentTriangle.positionB, currentTriangle.positionC));
        }

        return connections.ToArray();
    }

    private void DisplayConnections(ConnectionData[] connections)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            ConnectionData currentConnection = connections[i];

            // Create the line object
            GameObject lineObject = Instantiate(lineObjectPrefab, Vector3.zero, Quaternion.identity);

            // Create vertices array
            Vector3[] vertices = new Vector3[2];
            vertices[0] = new Vector3(currentConnection.aPointPosition.x, 0, currentConnection.aPointPosition.y);
            vertices[1] = new Vector3(currentConnection.bPointPosition.x, 0, currentConnection.bPointPosition.y);

            // Get the line renderer object
            lineObject.GetComponent<LineRenderer>().SetPositions(vertices);
        }
    }

    private ConnectionData[] RemoveDoubleConnections(ConnectionData[] connections)
    {
        List<ConnectionData> existingConnections = new List<ConnectionData>();

        for (int i = 0; i < connections.Length; i++)
        {
            ConnectionData currentConnection = connections[i];

            if (!ConnectionPresent(existingConnections, currentConnection))
                existingConnections.Add(currentConnection);
        }

        return existingConnections.ToArray();
    }

    private bool ConnectionPresent(List<ConnectionData> connections, ConnectionData target)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (target.Equals(connections[i]))
                return true;
        }

        return false;
    }
}
#pragma warning disable CS0659
public class ConnectionData
#pragma warning restore CS0659
{
    public Vector2 aPointPosition;
    public Vector2 bPointPosition;

    public int sizeOfConnection;

    public RouteData routeOne = new RouteData();
    public RouteData routeTwo = new RouteData();

    public ConnectionData() { }

    public ConnectionData(Vector2 va, Vector2 vb)
    {
        aPointPosition = va;
        bPointPosition = vb;
    }

    public static ConnectionData GetSharedEdge(TriangleData triOne, TriangleData triTwo)
    {
        List<Vector2> repeatedVertex = new List<Vector2>();
        List<Vector2> allVertex = new List<Vector2>();

        allVertex.Add(triOne.positionA);
        allVertex.Add(triOne.positionB);
        allVertex.Add(triOne.positionC);

        allVertex.Add(triTwo.positionA);
        allVertex.Add(triTwo.positionB);
        allVertex.Add(triTwo.positionC);

        for (int i = 0; i < allVertex.Count; i++)
        {
            for (int j = 0; j < allVertex.Count; j++)
            {
                if (i == j)
                    continue;

                if (allVertex[i] == allVertex[j])
                    if (!repeatedVertex.Contains(allVertex[i]))
                        repeatedVertex.Add(allVertex[i]);
            }
        }

        return new ConnectionData(repeatedVertex[0], repeatedVertex[1]);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
            return false;
        else
        {
            ConnectionData cd = (ConnectionData)obj;
            return this.Equal(cd);
        }
    }

    public bool Equal(ConnectionData other)
    {
        if (other.aPointPosition == aPointPosition && other.bPointPosition == bPointPosition)
            return true;
        else
            return false;
    }

    public float GetDistance()
    {
        return Vector2.Distance(aPointPosition, bPointPosition);
    }

    public void SetRouteLength(int length, int routeNum = 1)
    {
        if (routeNum == 1)
            routeOne.SetRouteSize(length);
        else
            routeTwo.SetRouteSize(length);
    }

    public int GetRouteLength(int routeNum = 1)
    {
        if (routeNum == 1)
            return routeOne.route_size;
        else
            return routeTwo.route_size;
    }

    public void SetRouteColour(Color colour, int routeNum = 1)
    {
        if (routeNum == 1)
            routeOne.route_colour = colour;
        else
            routeTwo.route_colour = colour;
    }

    public RouteData GetRoute(int routeNum = 1)
    {
        if (routeNum == 1)
            return routeOne;
        else
            return routeTwo;
    }

    public void SetNewPositions(Vector2 a, Vector2 b)
    {
        aPointPosition = a;
        bPointPosition = b;
    }

    public void DuplicateRoute()
    {
        routeTwo = new RouteData(routeOne.route_size, routeOne.route_colour, false);
    }

    public void RemoveRoute()
    {
        if (routeTwo.IsActive())
            routeTwo = new RouteData();
        else
            routeOne = new RouteData();
    }
}

public class RouteData
{
    public int route_size;
    public Color route_colour;
    public bool route_taken;

    private bool isActive = false;

    public RouteData()
    {
        route_size = 0;
        route_colour = Color.gray;
        route_taken = false;
    }

    public RouteData(int size, Color colour, bool taken)
    {
        route_size = size;
        route_colour = colour;
        route_taken = taken;

        isActive = true;
    }

    public void SetRouteSize(int size) 
    { 
        route_size = size;
        isActive = true;
    }

    public bool IsActive() { return isActive; }
}

public class MapData
{
    public static int MUTATE_NUMBER = 20;
    public static bool flip = false;
    public static bool remove = true;
    public static bool dupe = true;

    // Map points and triangle connections
    public Shape mapShape;

    // Two way connections between points calculated from triangles
    public ConnectionData[] mapConnections;

    public MapData() { }

    public MapData(Shape shape, ConnectionData[] connections)
    {
        mapShape = shape;
        mapConnections = connections;
    }

    public void CreateRouteLength()
    {
        // Find Min and Max distance of routes
        float minDist = float.MaxValue;
        float maxDist = float.MinValue;

        for (int i = 0; i < mapConnections.Length; i++)
        {
            float currentDist = mapConnections[i].GetDistance();

            if (currentDist < minDist)
                minDist = currentDist;
            else if (currentDist > maxDist)
                maxDist = currentDist;
        }

        // Create brackets for length of 1 to length of 6 routes
        const int MAX_ROUTE_LENGTH = 6;
        float difference = maxDist - minDist;
        float bracket = difference / MAX_ROUTE_LENGTH;

        // Assign length depending on the distance of the connection compared with the brackets
        for (int i = 0; i < mapConnections.Length; i++)
        {
            float currentDist = mapConnections[i].GetDistance();

            for (int j = 0; j < MAX_ROUTE_LENGTH; j++)
            {
                if (currentDist <= minDist + (bracket * (j + 1)))
                {
                    mapConnections[i].SetRouteLength(j + 1);
                    break;
                }
            }
        }
    }

    public void CreateRouteColours()
    {
        // Count number of active routes
        int totalRoutesNum = 0;

        for (int i = 0; i < mapConnections.Length; i++)
        {
            if (mapConnections[i].GetRoute(1).IsActive())
                totalRoutesNum++;
            if (mapConnections[i].GetRoute(2).IsActive())
                totalRoutesNum++;
        }

        // Create list of colours -> one for each route
        // 10% of each of 8 colours, 20% leftover is grey
        Color[] colours = new Color[] { Color.red, Color.green, Color.blue, Color.magenta, Color.black, Color.white, Color.yellow, new Color(1.0f, 0.64f, 0.0f), Color.grey, Color.grey };
        List<Color> totalColours = new List<Color>();

        for (int i = 0; i < totalRoutesNum; i++)
        {
            totalColours.Add(colours[i % 10]);
        }

        // Set the colours from the list
        for (int i = 0; i < totalRoutesNum; i++)
        {
            ConnectionData currentConnection = mapConnections[i];

            if (currentConnection.GetRoute(1).IsActive())
            {
                int randomIndex = Random.Range(0, totalColours.Count);
                Color chosenColour = totalColours[randomIndex];
                mapConnections[i].SetRouteColour(chosenColour);
                totalColours.RemoveAt(randomIndex);
            }
            if (currentConnection.GetRoute(2).IsActive())
            {
                int randomIndex = Random.Range(0, totalColours.Count);
                Color chosenColour = totalColours[randomIndex];
                mapConnections[i].SetRouteColour(chosenColour, 2);
                totalColours.RemoveAt(randomIndex);
            }
        }
    }

    public void DisplayRoutes(GameObject linePrefab)
    {
        Transform parentObject = new GameObject("Parent MultiLine Object").transform;

        for (int i = 0; i < mapConnections.Length; i++)
        {
            RouteData routeOne = mapConnections[i].GetRoute(1);
            RouteData routeTwo = mapConnections[i].GetRoute(2);

            Vector2 a = mapConnections[i].aPointPosition;
            Vector2 b = mapConnections[i].bPointPosition;

            if (routeOne.IsActive())
            {
                GameObject lineObject = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity, parentObject);
                lineObject.GetComponent<LineRendererSetup>().SetupLine(routeOne.route_size, routeOne.route_colour, new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
            }

            if (routeTwo.IsActive())
            {
                GameObject lineObject = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity, parentObject);
                lineObject.GetComponent<LineRendererSetup>().SetupLine(routeTwo.route_size, routeTwo.route_colour, new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
            }
        }
    }

    public void MutateRoutes()
    {
        for (int i = 0; i < MUTATE_NUMBER; i++)
        {
            RandomMutate();
        }
    }

    private void RandomMutate()
    {
        int randomChoice = Random.Range(0, 3);

        switch (randomChoice)
        {
            case 0:
                DuplicateEdge();
                break;
            case 1:
                RemoveEdge();
                break;
            case 2:
                FlipEdge();
                break;
            default:
                Debug.Log("ERROR: Mutating");
                break;
        }
    }

    private void RemoveEdge()
    {
        if (!remove)
            return;

        mapConnections[GetRandomEdgeIndex()].RemoveRoute();
    }

    private void DuplicateEdge()
    {
        if (!dupe)
            return;

        mapConnections[GetRandomEdgeIndex()].DuplicateRoute();
    }

    private void FlipEdge()
    {
        if (!flip)
            return;

        List<TriangleData[]> connectingTriangles = new List<TriangleData[]>();

        // Loop through all triangles
        for (int i = 0; i < mapShape.m_triangles.Length; i++)
        {
            TriangleData currentTriangle = mapShape.m_triangles[i];

            // Loop through all triangles again and compare each to each, except itself
            for (int j = 0; j < mapShape.m_triangles.Length; j++)
            {
                if (i == j)
                    continue;

                TriangleData checkTriangle = mapShape.m_triangles[j];

                if (currentTriangle.SharesMultipleVertex(checkTriangle))
                {
                    connectingTriangles.Add(new TriangleData[] { currentTriangle, checkTriangle  });
                }
            }
        }

        // Pick random one from possible
        int randomIndex = Random.Range(0, connectingTriangles.Count);
        TriangleData[] pairedTriangle = connectingTriangles[randomIndex];

        // Find shared edge
        ConnectionData sharedEdge = ConnectionData.GetSharedEdge(pairedTriangle[0], pairedTriangle[1]);

        // Set A, B, C, D, ready for swap algebra
        Vector2 a = sharedEdge.aPointPosition;
        Vector2 c = sharedEdge.bPointPosition;

        Vector2 b = pairedTriangle[0].GetUniqueVertex(a, c);
        Vector2 d = pairedTriangle[1].GetUniqueVertex(a, c);

        // Now want to swap the Triangles (A, B, C) and (A, C, D) to (D, B, C) and (A, B, D)
        // Remove old A to C connection and replace it with D to B connection
        int index = GetMapConnectionIndex(sharedEdge);
        mapConnections[index].SetNewPositions(b, d);
    }

    private int GetMapConnectionIndex(ConnectionData connection)
    {
        int index = 1;

        for (int i = 0; i < mapConnections.Length; i++)
        {
            if (mapConnections[i].Equals(connection))
            {
                index = i;
                break;
            }
        }

        return index;
    }

    private int GetRandomEdgeIndex() { return Random.Range(0, mapConnections.Length); }
}