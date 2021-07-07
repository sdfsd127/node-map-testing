using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTR.TTR_Cards;
using TTR.TTR_Trains;

namespace TTR
{
    namespace TTR_Players
    {
        public class PlayerManager : MonoBehaviour
        {
            public static PlayerManager Instance;

            PlayerCharacter[] playerCharacters = new PlayerCharacter[PLAYER_CHARACTER_NUM];
            int currentPlayerIndex = 0;
            const int PLAYER_CHARACTER_NUM = 3;

            private void Awake()
            {
                if (Instance == null)
                    Instance = this;
                else
                    Destroy(this);
            }

            private void Start()
            {
                for (int i = 0; i < PLAYER_CHARACTER_NUM; i++)
                {
                    playerCharacters[i] = new PlayerCharacter();
                }
            }

            public void DrawCard()
            {
                TrainCard card = TrainCardManager.Instance.DrawCard();
                GiveCurrentPlayerCard(card);
            }

            public void GiveCurrentPlayerCard(TrainCard card)
            {
                playerCharacters[currentPlayerIndex].GiveCard(card);

                UpdateTrainCardUI();
            }

            public void UpdateTrainCardUI()
            {
                UIManager.Instance.DisplayTrainCardInventory(GetCurrentPlayersCards());
                UIManager.Instance.UpdateNumbersUIText();
            }

            public TrainCard[] GetCurrentPlayersCards()
            {
                return playerCharacters[currentPlayerIndex].GetHand();
            }

            public void DiscardHand()
            {
                playerCharacters[currentPlayerIndex].DiscardHand();

                UpdateTrainCardUI();
            }

            public void NextPlayer()
            {
                currentPlayerIndex++;

                if (currentPlayerIndex >= PLAYER_CHARACTER_NUM)
                    currentPlayerIndex = 0;

                UpdateTrainCardUI();
            }

            public int GetCurrentPlayerIndex()
            {
                return currentPlayerIndex;
            }
        }

        public class PlayerCharacter
        {
            List<TrainCard> playerHand = new List<TrainCard>();

            public void GiveCard(TrainCard card)
            {
                playerHand.Add(card);
            }

            public TrainCard[] GetHand()
            {
                return playerHand.ToArray();
            }

            public void DiscardHand()
            {
                TrainCardManager.Instance.DiscardCards(GetHand());

                playerHand.Clear();
            }
        }
    }
}