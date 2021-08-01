using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeMapMarker : MonoBehaviour
{
    public string location_name;

    private void Start()
    {
        GetComponentInChildren<Text>().text = location_name.ToUpper();
    }

    public string GetName() { return location_name; }
}
