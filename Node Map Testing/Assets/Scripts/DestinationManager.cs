using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DestinationManager
{
    public DestinationData[] GetDestinations(GameObject[] nodes, MapData map)
    {
        List<DestinationData> destinations = new List<DestinationData>();

        // Get positions of node
        Vector2[] points = Triangulation.NodesToPositions(nodes);

        // List of all possible locations N number of times, where N = total routes accessible
        List<Vector2> possibleLocations = new List<Vector2>();

        // Loop through each point
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];

            // Loop through all map connections (i.e. routes)
            for (int j = 0; j < map.mapConnections.Length; j++)
            {
                ConnectionData currentConnection = map.mapConnections[j];

                if (currentConnection.SharesVertex(currentPoint))
                {
                    if (currentConnection.IsBothActive())
                    {
                        possibleLocations.Add(currentPoint);
                        possibleLocations.Add(currentPoint);
                    }
                    else if (currentConnection.IsOneActive())
                    {
                        possibleLocations.Add(currentPoint);
                    }
                }
            }
        }

        // Now make the destinations
        for (int i = 0; i < GameManager.MAX_DESTINATIONS; i++)
        {
            int randomStartIndex = UnityEngine.Random.Range(0, possibleLocations.Count);
            Vector2 startPos = possibleLocations[randomStartIndex];
            possibleLocations.RemoveAt(randomStartIndex);

            Vector2[] possibleMatches = GetAllNonDupes(possibleLocations, startPos);
            int randomFinishIndex = UnityEngine.Random.Range(0, possibleMatches.Length);
            Vector2 finishPos = possibleMatches[randomFinishIndex];
            possibleLocations.Remove(finishPos);

            DestinationData newDestination = new DestinationData(startPos, finishPos);
            destinations.Add(newDestination);
        }

        // Name the destinations
        for (int i = 0; i < destinations.Count; i++)
        {
            destinations[i].SetNames(GetNameFromPosition(nodes, destinations[i].a), GetNameFromPosition(nodes, destinations[i].b));
        }

        return destinations.ToArray();
    }

    private Vector2[] GetAllNonDupes(List<Vector2> list, Vector2 target)
    {
        List<Vector2> nonDupes = new List<Vector2>();

        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].Equals(target))
                nonDupes.Add(list[i]);
        }

        return nonDupes.ToArray();
    }

    private string GetNameFromPosition(GameObject[] nodes, Vector2 position)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].transform.position == new Vector3(position.x, 0, position.y))
            {
                return nodes[i].GetComponent<NodeMapMarker>().GetName();
            }
        }

        return "NULL";
    }
}

public class DestinationData
{
    public string start;
    public Vector2 a;

    public string finish;
    public Vector2 b;

    public int points;

    public DestinationData() { }

    public DestinationData(Vector2 start, Vector2 finish)
    {
        a = start;
        b = finish;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
            return false;
        else
        {
            DestinationData dd = (DestinationData)obj;
            return this.Equal(dd);
        }
    }

    public bool Equal(DestinationData other)
    {
        if ((this.a == other.a && this.b == other.b) || (this.b == other.a && this.a == other.b))
            return true;
        else
            return false;
    }

    public override int GetHashCode()
    {
        return a.GetHashCode() ^ b.GetHashCode() ^ start.GetHashCode() ^ finish.GetHashCode() ^ points.GetHashCode();
    }

    public void SetNames(string startName, string finishName)
    {
        start = startName;
        finish = finishName;
    }

    public void SetPointValue(int pointValue)
    {
        points = pointValue;
    }
}
