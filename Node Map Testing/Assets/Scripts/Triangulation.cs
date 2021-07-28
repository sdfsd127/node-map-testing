#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Triangulation
{
    public static Vector2[] NodesToPositions(GameObject[] nodes)
    {
        // Create list of positions for the vertices using the nodes
        Vector2[] vertexPositions = new Vector2[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            Vector3 nodePosition = nodes[i].transform.position;
            vertexPositions[i] = new Vector2(nodePosition.x, nodePosition.z);
        }

        return vertexPositions;
    }

    public static Shape DelaunayTriangulate(Vector2[] vertexPositions)
    {
        // Create empty and malleable list of triangles
        List<TriangleData> triangles = new List<TriangleData>();

        // Determine super triangle
        TriangleData superTriangle = FindSuperTriangle(vertexPositions);

        // Add the super triangle to the triangle list
        triangles.Add(superTriangle);

        // Loop through each point
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            Vector2 currentVertex = vertexPositions[i];
            List<TriangleData> badTriangles = new List<TriangleData>();

            // Loop through each current triangle
            for (int j = 0; j < triangles.Count; j++)
            {
                TriangleData currentTriangle = triangles[j];

                // Calculate and determine the circumcircle of the current triangle
                float circleRadius;
                Vector2 circleCentre;
                GetCircumcircleData(currentTriangle, out circleRadius, out circleCentre);

                if (IsVertexInsideCircumcircle(currentVertex, circleRadius, circleCentre)) // Check if current point is in current triangle circumcircle
                {
                    badTriangles.Add(currentTriangle);
                }
            }

            // Loop through each bad triangle and populate edge list
            List<EdgeData> edges = new List<EdgeData>();
            for (int j = 0; j < badTriangles.Count; j++)
            {
                TriangleData currentBadTri = badTriangles[j];

                // AB Edge
                edges.Add(new EdgeData(currentBadTri.positionA, currentBadTri.positionB));

                // AC Edge
                edges.Add(new EdgeData(currentBadTri.positionA, currentBadTri.positionC));

                // BC Edge
                edges.Add(new EdgeData(currentBadTri.positionB, currentBadTri.positionC));
            }

            // Loop through edges and find non-duplicates
            List<EdgeData> polygon = RemoveDoublesFromList(edges);

            // Remove bad triangles from triangle list
            for (int j = 0; j < badTriangles.Count; j++)
            {
                triangles = RemoveTriangleFromList(triangles, badTriangles[j]);
            }

            // Loop through each edge in polygon and re-triangulate it with the current point
            for (int j = 0; j < polygon.Count; j++)
            {
                EdgeData currentEdge = polygon[j];
                TriangleData newTriangle = new TriangleData(currentVertex, currentEdge.positionA, currentEdge.positionB);
                triangles.Add(newTriangle);
            }
        }

        // Finished inserting each point, now remove the supertriangle
        triangles = RemoveSuperTriangleReliantTriangles(triangles, superTriangle);

        // Create a Shape object to return
        return new Shape(vertexPositions, triangles.ToArray());
    }

    public static bool IsVertexInsideTriangle(Vector2 vertex, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 a_Heading_B = b - a;
        Vector2 b_Heading_C = c - b;
        Vector2 c_Heading_A = a - c;

        Vector2 a_Heading_Vertex = vertex - a;
        Vector2 b_Heading_Vertex = vertex - b;
        Vector2 c_Heading_Vertex = vertex - c;

        float cross1 = Utils.CrossProductMagnitude(a_Heading_B, a_Heading_Vertex);
        float cross2 = Utils.CrossProductMagnitude(b_Heading_C, b_Heading_Vertex);
        float cross3 = Utils.CrossProductMagnitude(c_Heading_A, c_Heading_Vertex);

        if (cross1 > 0.0f || cross2 > 0.0f || cross3 > 0.0f)
            return false;
        else
            return true;
    }

    public static TriangleData FindSuperTriangle(Vector2[] vertices, float scaleModifier = 1.0f)
    {
        Vector2 pointA, pointB, pointC;

        // Find min and max positions of vertices
        float x_min = float.MaxValue, x_max = float.MinValue;
        float y_min = float.MaxValue, y_max = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 currentVertice = vertices[i];

            if (currentVertice.x < x_min)
                x_min = currentVertice.x;
            else if (currentVertice.x > x_max)
                x_max = currentVertice.x;

            if (currentVertice.y < y_min)
                y_min = currentVertice.y;
            else if (currentVertice.y > y_max)
                y_max = currentVertice.y;
        }

        // Use that as a rectangle shape for our triangle to superseed
        pointA = new Vector2((x_min + x_max) / 2, y_max + (4 * scaleModifier));
        pointB = new Vector2(x_min - (4 * scaleModifier), y_min);
        pointC = new Vector2(x_max + (4 * scaleModifier), y_min);

        return new TriangleData(pointA, pointB, pointC);
    }

    public static void GetCircumcircleData(TriangleData triangle, out float radius, out Vector2 centre)
    {
        // Create easy reference vectors for A, B and C
        Vector2 a = triangle.positionA;
        Vector2 b = triangle.positionB;
        Vector2 c = triangle.positionC;

        // Create vectors for AB and AC
        Vector2 ab = b - a;
        Vector2 ac = c - a;

        // Find midpoints of AB and AC
        Vector2 abMidpoint = new Vector2((a.x + b.x) / 2, (a.y + b.y) / 2);
        Vector2 acMidpoint = new Vector2((a.x + c.x) / 2, (a.y + c.y) / 2);

        // Find slope of AB and AC
        float abSlope = (b.y - a.y) / (b.x - a.x);
        float acSlope = (c.y - a.y) / (c.x - a.x);

        // Invert to find perpendicular slope
        float abPerpSlope = -1 / abSlope;
        float acPerpSlope = -1 / acSlope;

        // Solve the equation y = mx + c, substituting in the known values of m, x, and y to solve for c.
        // y - mx = c
        float abPerpSolve = abMidpoint.y - (abPerpSlope * abMidpoint.x);
        float acPerpSolve = acMidpoint.y - (acPerpSlope * acMidpoint.x);

        // Now we have two equations in the form y = mx + c
        // y = (abPerpSlope * x) + abPerpSolve
        // y = (acPerpSlope * x) + acPerpSolve
        // (abPerpSlope * x) + abPerpSolve = (acPerpSlope * x) + acPerpSolve
        // abPerpSolve = ((acPerpSlope - abPerpSlope) * x) + acPerpSolve
        // abPerpSolve - acPerpSolve = (acPerpSlope - abPerpSlope) * x
        // (abPerpSolve - acPerpSolve) / (acPerpSlope - abPerpSlope) = x
        float xSolve = (abPerpSolve - acPerpSolve) / (acPerpSlope - abPerpSlope);
        float ySolve = (abPerpSlope * xSolve) + abPerpSolve;

        // Set found coordinates to centre
        centre = new Vector2(xSolve, ySolve);
        // Measure distance between centre and point A
        radius = Vector2.Distance(centre, a);
    }

    public static bool IsVertexInsideCircumcircle(Vector2 vertex, float circleRadius, Vector2 circleCentre)
    {
        // Create a vector heading from circle centre to vertex
        Vector2 centre_Heading_V = vertex - circleCentre;

        // Compare the size of the vector to the circles radius
        float distance = centre_Heading_V.magnitude;
        if (distance > circleRadius)
            return false;
        else
            return true;
    }

    public static List<TriangleData> RemoveTriangleFromList(List<TriangleData> triList, TriangleData target)
    {
        for (int i = 0; i < triList.Count; i++)
        {
            if (target.Equals(triList[i]))
            {
                triList.RemoveAt(i);
                break;
            }
        }

        return triList;
    }

    public static List<EdgeData> RemoveDoublesFromList(List<EdgeData> edgeList)
    {
        // Remember the positions of the elements to remove
        List<int> indexToRemove = new List<int>();

        // Loop through each edge and compare it with each edge
        for (int i = 0; i < edgeList.Count; i++)
        {
            for (int j = 0; j < edgeList.Count; j++)
            {
                if (i == j)
                    continue;

                // Keep track of the ones that are duplicates
                if (edgeList[i].Equals(edgeList[j]))
                {
                    if (!indexToRemove.Contains(i))
                        indexToRemove.Add(i);
                    if (!indexToRemove.Contains(j))
                        indexToRemove.Add(j);
                }
            }
        }

        // Sort it to ascending order and reverse it
        indexToRemove.Sort();
        indexToRemove.Reverse();

        // Now remove from the list in descending order
        for (int i = 0; i < indexToRemove.Count; i++)
            edgeList.RemoveAt(indexToRemove[i]);

        // Return the modified list
        return edgeList;
    }

    public static List<TriangleData> RemoveSuperTriangleReliantTriangles(List<TriangleData> triList, TriangleData superTriangle)
    {
        List<int> indexToRemove = new List<int>();

        for (int i = 0; i < triList.Count; i++)
        {
            TriangleData currentTriangle = triList[i];

            if (currentTriangle.SharesVertex(superTriangle))
                indexToRemove.Add(i);
        }

        indexToRemove.Sort();
        indexToRemove.Reverse();

        for (int i = 0; i < indexToRemove.Count; i++)
            triList.RemoveAt(indexToRemove[i]);

        return triList;
    }
}

public class Shape
{
    public Vector2[] m_vertices;
    public TriangleData[] m_triangles;

    public Shape() { }

    public Shape(Vector2[] vertices)
    {
        m_vertices = vertices;
    }

    public Shape(TriangleData[] triangles)
    {
        m_triangles = triangles;
    }

    public Shape(Vector2[] vertices, TriangleData[] triangles)
    {
        m_vertices = vertices;
        m_triangles = triangles;
    }

    public Vector3[] _3DVertexPositions()
    {
        Vector3[] array = new Vector3[m_vertices.Length];
        for (int i = 0; i < m_vertices.Length; i++)
            array[i] = new Vector3(m_vertices[i].x, 0, m_vertices[i].y);

        return array;
    }
}


public class TriangleData
{
    public Vector2 positionA, positionB, positionC;

    public TriangleData() { }

    public TriangleData(Vector2 pa, Vector2 pb, Vector2 pc)
    {
        positionA = pa;
        positionB = pb;
        positionC = pc;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
            return false;
        else
        {
            TriangleData td = (TriangleData)obj;
            return this.Equal(td);
        }
    }

    public bool Equal(TriangleData other)
    {
        if (this.positionA == other.positionA && this.positionB == other.positionB && this.positionC == other.positionC)
            return true;
        else
            return false;
    }

    public Vector2[] GetVertexArray2D()
    {
        return new Vector2[] { positionA, positionB, positionC };
    }

    public Vector3[] GetVertexArray3D()
    {
        return new Vector3[] { new Vector3(positionA.x, 0, positionA.y), new Vector3(positionB.x, 0, positionB.y), new Vector3(positionC.x, 0, positionC.y) };
    }

    public bool SharesVertex(TriangleData otherTri)
    {
        // Check if this has any vertex shared with other tri
        if (otherTri.positionA == positionA || otherTri.positionA == positionB || otherTri.positionA == positionC)
            return true;
        if (otherTri.positionB == positionA || otherTri.positionB == positionB || otherTri.positionB == positionC)
            return true;
        if (otherTri.positionC == positionA || otherTri.positionC == positionB || otherTri.positionC == positionC)
            return true;

        return false;
    }

    public bool SharesMultipleVertex(TriangleData otherTri)
    {
        int vertexSharedNum = 0;

        if (otherTri.positionA == positionA || otherTri.positionA == positionB || otherTri.positionA == positionC)
            vertexSharedNum++;
        if (otherTri.positionB == positionA || otherTri.positionB == positionB || otherTri.positionB == positionC)
            vertexSharedNum++;
        if (otherTri.positionC == positionA || otherTri.positionC == positionB || otherTri.positionC == positionC)
            vertexSharedNum++;

        if (vertexSharedNum > 1)
            return true;
        else
            return false;
    }

    public Vector2 GetUniqueVertex(Vector2 a, Vector2 b)
    {
        if (positionA != a && positionA != b)
            return positionA;
        else if (positionB != a && positionB != b)
            return positionB;
        else if (positionC != a && positionC != b)
            return positionC;
        else
            return Vector2.zero;
    }
}

public class EdgeData
{
    public Vector2 positionA, positionB;

    public EdgeData() { }

    public EdgeData(Vector2 pa, Vector2 pb)
    {
        positionA = pa;
        positionB = pb;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
            return false;
        else
        {
            EdgeData ed = (EdgeData)obj;
            return this.Equal(ed);
        }
    }

    public bool Equal(EdgeData other)
    {
        if (this.positionA == other.positionA && this.positionB == other.positionB)
            return true;
        else
            return false;
    }
}

#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()