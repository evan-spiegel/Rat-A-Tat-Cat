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
	private bool computerDrawingTwo = false;
	public int drawTwoIndex = 0;
	private List<Transform> cardsComputerKnows, computerPowerCards;

	void Start () {
		cardsComputerKnows = new List<Transform>();
		endTurnButton.interactable = false;
		//gameOverPanel.SetActive (false);
		InitializeDeck ();

		// Comment this out to test power cards
		ShuffleDeck ();

		Deal ("player");
		Deal ("computer");
		// Computer starts off knowing the bottom two cards, just like the player
		cardsComputerKnows.Add(computerField.GetChild(0));
		cardsComputerKnows.Add(computerField.GetChild(1));
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
		ComputerTurn ();
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
		if (!TopDiscard().GetComponent<CardDisplay>().IsPowerCard())
		{
			EnableDiscard(true);
		}
		EnableDeck(true);
	}

	private GameObject TopDiscard()
	{
		return discardTransform.GetChild(discardTransform.childCount - 1).gameObject;
	}

	void ComputerTurn ()
	{
		EnablePlayerCards(false);
		EnableComputerCards(false);
		EnableDiscard(false);
		// Computer should do all the same things the player does:
		// - Choose to take from deck or discard
		// - If taking from deck, choose to swap for one of its cards or discard it
		// - If drawing power card, activate it (unless it's peek, then choose whether to use it)
		// - Discard power card at the end and end turn
		// All computer actions should be animated with LeanTween so player can see what's happening
		ComputerDrawFromDeckOrDiscard();
	}

	private void ComputerDrawFromDeckOrDiscard()
	{
		GameObject topDiscard = TopDiscard();
		// If there is a zero, one, two, or three on top of the discard pile
		// the computer should take it
		if (topDiscard.GetComponent<CardDisplay>().IsLessThanFour())
		{
			ComputerTakeFromDiscard(topDiscard);
		}
		else
		{
			ComputerDrawFromDeck();
		}
	}

	private void ComputerDrawFromDeck()
	{
		// - If we draw a zero, one, two, or three we should take it
		// and either swap for an unknown card or a high card with
		// the greatest difference
		// - If higher than three, swap for greatest difference
		// - If swap or draw two, activate it
		// - If peek, choose whether to activate it
	}

	private void ComputerTakeFromDiscard(GameObject topDiscard)
	{
		int greatestDifference = -1;
		Transform greatestDifferenceCard = null;
		Dictionary<int, Transform> greatestDifferenceDict = FindGreatestDifference(topDiscard);
		// There should be only one key-value pair but we'll use this anyway
		foreach(KeyValuePair<int,Transform> kvp in greatestDifferenceDict)
		{
			greatestDifference = kvp.Key;
			greatestDifferenceCard = kvp.Value;
		}
		if (greatestDifference < 3)
		{
			if (computerPowerCards.Count > 0)
			{
				// Swap for one of the power cards randomly
				ComputerSwapForDiscard(computerPowerCards[0]);
			}
			else
			{
				// Swap for one of the unknown cards randomly
				if (cardsComputerKnows.Count < 4)
				{
					ComputerSwapForDiscard(CardsComputerDoesntKnow()[0]);
				}
				// If computer knows all 4 cards, swap for greatest difference
				else
				{
					ComputerSwapForDiscard(greatestDifferenceCard);
				}
			}
		}
		else
		{
			ComputerSwapForDiscard(greatestDifferenceCard);
		}
	}

	private Dictionary<int, Transform> FindGreatestDifference(GameObject swapCard)
	{
		computerPowerCards = new List<Transform>();
		int topDiscardValue = swapCard.GetComponent<CardDisplay>().Value();
		// Choose which card to swap for:
		// - Subtract card we're taking from each card we know
		// - Find greatest difference
		// - If difference is less than three, swap for unknown card or power card
		// - Otherwise swap for card with biggest difference/highest card
		int greatestDifference = -100;
		Transform greatestDifferenceCard = null;
		foreach (Transform card in cardsComputerKnows)
		{
			int cardValue = card.GetComponent<CardDisplay>().Value();
			// If difference is less than or equal to -10, it's a power card
			int difference = cardValue - topDiscardValue;
			// Find the computer's known power card locations
			if (difference <= -10)
			{
				computerPowerCards.Add(card);
			}
			if (difference > greatestDifference)
			{
				greatestDifference = difference;
				greatestDifferenceCard = card;
			}
		}
		Dictionary<int, Transform> greatestDifferenceDict = new Dictionary<int, Transform>();
		greatestDifferenceDict.Add(greatestDifference, greatestDifferenceCard);
		return greatestDifferenceDict;
	}

	private void ComputerSwapForDiscard(Transform computerCard)
	{
		GameObject topDiscard = TopDiscard();
		topDiscard.transform.SetParent(computerCard.parent);
		computerCard.SetParent(discardTransform);
		if (computerDrawingTwo)
		{
			ComputerDrawTwoOver();
			MovePowerCardToDiscard();
		}
		LeanTween.moveLocal(topDiscard, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			LeanTween.moveLocal(computerCard.gameObject, Vector2.zero, 1.0f).setOnComplete(()=>
			{
				UpdateCardStatus(topDiscard);
				UpdateCardStatus(computerCard.gameObject);
				// Flip new computer card over
				topDiscard.GetComponent<CardDisplay>().ShowFront(false);
				TurnOver();
			});
		});
	}

	public void MovePowerCardToDiscard()
	{
		GameObject powerCard = powerCardTransform.GetChild(0).gameObject;
		powerCard.transform.SetParent(discardTransform);
		powerCard.transform.localPosition = Vector2.zero;
		UpdateCardStatus(powerCard);
	}

	private void ComputerDrawTwoOver()
	{
		drawTwoIndex = 0;
		computerDrawingTwo = false;
	}

	private List<Transform> CardsComputerDoesntKnow()
	{
		List<Transform> unknownCards = new List<Transform>();
		foreach(Transform card in computerField)
		{
			if (!cardsComputerKnows.Contains(card))
			{
				unknownCards.Add(card);
			}
		}
		return unknownCards;
	}

	internal void DrawTwoOver()
	{
		drawTwoIndex = 0;
		drawingTwo = false;
	}

	public void UpdateCardStatus(GameObject card)
	{
		CardDisplay cardD = card.GetComponent<CardDisplay>();
		// If we belong to the player
		if (card.transform.parent.parent == playerField)
		{
			cardD.belongsToPlayer = true;
			cardD.belongsToComputer = false;
			cardD.isInDiscard = false;
			// Only show the front of the card if it's in the bottom two
			cardD.ShowFront(card.transform.parent.tag == "Bottom Card");
		}
		// If we belong to the computer
		else if (card.transform.parent.parent == computerField)
		{
			cardD.belongsToPlayer = false;
			cardD.belongsToComputer = true;
			cardD.isInDiscard = false;
			cardD.ShowFront(false);
		}
		// If we are in the discard
		else
		{
			cardD.belongsToPlayer = false;
			cardD.belongsToComputer = false;
			cardD.isInDiscard = true;
			// Show the front of the card if it's in the discard pile
			cardD.ShowFront(true);
		}
		cardD.isDrawnCard = false;
	}
}