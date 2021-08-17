using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager gameManager = (GameManager)target;

        // Create buttons to call functions in game manager
        // Toggle Map
        if (GUILayout.Button("Toggle Map Visibility"))
        {
            gameManager.ToggleMapActive();
            gameManager.ToggleLocationNames();
        }

        // New Nodes
        if (GUILayout.Button("Get Fresh Nodes"))
        {
            gameManager.GetNodes();

            if (gameManager.AUTO_UPDATE)
            {
                gameManager.CreateNewMap();
                gameManager.DisplayMap();
            }
        }

        // New Map
        if (GUILayout.Button("Create New Map"))
        {
            gameManager.CreateNewMap();

            if (gameManager.AUTO_UPDATE)
                gameManager.DisplayMap();
        }

        // Mutate Map
        if (GUILayout.Button("Mutate Map"))
        {
            gameManager.MutateMap();

            if (gameManager.AUTO_UPDATE)
                gameManager.DisplayMap();
        }

        // Display Map
        if (!gameManager.AUTO_UPDATE && GUILayout.Button("Display Map"))
        {
            gameManager.DisplayMap();
        }

        // Create Destinations
        if (GUILayout.Button("Produce Destinations"))
        {
            gameManager.ProduceDestinations();
        }

        // Toggle Debugging Mode
        if (GUILayout.Button("Toggle Debug Mode"))
        {
            gameManager.ToggleDebugMode();
        }

        // Show map connections internals
        if (gameManager.IsDebugging() && !gameManager.IsMapNull())
        {
            // Get connections
            MapData map = gameManager.GetCurrentMap();
            ConnectionData[] connections = map.mapConnections;

            // Get nodes for naming
            GameObject[] nodes = gameManager.GetNodeObjects();

            // Begin a new GUI area for elements
            //GUILayout.BeginArea(new Rect(10, 10, 100, 100));

            // Loop through all connections now and create options for them
            for (int i = 0; i < connections.Length; i++)
            {
                ConnectionData currentConnection = connections[i];

                // Begin this horizontal for this connection
                GUILayout.BeginHorizontal();

                GUILayout.Label(("Connection #" + i));
                GUILayout.Label(currentConnection.aPointPosition + " to " + currentConnection.bPointPosition);

                string nameA = DestinationManager.GetNameFromPosition(nodes, currentConnection.aPointPosition);
                string nameB = DestinationManager.GetNameFromPosition(nodes, currentConnection.bPointPosition);
                GUILayout.Label(nameA + " to " + nameB);

                if (GUILayout.Button("X", GUILayout.Width(50), GUILayout.Height(50)))
                {
                    gameManager.RemoveConnectionButton(i);

                    if (gameManager.AUTO_UPDATE)
                    {
                        gameManager.DisplayMap();
                    }
                }

                // End horizontal
                GUILayout.EndHorizontal();
            }

            // End the GUI area
            //GUILayout.EndArea();
        }

        // Do the usual below
        base.OnInspectorGUI();
    }
}
