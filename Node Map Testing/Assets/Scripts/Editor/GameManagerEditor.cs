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

        // Do the usual below
        base.OnInspectorGUI();
    }
}
