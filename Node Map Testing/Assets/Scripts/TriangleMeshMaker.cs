using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleMeshMaker : MonoBehaviour
{
    public void CreateTriangle(Vector3[] vertices, int[] triangles, Material material, Color colour)
    {
        Mesh newMesh = new Mesh();

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;

        Color[] newColours = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            newColours[i] = colour;
        newMesh.colors = newColours;

        this.gameObject.GetComponent<MeshFilter>().mesh = newMesh;
        this.gameObject.GetComponent<MeshRenderer>().material = material;
    }
}
