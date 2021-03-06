using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathHelper;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject lineObjectPrefab;
    [SerializeField] GameObject meshObjectPrefab;
    [SerializeField] GameObject multiLineObjectPrefab;
    [SerializeField] Shader meshShader;

    public GameObject mapObject;
    private GameObject[] nodes;

    public bool randomNodeSequence;
    public bool earClipTriangulating;
    public bool delaunayTriangulating;

    public bool createConnections;
    public bool produceEndMap;

    private void Start()
    {
        // Get the nodes on the map, random or sequenced, and use their positions
        GetNodes();

        // Run the triangulation algorithm and calculate
        Triangulate();

        // Create connections between points using TRI list
        if (createConnections)
        {
            ConnectionData[] connections = CreateConnections(null);
            connections = RemoveDoubleConnections(connections);
            DisplayConnections(connections);
        }

        // Create end result map
        if (produceEndMap)
        {
            Shape newShape = Triangulation.EarClipTriangulate(nodes);
            MapData newMap = new MapData(newShape, CreateConnections(newShape));

            newMap.CreateRouteLength();
            newMap.DisplayRoutes(multiLineObjectPrefab);

            for (int i = 0; i < newMap.mapConnections.Length; i++)
            {
                Debug.Log(newMap.mapConnections[i].GetRouteLength());
            }
        }
    }

    private void Update()
    {
        // Runtime-REDO -> Generate new mesh
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GameObject[] meshGameObjects = GameObject.FindGameObjectsWithTag("MeshObject");

            foreach (GameObject g in meshGameObjects)
                Destroy(g);

            GetNodes();
            Triangulate();
        }
    }

    private void GetNodes()
    {
        if (randomNodeSequence)
            nodes = mapObject.GetComponent<NodeRandomiser>().GetRandomNodeSequence();
        else
            nodes = mapObject.GetComponent<NodeSequence>().nodes;
    }

    private void Triangulate()
    {
        if (earClipTriangulating && !createConnections)
            EarClipTriangulateMeshSetup();

        if (delaunayTriangulating && !createConnections)
            DelaunayTriangulateMeshSetup();
    }

    private void EarClipTriangulateMeshSetup()
    {
        // Ear Clipping Triangulation Visualisation Method
        Shape shapeData = Triangulation.EarClipTriangulate(nodes);
        DisplayTriangleMeshes(shapeData);
    }

    private void DelaunayTriangulateMeshSetup()
    {
        // Delaunay Triangulation Visualisation Method
        Shape shapeData = Triangulation.DelaunayTriangulate(nodes);
        DisplayTriangleMeshes(shapeData);
    }

    private void DisplayTriangleMeshes(Shape shapeData)
    {
        Vector3[] vertexPositions = shapeData._3DVertexPositions();
        Material material = new Material(meshShader);

        for (int i = 0; i < shapeData.m_triangles.Length; i += 3)
        {
            // Create the mesh object
            GameObject triObject = Instantiate(meshObjectPrefab, Vector3.zero, Quaternion.identity);

            // Create the vertices array
            Vector3[] vertices = new Vector3[3];
            vertices[0] = vertexPositions[shapeData.m_triangles[i + 0]];
            vertices[1] = vertexPositions[shapeData.m_triangles[i + 1]];
            vertices[2] = vertexPositions[shapeData.m_triangles[i + 2]];

            // Create triangles array
            int[] triangles = new int[3] { 0, 1, 2 };

            // Apply the data to the objects script
            triObject.GetComponent<TriangleMeshMaker>().CreateTriangle(vertices, triangles, material, new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
        }
    }

    private ConnectionData[] CreateConnections(Shape shapeData)
    {
        if (shapeData == null)
        {
            if (earClipTriangulating)
                shapeData = Triangulation.EarClipTriangulate(nodes);
            if (delaunayTriangulating)
                shapeData = Triangulation.DelaunayTriangulate(nodes);
        }

        List<ConnectionData> connections = new List<ConnectionData>();

        for (int i = 0; i < shapeData.m_triangles.Length - 4; i += 3)
        {
            // Connect AB
            connections.Add(new ConnectionData(shapeData.m_triangles[i], shapeData.m_triangles[i + 1], shapeData.m_vertices[shapeData.m_triangles[i]], shapeData.m_vertices[shapeData.m_triangles[i + 1]]));

            // Connect AC
            connections.Add(new ConnectionData(shapeData.m_triangles[i], shapeData.m_triangles[i + 2], shapeData.m_vertices[shapeData.m_triangles[i]], shapeData.m_vertices[shapeData.m_triangles[i + 2]]));

            // Connect BC
            connections.Add(new ConnectionData(shapeData.m_triangles[i + 1], shapeData.m_triangles[i + 2], shapeData.m_vertices[shapeData.m_triangles[i + 1]], shapeData.m_vertices[shapeData.m_triangles[i + 2]]));
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

public class ConnectionData
{
    // World Position based location data
    public Vector2 aPointPosition;
    public Vector2 bPointPosition;
    public int aPointIndex;
    public int bPointIndex;

    // In-game collectible route data
    RouteData routeOne = new RouteData();
    RouteData routeTwo = new RouteData();

    public ConnectionData() { }

    public ConnectionData(int ia, int ib, Vector2 va, Vector2 vb)
    {
        aPointIndex = ia;
        bPointIndex = ib;

        aPointPosition = va;
        bPointPosition = vb;
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
        if ((other.aPointPosition == aPointPosition && other.bPointPosition == bPointPosition) || (other.bPointPosition == aPointPosition && other.aPointPosition == bPointPosition))
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
            routeOne.route_size = length;
        else
            routeTwo.route_size = length;
    }

    public int GetRouteLength(int routeNum = 1)
    {
        if (routeNum == 1)
            return routeOne.route_size;
        else
            return routeTwo.route_size;
    }

    public RouteData GetRoute(int routeNum = 1)
    {
        if (routeNum == 1)
            return routeOne;
        else
            return routeTwo;
    }
}

public class RouteData
{
    public int route_size;
    public Color route_colour;
    public bool route_taken;

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
    }
}

public class MapData
{
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

    public void DisplayRoutes(GameObject linePrefab)
    {
        Transform parentObject = new GameObject("Parent MultiLine Object").transform;

        for (int i = 0; i < mapConnections.Length; i++)
        {
            RouteData routeOne = mapConnections[i].GetRoute(1);
            RouteData routeTwo = mapConnections[i].GetRoute(2);

            Vector2 a = mapConnections[i].aPointPosition;
            Vector2 b = mapConnections[i].bPointPosition;

            if (routeOne.route_size != 0)
            {
                GameObject lineObject = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity, parentObject);
                lineObject.GetComponent<LineRendererSetup>().SetupLine(routeOne.route_size, routeOne.route_colour, new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
            }

            if (routeTwo.route_size != 0)
            {
                GameObject lineObject = GameObject.Instantiate(linePrefab, Vector3.zero, Quaternion.identity, parentObject);
                lineObject.GetComponent<LineRendererSetup>().SetupLine(routeTwo.route_size, routeTwo.route_colour, new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
            }
        }
    }
}