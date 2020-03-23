using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Deck deck;
	public Transform playerField, computerField;
	public GameObject cardPrefab;
	public GameObject gameOverPanel;
	public Text winnerText;
	public Card[] cardList;
	public Sprite cardBack, swapArtwork;
	private int currentDeckIndex = 0;
	public Transform drawnCardTransform, discardTransform, deckTransform, powerCardTransform;
	public bool draggingCard = false, draggingOverDiscard = false, draggingOverPowerCard = false;
	public GameObject draggingOverCard = null, peekedCard = null;
	public Button endTurnButton;
	public bool swapping = false, drawingTwo = false, peeking = false;
	public int drawTwoIndex = 0;

	void Start () {
		endTurnButton.interactable = false;
		//gameOverPanel.SetActive (false);
		InitializeDeck ();

		// Comment this out to test power cards
		ShuffleDeck ();

		Deal ("player");
		Deal ("computer");
		CreateTopCard();
		// Disable computer cards at the start
		EnableComputerCards(false);
		// Disable player cards at the start,
		// since there is nothing in discard yet
		EnablePlayerCards(false);
		// But enable top card of deck
		deckTransform.GetChild(0).GetComponent<Image>().raycastTarget = true;
		OnlyShowBottomTwo ();
		PlayerTurn ();
	}

	public void EnableComputerCards(bool enable)
	{
		foreach(Transform cardTransform in computerField)
		{
			cardTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
		}
	}

	public void CreateTopCard()
	{
		GameObject topCard = Instantiate(cardPrefab, deckTransform, false);
		topCard.GetComponent<CardDisplay>().card =
			deck.currentDeck[currentDeckIndex];
		topCard.GetComponent<CardDisplay>().ShowFront(false);
		topCard.GetComponent<CardDisplay>().isDrawnCard = false;
		currentDeckIndex++;
	}

	void PlayerTurn ()
    {
		
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

		// For testing, make the first card the player draws a swap
		//deck.startingDeck[8] = cardList[12];

		deck.currentDeck = deck.startingDeck;
	}

	void ShuffleDeck ()
	{
		Card[] temp = new Card[deck.startingDeck.Length];
		// For each card in the deck, move it to a random location
		// and make sure that location is empty first
		for (int i = 0; i < deck.startingDeck.Length; i++) {
			int randomIndex = UnityEngine.Random.Range (0, deck.startingDeck.Length);
			while (temp [randomIndex] != null) {
				randomIndex = UnityEngine.Random.Range (0, deck.startingDeck.Length);
			}
			temp [randomIndex] = deck.startingDeck [i];
		}
		deck.currentDeck = temp;
	}

	internal void TurnOver()
	{
		endTurnButton.interactable = true;
		EnablePlayerCards(false);
		EnableDiscard(false);
		EnableDeck(false);
	}

	public void EnablePlayerCards(bool enable)
	{
		// Make it so the player can't interact with any more cards
		// after taking action (or enable them for start of next turn)
		foreach (Transform cardTransform in playerField.transform)
		{
			cardTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
		}
	}

	public void EnableDiscard(bool enable)
	{
		// Disable (or enable) all cards in discard
		foreach (Transform card in discardTransform)
		{
			card.GetComponent<Image>().raycastTarget = enable;
		}
	}

	public void EnableDeck(bool enable)
	{
		// Disable (or enable) top card of deck
		if (deckTransform.childCount > 0)
		{
			deckTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
		}
	}

	public void EnableDrawnCard(bool enable)
	{
		// Disable (or enable) the drawn card
		if (drawnCardTransform.childCount > 0)
		{
			drawnCardTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
		}
	}

	public void EnablePowerCard(bool enable)
	{
		// Disable (or enable) the drawn card
		if (powerCardTransform.childCount > 0)
		{
			powerCardTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
		}
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
			{
				card.GetComponent<CardDisplay>().belongsToPlayer = true;
				card.GetComponent<CardDisplay>().belongsToComputer = false;
			}
			// dealTo == "computer"
			else
			{
				card.GetComponent<CardDisplay>().belongsToPlayer = false;
				card.GetComponent<CardDisplay>().belongsToComputer = true;
			}
		}
		currentDeckIndex += 4;
	}

    internal void StartPeek()
    {
		EnableComputerCards(false);
		EnableDiscard(false);
		EnablePowerCard(false);
		EnableTopTwoPlayerCards(true);
		peeking = true;
    }

	public void EnableTopTwoPlayerCards(bool enable)
	{
		foreach (Transform cardTransform in playerField.transform)
		{
			if (cardTransform.tag != "Bottom Card")
			{
				cardTransform.GetChild(0).GetComponent<Image>().raycastTarget = enable;
			}
		}
	}

	internal void StartDrawTwo()
	{
		EnableComputerCards(false);
		EnableDiscard(false);
		EnablePowerCard(false);
		drawingTwo = true;
		CreateTopCard();
	}

	internal void StartSwap()
	{
		EnablePlayerCards(true);
		EnableComputerCards(true);
		EnableDiscard(false);
		EnablePowerCard(false);
		swapping = true;
	}

	public void OnlyShowBottomTwo ()
    {
		int cardNum = 0;
		foreach (Transform transform in playerField)
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
		foreach (Transform transform in computerField)
		{
			Transform card = transform.GetChild(0);
			card.gameObject.GetComponent<CardDisplay>().ShowFront(false);
		}
	}

	public void EndTurn ()
	{
		//ComputerTurn ();
		// Only create a new top card if we took from the deck,
		// not if we took from the discard
		if (deckTransform.childCount == 0)
		{
			CreateTopCard();
		}
		// This button should only be interactable if we have either
		// swapped the drawn card for one of our own or dropped
		// the drawn card in the discard pile.
		// Deactivate it for the start of the next turn
		endTurnButton.interactable = false;
		EnablePlayerCards(true);
		// Only enable discard if top card is not a power card
		if (!TopDiscardIsPowerCard())
		{
			EnableDiscard(true);
		}
		EnableDeck(true);
	}

	private bool TopDiscardIsPowerCard()
	{
		return discardTransform.GetChild(discardTransform.childCount - 1)
			.GetComponent<CardDisplay>().IsPowerCard();
	}

	void ComputerTurn ()
	{
		
	}

	internal void DrawTwoOver()
	{
		drawTwoIndex = 0;
		drawingTwo = false;
	}
}