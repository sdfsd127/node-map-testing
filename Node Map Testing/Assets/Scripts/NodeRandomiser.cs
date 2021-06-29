using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeRandomiser : MonoBehaviour
{
    [SerializeField] GameObject[] nodes;

    public GameObject[] GetRandomNodeSequence()
    {
        // Create empty array of nodes to be populated
        GameObject[] nodeArray = new GameObject[nodes.Length];

        // List of index to nodes to choose randomly from
        List<int> nodeIndices = new List<int>();
        for (int i = 0; i < nodes.Length; i++)
            nodeIndices.Add(i);

        int counter = 0;
        while (nodeIndices.Count > 0)
        {
            int randomIndex = nodeIndices[Random.Range(0, nodeIndices.Count)];
            nodeIndices.Remove(randomIndex);

            nodeArray[counter] = nodes[randomIndex];
            counter++;
        }

        return nodeArray;
    }
}
