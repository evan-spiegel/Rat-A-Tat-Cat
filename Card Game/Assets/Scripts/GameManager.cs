﻿using System;
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
	// cardsComputerKnows includes cards owned by the computer and the player,
	// so we'll have to keep track of that
	public List<Transform> cardsComputerKnows, computerPowerCards;

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
		if (deckTransform.childCount == 0)
		{
			GameObject topCard = Instantiate(cardPrefab, deckTransform, false);
			topCard.GetComponent<CardDisplay>().card =
				deck.currentDeck[currentDeckIndex];
			topCard.GetComponent<CardDisplay>().ShowFront(false);
			topCard.GetComponent<CardDisplay>().isDrawnCard = false;
			currentDeckIndex++;
		}
	}

	void PlayerTurn ()
    {
		CreateTopCard();
		// This button should only be interactable if we have either
		// swapped the drawn card for one of our own or dropped
		// the drawn card in the discard pile.
		// Deactivate it for the start of the next turn
		endTurnButton.interactable = false;
		// Only enable discard if top card is not a power card
		if (!TopDiscard().GetComponent<CardDisplay>().IsPowerCard())
		{
			EnableDiscard(true);
			EnablePlayerCards(true);
		}
		else
		{
			EnableDiscard(false);
			EnablePlayerCards(false);
		}
		EnableDeck(true);
		EnableComputerCards(false);
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

		// First card player draws
		//deck.startingDeck[8] = cardList[12];
		// First card computer draws
		//deck.startingDeck[9] = cardList[10];

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
		endTurnButton.interactable = false;
		EnablePlayerCards(false);
		EnableDiscard(false);
		EnableDeck(false);
		ComputerTurn();
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
		if (drawingTwo)
			DrawTwoOver();
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
		if (drawingTwo)
			DrawTwoOver();
		EnableComputerCards(false);
		EnableDiscard(false);
		EnablePowerCard(false);
		drawingTwo = true;
		CreateTopCard();
	}

	internal void StartSwap()
	{
		if (drawingTwo)
			DrawTwoOver();
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

	private GameObject TopDiscard()
	{
		return discardTransform.GetChild(discardTransform.childCount - 1).gameObject;
	}

	void ComputerTurn ()
	{
		CreateTopCard();
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
		// If there is a zero, one, two, or three on top of the discard pile
		// the computer should take it
		if (TopDiscard().GetComponent<CardDisplay>().IsLessThanFour())
		{
			ComputerMakeBestSwap("discard");
		}
		else
		{
			ComputerDrawFromDeck();
		}
	}

	private void ComputerDrawFromDeck()
	{
		CardDisplay topDeckCD = TopDeck().GetComponent<CardDisplay>();
		// - If we draw a zero, one, two, or three we should take it
		// and either swap for an unknown card or a high card with
		// the greatest difference
		// - If higher than three, swap for greatest difference
		// - If swap or draw two, activate it
		// - If peek, choose whether to activate it
		if (!topDeckCD.IsPowerCard())
		{
			ComputerMakeBestSwap("deck");
		}
		else
		{
			if (topDeckCD.card.cardType == "draw two")
			{
				ComputerDrawTwo();
			}
			else if (topDeckCD.card.cardType == "peek")
			{
				ComputerPeek();
			}
			else // "swap"
			{
				ComputerSwapPowerCard();
			}
		}
	}

	private void ComputerSwapPowerCard()
	{
		GameObject highestComputerCard = null;
		GameObject lowestPlayerCard = null;
		// For now, swap highest card we know and own for
		// lowest card we know the player owns
		List<Transform> cardsComputerKnowsAndOwns = CardsComputerKnowsAndOwns();
		List<Transform> cardsComputerKnowsPlayerOwns = new List<Transform>();
		GameObject swap = TopDeck();
		swap.GetComponent<CardDisplay>().ShowFront(true);
		swap.transform.SetParent(powerCardTransform);
		LeanTween.moveLocal(swap, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			foreach (Transform cardTransform in cardsComputerKnows)
			{
				if (!cardsComputerKnowsAndOwns.Contains(cardTransform))
				{
					cardsComputerKnowsPlayerOwns.Add(cardTransform);
				}
			}
			foreach (Transform cardTransform in cardsComputerKnowsAndOwns)
			{
				if (highestComputerCard == null ||
					cardTransform.GetChild(0).GetComponent<CardDisplay>().Value() >
					highestComputerCard.GetComponent<CardDisplay>().Value())
				{
					highestComputerCard = cardTransform.GetChild(0).gameObject;
				}
			}
			if (cardsComputerKnowsPlayerOwns.Count > 0)
			{
				foreach (Transform cardTransform in cardsComputerKnowsPlayerOwns)
				{
					// We don't want to swap for a power card
					if (!cardTransform.GetChild(0).GetComponent<CardDisplay>().IsPowerCard())
					{
						if (lowestPlayerCard == null ||
						cardTransform.GetChild(0).GetComponent<CardDisplay>().Value() <
						lowestPlayerCard.GetComponent<CardDisplay>().Value())
						{
							lowestPlayerCard = cardTransform.GetChild(0).gameObject;
						}
					}
				}
				if (lowestPlayerCard != null)
				{
					ComputerSwapPlayer(highestComputerCard, lowestPlayerCard);
				}
				else
				{
					// If the only cards we know the player to own are power cards,
					// swap for a random player card we don't know
					foreach (Transform cardTransform in playerField)
					{
						if (!cardsComputerKnowsPlayerOwns.Contains(cardTransform))
						{
							ComputerSwapPlayer(highestComputerCard, cardTransform.GetChild(0).gameObject);
							break;
						}
					}
				}
			}
			else
			{
				// If computer doesn't know any of the player's cards,
				// swap its highest card for a random player card
				ComputerSwapPlayer(highestComputerCard, playerField.GetChild(0).GetChild(0).gameObject);
			}
		});
	}

	private void ComputerSwapPlayer(GameObject computerCard, GameObject playerCard)
	{
		UpdateCardsComputerKnows(computerCard, playerCard);
		// Swap parents
		Transform computerCardParent = computerCard.transform.parent;
		Transform playerCardParent = playerCard.transform.parent;
		computerCard.transform.SetParent(playerCardParent);
		playerCard.transform.SetParent(computerCardParent);
		LeanTween.moveLocal(computerCard, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			LeanTween.moveLocal(playerCard, Vector2.zero, 1.0f).setOnComplete(()=>
			{
				UpdateCardStatus(computerCard);
				UpdateCardStatus(playerCard);
				// Move swap card to discard pile
				ComputerMovePowerCardToDiscard();
				ComputerTurnOver();
			});
		});
	}

	private void ComputerPeek()
	{
		GameObject peek = TopDeck();
		peek.GetComponent<CardDisplay>().ShowFront(true);
		// Peek randomly at one of the cards we don't know
		List<Transform> cardsComputerDoesntKnow = CardsComputerDoesntKnow();
		if (cardsComputerDoesntKnow.Count > 0)
		{
			peek.transform.SetParent(powerCardTransform);
			LeanTween.moveLocal(peek, Vector2.zero, 1.0f).setOnComplete(()=>
			{
				Transform peekCardTransform = cardsComputerDoesntKnow[0];
				cardsComputerKnows.Add(peekCardTransform);
				GameObject peekCard = peekCardTransform.GetChild(0).gameObject;
				// Animation to show player which card computer is peeking at
				LeanTween.scale(peekCard, Vector2.one * 1.3f, 0.5f).setOnComplete(()=>
				{
					LeanTween.scale(peekCard, Vector2.one, 0.5f).setOnComplete(()=>
					{
						ComputerMovePowerCardToDiscard();
					});
				});
			});
		}
		else
		{
			// If we know all our cards, just discard the peek
			ComputerDiscardDrawnCard(peek);
		}
	}

	private void ComputerDrawTwo()
	{
		// Make this 1
		drawTwoIndex++;
		GameObject drawTwo = TopDeck();
		drawTwo.GetComponent<CardDisplay>().ShowFront(true);
		// - Move draw two card to power card transform
		// - Swap or discard first card
		// - If discarded first card, swap or discard second card
		// - Move draw two card to discard
		drawTwo.transform.SetParent(powerCardTransform);
		LeanTween.moveLocal(drawTwo, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			while (drawTwoIndex <= 2)
			{
				// To keep things simple, only swap first drawn card if it's less than 4
				CreateTopCard();
				GameObject drawnCard = TopDeck();
				if (drawnCard.GetComponent<CardDisplay>().IsLessThanFour())
				{
					ComputerMakeBestSwap("deck");
					ComputerDrawTwoOver();
					return;
				}
				else
				{
					if (drawTwoIndex == 1)
					{
						// Discard it
						ComputerDiscardDrawnCard(drawnCard);
					}
					else
					{
						Transform highestSwappable = null;
						// If there are any cards we know to be
						// higher than this one, might as well swap it
						List<Transform> cardsComputerKnowsAndOwns = CardsComputerKnowsAndOwns();
						foreach(Transform cardTransform in cardsComputerKnowsAndOwns)
						{
							CardDisplay card = cardTransform.GetChild(0).GetComponent<CardDisplay>();
							if (card.Value() > drawnCard.GetComponent<CardDisplay>().Value())
							{
								if (highestSwappable == null ||
									highestSwappable.GetChild(0).GetComponent<CardDisplay>().Value() <
									card.Value())
								{
									highestSwappable = cardTransform;
								}
							}
						}
						if (highestSwappable != null)
						{
							ComputerSwapDeckDiscard(highestSwappable.GetChild(0).gameObject, "deck");
						}
						else
						{
							// Otherwise discard it
							ComputerDiscardDrawnCard(drawnCard);
						}
					}
					drawTwoIndex++;
				}
			}
			ComputerDrawTwoOver();
		});
	}

	private void ComputerDiscardDrawnCard(GameObject drawnCard)
	{
		drawnCard.transform.SetParent(discardTransform);
		LeanTween.moveLocal(drawnCard, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			drawnCard.GetComponent<CardDisplay>().isDrawnCard = false;
			drawnCard.GetComponent<CardDisplay>().isInDiscard = true;
			if (computerDrawingTwo)
			{
				if (drawTwoIndex >= 2)
				{
					ComputerDrawTwoOver();
				}
				else
				{
					CreateTopCard();
				}
			}
			else
			{
				ComputerTurnOver();
			}
		});
	}

	private void ComputerTurnOver()
	{
		EnablePlayerCards(true);
		EnableDiscard(true);
		EnableDeck(true);
		PlayerTurn();
	}

	private void ComputerMovePowerCardToDiscard()
	{
		GameObject powerCard = powerCardTransform.GetChild(0).gameObject;
		powerCard.transform.SetParent(discardTransform);
		LeanTween.moveLocal(powerCard, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			UpdateCardStatus(powerCard);
			ComputerTurnOver();
		});
	}

	// source should be "deck" or "discard"
	private void ComputerMakeBestSwap(string source)
	{
		int greatestDifference = -1;
		Transform greatestDifferenceCard = null;
		Dictionary<int, Transform> greatestDifferenceDict = FindGreatestDifference(source);
		// There should be only one key-value pair but we'll use this anyway
		foreach (KeyValuePair<int, Transform> kvp in greatestDifferenceDict)
		{
			greatestDifference = kvp.Key;
			greatestDifferenceCard = kvp.Value;
		}
		if (greatestDifference < 3)
		{
			if (computerPowerCards.Count > 0)
			{
				// Swap for one of the power cards randomly
				ComputerSwapDeckDiscard(computerPowerCards[0].GetChild(0).gameObject, source);
			}
			else
			{
				// Swap for one of the unknown cards randomly
				if (CardsComputerKnowsAndOwns().Count < 4)
				{
					ComputerSwapDeckDiscard(CardsComputerDoesntKnow()[0].GetChild(0).gameObject, source);
				}
				// If computer knows all 4 cards, swap for greatest difference
				else
				{
					ComputerSwapDeckDiscard(greatestDifferenceCard.GetChild(0).gameObject, source);
				}
			}
		}
		else
		{
			ComputerSwapDeckDiscard(greatestDifferenceCard.GetChild(0).gameObject, source);
		}
	}

	private List<Transform> CardsComputerKnowsAndOwns()
	{
		List<Transform> cards = new List<Transform>();
		foreach(Transform card in cardsComputerKnows)
		{
			if (card.GetChild(0).GetComponent<CardDisplay>().belongsToComputer)
			{
				cards.Add(card);
			}
		}
		return cards;
	}

	private Dictionary<int, Transform> FindGreatestDifference(string source)
	{
		computerPowerCards = new List<Transform>();
		GameObject swapCard = source == "deck" ? TopDeck() : TopDiscard();
		int topDiscardValue = swapCard.GetComponent<CardDisplay>().Value();
		// Choose which card to swap for:
		// - Subtract card we're taking from each card we know
		// - Find greatest difference
		// - If difference is less than three, swap for unknown card or power card
		// - Otherwise swap for card with biggest difference/highest card
		int greatestDifference = -100;
		Transform greatestDifferenceCard = null;
		List<Transform> cardsComputerKnowsAndOwns = CardsComputerKnowsAndOwns();
		foreach (Transform cardTransform in cardsComputerKnowsAndOwns)
		{
			int cardValue = cardTransform.GetChild(0).GetComponent<CardDisplay>().Value();
			// If difference is less than or equal to -10, it's a power card
			int difference = cardValue - topDiscardValue;
			// Find the computer's known power card locations
			if (difference <= -10)
			{
				computerPowerCards.Add(cardTransform);
			}
			if (difference > greatestDifference)
			{
				greatestDifference = difference;
				greatestDifferenceCard = cardTransform;
			}
		}
		Dictionary<int, Transform> greatestDifferenceDict = new Dictionary<int, Transform>();
		greatestDifferenceDict.Add(greatestDifference, greatestDifferenceCard);
		return greatestDifferenceDict;
	}

	// source should be "deck" or "discard"
	private void ComputerSwapDeckDiscard(GameObject computerCard, string source)
	{
		GameObject swapCard = source == "deck"? TopDeck() : TopDiscard();
		swapCard.transform.SetParent(computerCard.transform.parent);
		computerCard.transform.SetParent(discardTransform);
		// If we swapped for an unknown card, we now know what that card is
		if (!cardsComputerKnows.Contains(swapCard.transform.parent))
		{
			cardsComputerKnows.Add(swapCard.transform.parent);
		}
		if (computerDrawingTwo)
		{
			ComputerDrawTwoOver();
		}
		Debug.Log("swapCard.transform.localPosition: " + swapCard.transform.localPosition);
		// Move the swap card to the computer card location
		LeanTween.moveLocal(swapCard, Vector2.zero, 1.0f).setOnComplete(()=>
		{
			// Move the computer card to the discard
			LeanTween.moveLocal(computerCard.gameObject, Vector2.zero, 1.0f).setOnComplete(()=>
			{
				UpdateCardStatus(swapCard);
				UpdateCardStatus(computerCard.gameObject);
				// Flip new computer card over
				swapCard.GetComponent<CardDisplay>().ShowFront(false);
				// Show computer card now that it's in discard
				computerCard.GetComponent<CardDisplay>().ShowFront(true);
				ComputerTurnOver();
			});
		});
	}

	private GameObject TopDeck()
	{
		return deckTransform.GetChild(0).gameObject;
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
		ComputerMovePowerCardToDiscard();
		ComputerTurnOver();
	}

	private List<Transform> CardsComputerDoesntKnow()
	{
		List<Transform> unknownCards = new List<Transform>();
		List<Transform> cardsComputerKnowsAndOwns = CardsComputerKnowsAndOwns();
		foreach(Transform card in computerField)
		{
			if (!cardsComputerKnowsAndOwns.Contains(card))
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
		MovePowerCardToDiscard();
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

	public void UpdateCardsComputerKnows(GameObject thisCard, GameObject otherCard)
	{
		// There are a couple scenarios here:
		// 1. The player swaps a card the computer knows
		// for a card the computer knows - nothing changes
		// 2. The player swaps a card the computer knows
		// for a card the computer doesn't know - switch them
		// 3. The player swaps a card the computer doesn't know
		// for a card the computer knows - switch them
		// 4. The player swaps a card the computer doesn't know
		// for a card the computer doesn't know - nothing changes
		if (cardsComputerKnows.Contains(thisCard.transform.parent) &&
			!cardsComputerKnows.Contains(otherCard.transform.parent))
		{
			cardsComputerKnows.Remove(thisCard.transform.parent);
			cardsComputerKnows.Add(otherCard.transform.parent);
		}
		else if (!cardsComputerKnows.Contains(thisCard.transform.parent) &&
			cardsComputerKnows.Contains(otherCard.transform.parent))
		{
			cardsComputerKnows.Add(thisCard.transform.parent);
			cardsComputerKnows.Remove(otherCard.transform.parent);
		}
		// Afterward, we can just make sure the computer knows its bottom two cards
		if (!cardsComputerKnows.Contains(computerField.GetChild(0)))
		{
			cardsComputerKnows.Add(computerField.GetChild(0));
		}
		if (!cardsComputerKnows.Contains(computerField.GetChild(1)))
		{
			cardsComputerKnows.Add(computerField.GetChild(1));
		}
	}
}