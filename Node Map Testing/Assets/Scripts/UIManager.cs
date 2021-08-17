using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    List<GameObject> locationNames;

    [SerializeField] GameObject nameCanvas;
    [SerializeField] GameObject locationTextPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        locationNames = new List<GameObject>();
    }

    public void SetNameVisiblity(bool state)
    {
        ChangeNameVisibility(state);
    }

    public void ShowNames()
    {
        ChangeNameVisibility(true);
    }

    public void HideNames()
    {
        ChangeNameVisibility(false);
    }

    private void ChangeNameVisibility(bool state)
    {
        for (int i = 0; i < locationNames.Count; i++)
        {
            locationNames[i].gameObject.SetActive(state);
        }
    }

    public void SetupNames(GameObject[] nodes, bool initialState)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            GameObject newTextObject = Instantiate(locationTextPrefab, nameCanvas.transform);
            newTextObject.GetComponent<Text>().text = nodes[i].GetComponent<NodeMapMarker>().GetName();
            newTextObject.transform.position = nodes[i].transform.position;

            locationNames.Add(newTextObject);
        }

        if (!initialState)
            ChangeNameVisibility(false);
    }
}
