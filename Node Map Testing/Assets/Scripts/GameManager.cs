using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject meshObjectPrefab;
    [SerializeField] Shader meshShader;

    public GameObject mapObject;
    private GameObject[] nodes;

    public bool randomNodeSequence;
    public bool earClipTriangulating;
    public bool delaunayTriangulating;

    private void Start()
    {
        // Get the nodes on the map, random or sequenced, and use their positions
        GetNodes();

        // Run the triangulation algorithm and calculate
        Triangulate();
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

    /*
    private void TestCircumcircle()
    {
        Vector2 pointA = new Vector2(2, 3);
        Vector2 pointB = new Vector2(1, 1);
        Vector2 pointC = new Vector2(3, 1);

        GameObject pointAObj = new GameObject("OBJECT A");
        pointAObj.transform.position = new Vector3(pointA.x, 0, pointA.y);
        GameObject pointBObj = new GameObject("OBJECT B");
        pointBObj.transform.position = new Vector3(pointB.x, 0, pointB.y);
        GameObject pointCObj = new GameObject("OBJECT C");
        pointCObj.transform.position = new Vector3(pointC.x, 0, pointC.y);

        Triangulation.GetCircumcircleData(new TriangleData(0, 1, 2, pointA, pointB, pointC), out radius, out centre);

        ready = true;
    }

    private Vector2 centre;
    private float radius;
    public bool ready;

    private void OnDrawGizmos()
    {
        if (circumcircleTesting && ready)
        {
            Gizmos.DrawSphere(new Vector3(centre.x, 0, centre.y), radius);
        }
    }
    */
}
