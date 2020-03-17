using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Deck deck;
	public GameObject playerField, computerField;
	public GameObject cardPrefab;
	public GameObject gameOverPanel;
	public Text winnerText;
	public Card[] cardList;
	public Sprite cardBack;
	private int currentDeckIndex = 0;
	public Transform drawnCardTransform, discardTransform;
	public bool draggingCard = false, draggingOverDiscard = false;
	public GameObject draggingOverCard = null;
	public Button endTurnButton;

	void Start () {
		endTurnButton.interactable = false;
		//gameOverPanel.SetActive (false);
		InitializeDeck ();
		ShuffleDeck ();
		Deal ("player");
		Deal ("computer");
		OnlyShowBottomTwo ();
		PlayerTurn ();
	}

	void PlayerTurn ()
    {
		DrawCard ();
    }

	void DrawCard ()
    {
		GameObject drawnCard = Instantiate(cardPrefab, drawnCardTransform, false);
		drawnCard.GetComponent<CardDisplay>().card =
			deck.currentDeck[currentDeckIndex];
		drawnCard.GetComponent<CardDisplay>().ShowFront(true);
		drawnCard.GetComponent<CardDisplay>().isDrawnCard = true;
		currentDeckIndex++;
	}

	void InitializeDeck ()
	{
		deck.startingDeck = new Card[Deck.numberOfCards];
		deck.currentDeck = new Card[Deck.numberOfCards];
		// Deck index
		int i = 0;
		// There are four of each card numbered 0-8
		for (int j = 0; j <= 8; j++)
        {
			for (int k = 0; k < 4; k++)
            {
				deck.startingDeck[i] = cardList[j];
				i++;
            }
        }
		// There are nine 9s
		for (int j = 0; j < 9; j++)
        {
			deck.startingDeck[i] = cardList[9];
			i++;
        }
		// There are three of each power card (draw two, peek, and swap)
		for (int j = 10; j <= 12; j++)
        {
			for (int k = 0; k < 3; k++)
            {
				deck.startingDeck[i] = cardList[j];
				i++;
            }
        }

		deck.currentDeck = deck.startingDeck;
	}

	void ShuffleDeck ()
	{
		Card[] temp = new Card[deck.startingDeck.Length];
		// For each card in the deck, move it to a random location
		// and make sure that location is empty first
		for (int i = 0; i < deck.startingDeck.Length; i++) {
			int randomIndex = Random.Range (0, deck.startingDeck.Length);
			while (temp [randomIndex] != null) {
				randomIndex = Random.Range (0, deck.startingDeck.Length);
			}
			temp [randomIndex] = deck.startingDeck [i];
		}
		deck.currentDeck = temp;
	}

	void Deal (string dealTo)
	{
		// dealTo should be either "player" or "computer"
		for (int i = 0; i < 4; i++) {
			GameObject card = Instantiate (cardPrefab, dealTo == "player"? 
				playerField.transform.GetChild(i) : computerField.transform.GetChild(i), false);
			card.GetComponent<CardDisplay> ().card = 
				deck.currentDeck [currentDeckIndex + i];
			if (dealTo == "player")
				card.GetComponent<CardDisplay>().belongsToPlayer = true;
			else
				card.GetComponent<CardDisplay>().belongsToPlayer = false;
		}
		currentDeckIndex += 4;
	}

	void OnlyShowBottomTwo ()
    {
		int cardNum = 0;
		foreach (Transform transform in playerField.transform)
        {
			Transform card = transform.GetChild(0);
			if (cardNum == 2 || cardNum == 3)
            {
				card.gameObject.GetComponent<CardDisplay>().ShowFront(true);
            }
			else
            {
				card.gameObject.GetComponent<CardDisplay>().ShowFront(false);
			}
			cardNum++;
        }
		foreach (Transform transform in computerField.transform)
		{
			Transform card = transform.GetChild(0);
			card.gameObject.GetComponent<CardDisplay>().ShowFront(false);
		}
	}

	public void EndTurn ()
	{
		//ComputerTurn ();

		// For now, just deal another card to the player
		DrawCard ();
		// This button should only be interactable if we have either
		// swapped the drawn card for one of our own or dropped
		// the drawn card in the discard pile.
		// Deactivate it for the start of the next turn
		endTurnButton.interactable = false;
	}

	void ComputerTurn ()
	{
		
	}
}