using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMapMarker : MonoBehaviour
{
    public string location_name;
    private Vector2 location_coords;

    private void Start()
    {
        location_coords = new Vector2(transform.position.x, transform.position.z);

        GetComponentInChildren<Text>().text = location_name.ToUpper();
    }

    public Vector2 GetPosition() { return location_coords; }
}
