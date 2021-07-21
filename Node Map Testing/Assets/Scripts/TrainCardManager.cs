using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTR.TTR_Trains;

namespace TTR
{
    namespace TTR_Cards
    {
        public class TrainCardManager : MonoBehaviour
        {
            public static TrainCardManager Instance;

            public TrainCard[] allCards; // Default, unchanging set of cards

            List<TrainCard> deck = new List<TrainCard>(); // Player draw pile
            List<TrainCard> discardPile = new List<TrainCard>(); // Player discard pile

            public const int NUMBER_OF_TRAIN_CARDS = 12;
            public const int NUMBER_OF_LOCOMOTIVE_CARDS = 14;

            private void Awake()
            {
                if (Instance == null)
                    Instance = this;
                else
                    Destroy(this);
            }

            private void Start()
            {
                PopulateNewDeck();
                ShuffleDeck();
            }

            private void Update()
            {
                if (Input.GetKeyUp(KeyCode.R))
                    ShuffleDiscardIntoDeck();

                if (Input.GetKeyUp(KeyCode.I))
                {
                    Debug.Log(deck.Count);

                    foreach (TrainCard tc in deck)
                        Debug.Log(tc.colour);
                }
            }

            void PopulateNewDeck()
            {
                TrainCard locomotive = GetTrainCard(TRAIN_COLOUR.ANY);

                TrainCard red = GetTrainCard(TRAIN_COLOUR.Red);
                TrainCard blue = GetTrainCard(TRAIN_COLOUR.Blue);
                TrainCard green = GetTrainCard(TRAIN_COLOUR.Green);
                TrainCard purple = GetTrainCard(TRAIN_COLOUR.Purple);
                TrainCard black = GetTrainCard(TRAIN_COLOUR.Black);
                TrainCard white = GetTrainCard(TRAIN_COLOUR.White);
                TrainCard yellow = GetTrainCard(TRAIN_COLOUR.Yellow);
                TrainCard orange = GetTrainCard(TRAIN_COLOUR.Orange);

                for (int i = 0; i < NUMBER_OF_TRAIN_CARDS; i++)
                {
                    deck.Add(red);
                    deck.Add(blue);
                    deck.Add(green);
                    deck.Add(purple);
                    deck.Add(black);
                    deck.Add(white);
                    deck.Add(yellow);
                    deck.Add(orange);
                }

                for (int i = 0; i < NUMBER_OF_LOCOMOTIVE_CARDS; i++)
                    deck.Add(locomotive);
            }

            void ShuffleDeck()
            {
                // Create empty shuffled deck
                List<TrainCard> shuffledDeck = new List<TrainCard>();

                // Create list of indexes to choose and remove from
                List<int> deckIndexList = new List<int>();
                for (int i = 0; i < deck.Count; i++)
                    deckIndexList.Add(i);

                for (int i = 0; i < deck.Count; i++)
                {
                    // Choose a random index from the list of remaining indexes
                    int index = Random.Range(0, deckIndexList.Count);
                    int indexIndex = deckIndexList[index];

                    shuffledDeck.Add(deck[indexIndex]);
                    deckIndexList.RemoveAt(index);
                }

                // Turn shuffled deck into deck
                deck = shuffledDeck;
            }

            void ShuffleDiscardIntoDeck()
            {
                // Add discard into deck
                for (int i = discardPile.Count - 1; i >= 0; i--)
                {
                    deck.Add(discardPile[i]);
                    discardPile.RemoveAt(i);
                }

                // Shuffle the deck
                ShuffleDeck();
            }


            //
            // EXTERNAL CALLS
            //
            public TrainCard DrawCard()
            {
                TrainCard drawnCard = deck[0];
                deck.RemoveAt(0);

                if (deck.Count == 0)
                    ShuffleDiscardIntoDeck();

                return drawnCard;
            }

            public TrainCard GetTrainCard(TRAIN_COLOUR trainColour)
            {
                for (int i = 0; i < allCards.Length; i++)
                    if (allCards[i].colour == trainColour)
                        return allCards[i];

                return new TrainCard();
            }

            public void DiscardCard(TrainCard card)
            {
                discardPile.Add(card);
            }

            public void DiscardCards(TrainCard[] cards)
            {
                for (int i = 0; i < cards.Length; i++)
                    discardPile.Add(cards[i]);
            }

            public int GetNumberDiscarded()
            {
                return discardPile.Count;
            }

            public int GetNumberDecked()
            {
                return deck.Count;
            }
        }
    }
}
