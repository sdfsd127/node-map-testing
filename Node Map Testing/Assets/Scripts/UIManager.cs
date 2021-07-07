using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTR.TTR_Trains;
using TTR.TTR_Cards;
using TTR.TTR_Players;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject cardInventoryPanel;
    public GameObject cardPrefab;

    List<GameObject> cardObjects = new List<GameObject>();

    public Text discardedNumText;
    public Text deckedNumText;

    public Text currentPlayerText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void ClearTrainCardInventoryDisplay()
    {
        foreach (GameObject g in cardObjects)
            Destroy(g);

        cardObjects.Clear();
    }

    public void DisplayTrainCardInventory(TrainCard[] trainCards)
    {
        ClearTrainCardInventoryDisplay();

        for (int i = 0; i < trainCards.Length; i++)
        {
            DrawTrainCard(trainCards[i]);
        }
    }

    public void UpdateNumbersUIText()
    {
        discardedNumText.text = "DISCARDED: " + TrainCardManager.Instance.GetNumberDiscarded();
        deckedNumText.text = "DECKED: " + TrainCardManager.Instance.GetNumberDecked();
        currentPlayerText.text = "Current Player: " + PlayerManager.Instance.GetCurrentPlayerIndex();
    }

    public void DrawTrainCard(TrainCard card)
    {
        GameObject cardObject = Instantiate(cardPrefab, cardInventoryPanel.transform);
        cardObject.GetComponent<Image>().sprite = card.sprite;
        cardObjects.Add(cardObject);
    }
}
