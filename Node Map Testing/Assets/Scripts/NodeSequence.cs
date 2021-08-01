using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSequence : MonoBehaviour
{
    public bool randomNodeSequence;

    public GameObject[] nodes;

    public GameObject[] GetNodes()
    {
        if (randomNodeSequence)
            return GetRandomNodeSequence();
        else
            return nodes;
    }

    private GameObject[] GetRandomNodeSequence()
    {
        // Create empty array of nodes to be populated
        GameObject[] nodeArray = new GameObject[nodes.Length];

        // List of index to nodes to choose randomly from
        List<int> nodeIndices = new List<int>();
        for (int i = 0; i < nodes.Length; i++)
            nodeIndices.Add(i);

        for (int i = 0; i < nodes.Length; i++)
        {
            int randomIndex = nodeIndices[Random.Range(0, nodeIndices.Count)];
            nodeArray[i] = nodes[randomIndex];

            nodeIndices.Remove(randomIndex);
        }

        return nodeArray;
    }
}
