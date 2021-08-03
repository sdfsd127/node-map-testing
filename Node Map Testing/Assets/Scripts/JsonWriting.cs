using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class JsonWriting
{
    public static void OutputDestinationJSON(DestinationData[] destinations)
    {
        DestinationsJSON_Array destinationJSON = new DestinationsJSON_Array();
        Array.Resize<DestinationsJSON>(ref destinationJSON.destinations, destinations.Length);

        for (int i = 0; i < destinations.Length; i++)
        {
            DestinationData currentDestionationData = destinations[i];
            destinationJSON.destinations[i] = new DestinationsJSON();

            destinationJSON.destinations[i].start = currentDestionationData.start;
            destinationJSON.destinations[i].finish = currentDestionationData.finish;
            destinationJSON.destinations[i].value = currentDestionationData.points;
        }

        string jsonString = JsonUtility.ToJson(destinationJSON, true);
        File.WriteAllText(Application.dataPath + "/StreamingData/destinations.json", jsonString);
    }
}

[System.Serializable]
public class DestinationsJSON_Array
{
    public DestinationsJSON[] destinations;
}

[System.Serializable]
public class DestinationsJSON
{
    public string start;
    public string finish;
    public int value;
}