using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject lineObjectPrefab;
    [SerializeField] GameObject meshObjectPrefab;
    [SerializeField] Shader meshShader;

    public GameObject mapObject;
    private GameObject[] nodes;

    public bool randomNodeSequence;
    public bool earClipTriangulating;
    public bool delaunayTriangulating;

    public bool createConnections;

    private void Start()
    {
        // Get the nodes on the map, random or sequenced, and use their positions
        GetNodes();

        // Run the triangulation algorithm and calculate
        Triangulate();

        // Create connections between points using TRI list
        if (createConnections)
        {
            ConnectionData[] connections = CreateConnections();
            connections = RemoveDoubleConnections(connections);
            DisplayConnections(connections);
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
        if (earClipTriangulating)
            EarClipTriangulateMeshSetup();

        if (delaunayTriangulating)
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

    private ConnectionData[] CreateConnections()
    {
        Shape shapeData = Triangulation.EarClipTriangulate(nodes);

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
    public Vector2 aPointPosition;
    public Vector2 bPointPosition;
    public int aPointIndex;
    public int bPointIndex;

    public int sizeOfConnection;

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
        if (other.aPointPosition == aPointPosition && other.bPointPosition == bPointPosition)
            return true;
        else
            return false;
    }
}