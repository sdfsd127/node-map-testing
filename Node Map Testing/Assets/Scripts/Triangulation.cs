using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Triangulation
{
    public static Shape EarClipTriangulate(GameObject[] nodes)
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
            vertexPositions[i] = new Vector2(nodes[i].transform.position.x, nodes[i].transform.position.z);

        // Repeat algorithm until only 3 vertices are left in the list
        // Infinite loop safety variable included
        int loopCount = 0;
        while (indexList.Count > 3 && loopCount < 1000)
        {
            for (int i = 0; i < indexList.Count; i++)
            {
                // Fetch 3 vertex indices used to make triangle
                int pointA = indexList[i];
                int pointB = Utils.GetItem(indexList, i - 1);
                int pointC = Utils.GetItem(indexList, i + 1);

                // Fetch the positions of the vertices
                Vector2 vA = vertexPositions[pointA];
                Vector2 vB = vertexPositions[pointB];
                Vector2 vC = vertexPositions[pointC];

                // Create heading vectors for cross product checks
                Vector2 a_Heading_B = vB - vA;
                Vector2 a_Heading_C = vC - vA;

                // Check for a 180 or greater degree angle
                if (Utils.CrossProductMagnitude(a_Heading_B, a_Heading_C) < 0.0f)
                    continue;

                // Begin check on all vertex to see if any are inside the triangle
                bool earTriangleFound = true;

                for (int j = 0; j < vertexPositions.Length; j++)
                {
                    // Ignore the vertices checking themselves
                    if (j == pointA || j == pointB || j == pointC)
                        continue;

                    if (IsVertexInsideTriangle(vertexPositions[j], vB, vA, vC)) // Clockwise, B, A, C
                    {
                        // If inside, this is not an ear piece triangle
                        earTriangleFound = false;
                        break;
                    }
                }

                // Check if the triangle was found to be an ear piece or not
                if (earTriangleFound)
                {
                    // If found add the triangle and remove the origin (A) point from the index list
                    // Clockwise order, B, A, C
                    triangles.Add(pointB);
                    triangles.Add(pointA);
                    triangles.Add(pointC);

                    indexList.RemoveAt(i);
                    break;
                }
            }

            loopCount++;
        }

        // Add the last triangle from the remaining vertices
        if (indexList.Count == 3)
        {
            triangles.Add(indexList[0]);
            triangles.Add(indexList[1]);
            triangles.Add(indexList[2]);
        }

        // Create a TriangleSet object to return
        Shape triangleSet = new Shape(vertexPositions, triangles.ToArray());
        return triangleSet;
    }

    public static Shape DelaunayTriangulate(GameObject[] nodes)
    {
        // Create list of positions for the vertices using the nodes
        Vector2[] vertexPositions = new Vector2[nodes.Length + 3]; // 3 Extra spaces for supertriangle
        for (int i = 0; i < nodes.Length; i++)
        {
            Vector3 nodePosition = nodes[i].transform.position;
            vertexPositions[i] = new Vector2(nodePosition.x, nodePosition.z);
        }

        // Create empty and malleable list
        List<TriangleData> triangles = new List<TriangleData>();

        // Determine super triangle
        TriangleData superTriangle = FindSuperTriangle(vertexPositions);

        // Add super triangle vertices to end of vertex list
        vertexPositions[nodes.Length + 0] = superTriangle.positionA;
        vertexPositions[nodes.Length + 1] = superTriangle.positionB;
        vertexPositions[nodes.Length + 2] = superTriangle.positionC;

        // Update the index to match the new position in the array for vertices
        superTriangle.SetTriangleArray(new int[] { nodes.Length + 1, nodes.Length + 2, nodes.Length + 3 });
        // Add the triangle to the triangle list
        triangles.Add(superTriangle);

        // Loop through each point in the vertex list
        for (int i = 0; i < vertexPositions.Length; i++)
        {
            // Create empty edge list
            List<EdgeData> edges = new List<EdgeData>();

            // Loop through all triangles in triangle list
            for (int t = 0; t < triangles.Count; t++)
            {
                TriangleData currentTriangle = triangles[t];

                // Calculate and determine the circumcircle of the current triangle
                float circleRadius;
                Vector2 circleCentre;
                GetCircumcircleData(currentTriangle, out circleRadius, out circleCentre);

                // Check if the current vertex is inside the current triangles circumcircle
                if (IsVertexInsideCircumcircle(vertexPositions[i], circleRadius, circleCentre))
                {
                    // Add the three triangle edges to the edge buffer
                    // Edges AB, AC, and BC
                    edges.Add(new EdgeData(currentTriangle.indexA, currentTriangle.indexB, currentTriangle.positionA, currentTriangle.positionB));
                    edges.Add(new EdgeData(currentTriangle.indexA, currentTriangle.indexC, currentTriangle.positionA, currentTriangle.positionC));
                    edges.Add(new EdgeData(currentTriangle.indexB, currentTriangle.indexC, currentTriangle.positionB, currentTriangle.positionC));

                    // Remove the triangle from the triangle list
                    triangles = RemoveTriangleFromList(triangles, currentTriangle);
                }
            }

            // Remove all double specified edges
            edges = RemoveDoublesFromList(edges);

            // Connect the current point with each edge in the edge list to form triangles and add them to the list
            for (int j = 0; j < edges.Count; j++)
            {
                EdgeData currentEdge = edges[j];
                TriangleData newTriangle = new TriangleData(i, currentEdge.indexA, currentEdge.indexB, vertexPositions[i], currentEdge.positionA, currentEdge.positionB);
                triangles.Add(newTriangle);
            }
        }

        // Remove any triangles using the supertriangle from the triangle list
        int superTriangleIndex = nodes.Length;
        triangles = RemoveSuperTriangleReliantTriangles(triangles, superTriangleIndex);

        // Remove the super triangle vertices from the vertex list
        Array.Resize<Vector2>(ref vertexPositions, nodes.Length);

        // Create a TriangleSet object to return
        int[] triangleIndices = new int[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++)
        {
            TriangleData currentTriangle = triangles[i];
            triangleIndices[(i * 3) + 0] = currentTriangle.indexA;
            triangleIndices[(i * 3) + 1] = currentTriangle.indexB;
            triangleIndices[(i * 3) + 2] = currentTriangle.indexC;
        }

        Shape triangleSet = new Shape(vertexPositions, triangleIndices);
        return triangleSet;
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

        return new TriangleData(0, 1, 2, pointA, pointB, pointC);
    }

    public static void GetCircumcircleData(TriangleData triangle, out float radius, out Vector2 centre)
    {
        // Default settings
        radius = 0.0f;
        centre = new Vector2(0, 0);

        // 2(xA - xB)X + 2(yA - yB)Y = (xA^2 + yA^2) - (xB^2 + yB^2)     
        float eq1lhsXCoeff = 2 * (triangle.positionA.x - triangle.positionB.x); // Equation 1, LHS, X
        float eq1lhsYCoeff = 2 * (triangle.positionA.y - triangle.positionB.y); // Equation 1, LHS, Y
        float eq1rhsSum = ((triangle.positionA.x * triangle.positionA.x) + (triangle.positionA.y * triangle.positionA.y)) - 
                        ((triangle.positionB.x * triangle.positionB.x) + (triangle.positionB.y * triangle.positionB.y));

        // 2(xA - xC)X + 2(yA - yC)Y = (xA^2 + yA^2) - (xC^2 + yC^2)
        float eq2lhsXCoeff = 2 * (triangle.positionA.x - triangle.positionC.x); // Equation 2, LHS, X
        float eq2lhsYCoeff = 2 * (triangle.positionA.y - triangle.positionC.y); // Equation 2, LHS, Y
        float eq2rhsSum = ((triangle.positionA.x * triangle.positionA.x) + (triangle.positionA.y * triangle.positionA.y)) -
                        ((triangle.positionC.x * triangle.positionC.x) + (triangle.positionC.y * triangle.positionC.y));

        // This gives 2 equations
        // eq1lhsXCoeff(X) + eq1lhsYCoeff(Y) = eq1rhsSum
        // eq1rhsXCoeff(X) + eq1rhsYCoeff(Y) = eq2rhsSum
        // Find a common multiple to multiply both equations by to cancel out one unknown
        Vector2 multiples;
        if (Utils.FindMultiple(new Vector2(eq1lhsXCoeff, eq1lhsYCoeff), new Vector2(eq2lhsXCoeff, eq2lhsYCoeff), out multiples))
        {
            Debug.Log(multiples);

            // Create new equations that have been multiplied
            float eq3XCoeff = eq1lhsXCoeff * multiples.x;
            float eq3YCoeff = eq1lhsYCoeff * multiples.x;
            float eq3Sum = eq1rhsSum * multiples.x;

            float eq4XCoeff = eq2lhsXCoeff * multiples.y;
            float eq4YCoeff = eq2lhsYCoeff * multiples.y;
            float eq4Sum = eq2rhsSum * multiples.y;

            // Subtract one from another to get the solving equation
            float eq5XCoeff = eq3XCoeff - eq4XCoeff;
            float eq5YCoeff = eq3YCoeff - eq4YCoeff;
            float eq5Sum = eq3Sum - eq4Sum;

            // Init final value of unknowns
            float xValue = 0.0f;
            float yValue = 0.0f;

            // Find which unknown we are solving for first
            if (eq5XCoeff == 0)
            {
                // Solve Y then X
                yValue = eq5Sum / eq5YCoeff;
                float trueSum = eq1rhsSum - (eq1lhsYCoeff * yValue);
                xValue = trueSum / eq1lhsXCoeff;
            }
            else if (eq5YCoeff == 0)
            {
                // Solve X then Y
                xValue = eq5Sum / eq5XCoeff;
                float trueSum = eq1rhsSum - (eq1lhsXCoeff * xValue);
                yValue = trueSum / eq1lhsYCoeff;
            }
            else
            {
                Debug.Log("ERROR: X:" + eq5XCoeff + ", Y:" + eq5YCoeff);
            }

            Debug.Log("X: " + xValue + ", Y: " + yValue);
            centre = new Vector2(xValue, yValue);

            // Use Equation (X - aX)^2 + (Y - aY)^2 = R^2
            // Find radius by plugging 1 into x and y
            float lhsX = (1 - triangle.positionA.x) * (1 - triangle.positionA.x);
            float lhsY = (1 - triangle.positionA.y) * (1 - triangle.positionA.y);
            float rSquared = lhsX + lhsY;
            Debug.Log("R Squared: " + rSquared);

            radius = Mathf.Sqrt(rSquared);
            Debug.Log("R: " + radius);
        }
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

    public static List<TriangleData> RemoveSuperTriangleReliantTriangles(List<TriangleData> triList, int superTriangleIndex)
    {
        List<int> indexToRemove = new List<int>();

        for (int i = 0; i < triList.Count; i++)
        {
            TriangleData currentTriangle = triList[i];

            if (currentTriangle.indexA >= superTriangleIndex || currentTriangle.indexB >= superTriangleIndex || currentTriangle.indexC >= superTriangleIndex)
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
    public int[] m_triangles;

    public Shape() { }

    public Shape(Vector2[] vertices)
    {
        m_vertices = vertices;
    }

    public Shape(int[] triangles)
    {
        m_triangles = triangles;
    }

    public Shape(Vector2[] vertices, int[] triangles)
    {
        m_vertices = vertices;
        m_triangles = triangles;
    }

    public Vector3[] _3DVertexPositions()
    {
        Vector3[] array = new Vector3[m_vertices.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = new Vector3(m_vertices[i].x, 0, m_vertices[i].y);

        return array;
    }
}

public class TriangleData
{
    public int indexA, indexB, indexC;
    public Vector2 positionA, positionB, positionC;

    public TriangleData() { }

    public TriangleData(int ia, int ib, int ic, Vector2 pa, Vector2 pb, Vector2 pc)
    {
        indexA = ia;
        indexB = ib;
        indexC = ic;

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

    public void SetTriangleArray(int[] array)
    {
        indexA = array[0];
        indexB = array[1];
        indexC = array[2];
    }

    public int[] GetTriangleArray()
    {
        return new int[] { indexA, indexB, indexC };
    }

    public int[] GetEmptyTriangleArray()
    {
        return new int[] { 0, 1, 2 };
    }
}

public class EdgeData
{
    public int indexA, indexB;
    public Vector2 positionA, positionB;

    public EdgeData() { }

    public EdgeData(int ia, int ib, Vector2 pa, Vector2 pb)
    {
        indexA = ia;
        indexB = ib;

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