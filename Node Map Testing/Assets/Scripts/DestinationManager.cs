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

        // Loop through each destination
        for (int i = 0; i < destinations.Count; i++)
        {
            // Name the destinations
            destinations[i].SetNames(GetNameFromPosition(nodes, destinations[i].a), GetNameFromPosition(nodes, destinations[i].b));
            // Calculate and Assign values to the destinations
            destinations[i].SetPointValue(GetPointValue(destinations[i].a, destinations[i].b, map));
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

    public static int GetPointValue(Vector2 pointA, Vector2 pointB, MapData map)
    {
        Journey shortestJourney = GetShortestPathBetweenPoints(pointA, pointB, map);
        return shortestJourney.currentCost;
    }

    public static Journey GetShortestPathBetweenPoints(Vector2 a, Vector2 b, MapData map)
    {
        // Remember current shortest
        int currentShortestLength = 100;

        // List to populate of journeyed routes
        List<Journey> finishedJourneys = new List<Journey>();
        List<Journey> exploringJourneys = new List<Journey>();

        List<Journey> toRemove = new List<Journey>();

        // Create new journey to evaluate
        Journey initialJourney = new Journey(a); // Start at point A
        exploringJourneys.Add(initialJourney);

        // Loop through and simulate the journeys to find the shortest path
        while (exploringJourneys.Count > 0)
        {
            // Loop through all active journeys
            int loops = exploringJourneys.Count;
            for (int i = 0; i < loops; i++)
            {
                // Find the current journey from the list
                Journey currentJourney = exploringJourneys[i];

                // If not at the end, search other connecting routes
                if (currentJourney.currentPosition != b)
                {
                    // Find all connections from journey current position point
                    ConnectionData[] connections = map.GetAllConnectionsWithPoint(currentJourney.currentPosition);
                    connections = RemoveConnectionsContainingVisitedPoints(connections, currentJourney);

                    // Loop through all found connections
                    for (int j = 0; j < connections.Length; j++)
                    {
                        // Create a new journey at the other end of the connection
                        Journey newJourney = new Journey(currentJourney);
                        Vector2 otherPos = connections[j].GetOtherVertex(currentJourney.currentPosition);
                        newJourney.Travel(otherPos, connections[j].GetRouteLength());

                        // Only keep moving this journey if the cost is less than the current lowest
                        if (newJourney.currentCost < currentShortestLength)
                            exploringJourneys.Add(newJourney);
                    }
                }
                else
                {
                    if (currentJourney.currentCost < currentShortestLength)
                    {
                        currentShortestLength = currentJourney.currentCost;
                    }

                    // Add to completely explored connections journey
                    finishedJourneys.Add(currentJourney);
                }

                // Add to remove from explore list
                toRemove.Add(currentJourney);
            }

            // Remove finished journeys from exploring list
            for (int i = 0; i < toRemove.Count; i++)
            {
                exploringJourneys.Remove(toRemove[i]);
            }
            toRemove.Clear();
        }

        // Find all the shortest of the journeys
        Journey shortestJourney = GetShortestJourney(finishedJourneys, b);

        // Final return
        return shortestJourney;
    }

    public static Journey GetShortestJourney(List<Journey> journeys, Vector2 target)
    {
        int currentShortestValue = int.MaxValue;
        int currentShortestIndex = -1;

        for (int i = 0; i < journeys.Count; i++)
        {
            Journey currentJourney = journeys[i];

            if (currentJourney.currentPosition == target && currentJourney.currentCost < currentShortestValue)
            {
                currentShortestValue = currentJourney.currentCost;
                currentShortestIndex = i;
            }
        }

        return journeys[currentShortestIndex];
    }

    public static ConnectionData[] RemoveConnectionsContainingVisitedPoints(ConnectionData[] connections, Journey journey)
    {
        List<ConnectionData> newConnections = new List<ConnectionData>();

        for (int i = 0; i < connections.Length; i++)
        {
            ConnectionData currentConnection = connections[i];
            Vector2 otherPos = currentConnection.GetOtherVertex(journey.currentPosition);

            if (!journey.HasVisitedPosition(otherPos))
                newConnections.Add(currentConnection);
        }

        return newConnections.ToArray();
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

public class Journey
{
    public Vector2 currentPosition;
    public int currentCost;
    public List<Vector2> visitedPlaces;

    public Journey() { }

    public Journey(Vector2 position)
    {
        currentPosition = position;
        currentCost = 0;

        visitedPlaces = new List<Vector2>();
    }

    public Journey(Journey oldJourney)
    {
        currentPosition = oldJourney.currentPosition;
        currentCost = oldJourney.currentCost;
        visitedPlaces = oldJourney.visitedPlaces;
    }

    public void Travel(Vector2 newPos, int moveCost)
    {
        visitedPlaces.Add(currentPosition);

        currentCost += moveCost;
        currentPosition = newPos;
    }

    public bool HasVisitedPosition(Vector2 position)
    {
        for (int i = 0; i < visitedPlaces.Count; i++)
        {
            if (visitedPlaces[i] == position)
                return true;
        }

        return false;
    }
}