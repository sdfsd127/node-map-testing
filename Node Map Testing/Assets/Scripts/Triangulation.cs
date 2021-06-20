using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation : MonoBehaviour
{
    public void Triangulate(GameObject[] nodes)
    {
        // Create empty list of vertex indexes and populate them with their ID in ascending order
        List<int> indexList = new List<int>();
        for (int i = 0; i < nodes.Length; i++)
            indexList.Add(i);

        // Create empty list of triangles that will be populated through the algorithm
        List<int> triangles = new List<int>();

        // Get an array of the vertex positions from the nodes
        Vector2[] vertexPositions = new Vector2[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
            vertexPositions[i] = nodes[i].GetComponent<NodeMapMarker>().GetPosition();

        // Repeat algorithm until only 3 vertices are left in the list
        // Infinite loop safety variable included
        int loopCount = 0;
        while (indexList.Count > 3 && loopCount < 1000)
        {
            for (int i = 0; i < indexList.Count; i++)
            {
                int pointA = indexList[i];
                int pointB = GetPreviousIndex(indexList, i);
                int pointC = GetNextIndex(indexList, i);
                Debug.Log(pointA + " " + pointB + " " + pointC);

                Vector2 vA = vertexPositions[pointA];
                Vector2 vB = vertexPositions[pointB];
                Vector2 vC = vertexPositions[pointC];
                Debug.Log(vA + " " + vB + " " + vC);

                Vector2 a_Heading_B = vB - vA;
                Vector2 a_Heading_C = vC - vA;

                if (Vector3.Cross(new Vector3(a_Heading_B.x, 0, a_Heading_B.y), new Vector3(a_Heading_C.x, 0, a_Heading_C.y)).magnitude < 0.0f)
                {

                }
            }

            loopCount++;
        }

        // Add the last triangle from the remaining vertices
        triangles.Add(indexList[0]);
        triangles.Add(indexList[1]);
        triangles.Add(indexList[2]);
    }

    public static int GetPreviousIndex(List<int> indexList, int currentIndex)
    {
        if (currentIndex - 1 < 0)
            return indexList[indexList.Count - 1];
        else
            return indexList[currentIndex - 1];
    }

    public static int GetNextIndex(List<int> indexList, int currentIndex)
    {
        if (currentIndex + 1 < indexList.Count - 1)
            return indexList[0];
        else
            return indexList[currentIndex + 1];
    }
}
