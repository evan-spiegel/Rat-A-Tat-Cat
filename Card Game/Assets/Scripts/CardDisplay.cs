﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CardDisplay : MonoBehaviour, 
	IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

	public Card card;

	public Image artworkImage;

	public Vector3 InitialScale { get; set; }
	private float highlightScale = 0.05f;
	private GameManager gameManager;
	public bool belongsToPlayer = false, belongsToComputer = false;
	public bool isDrawnCard = false;
	public bool isInDiscard = false;

	void Start () {
		InitialScale = transform.localScale;
		gameManager = FindObjectOfType<GameManager> ();
	}

	public void ShowFront (bool show)
    {
		if (gameManager == null)
			gameManager = FindObjectOfType<GameManager> ();
		artworkImage.sprite = show? card.artwork : gameManager.cardBack;
    }

	// Only refers to text attached to card prefab, not text attached to
	// card transforms
	public void ShowNumberText(bool show)
	{
		transform.GetChild(0).GetComponent<Text>().text = show ?
			card.cardType : "";
	}

	public void ShowNumberTextDiscard(bool show)
	{
		transform.GetChild(1).GetComponent<Text>().text = show ?
			card.cardType : "";
		if (gameManager == null)
		{
			gameManager = FindObjectOfType<GameManager>();
		}
		gameManager.MakeSureOnlyTopDiscardShowsNumber();
	}

	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		if (!gameManager.computerTakingTurn)
		{
			if (gameManager.draggingCard)
			{
				gameManager.draggingOverCard = gameObject;
			}
			HighlightCard();
		}
	}

	void HighlightCard ()
	{
		transform.localScale = new Vector3(
			transform.localScale.x + highlightScale,
			transform.localScale.y + highlightScale,
			transform.localScale.z
		);
	}

	public void UnhighlightCard()
	{
		transform.localScale = InitialScale;
	}

	public void OnPointerExit(PointerEventData pointerEventData)
	{
		if (!gameManager.computerTakingTurn)
		{
			UnhighlightCard();
			gameManager.draggingOverCard = null;
		}
	}

	internal bool IsPowerCard()
    {
		return card.cardType == "draw two" || card.cardType == "peek" || card.cardType == "swap";
    }

	public void OnPointerDown(PointerEventData eventData)
	{
		if (gameManager.peeking && belongsToPlayer && transform.parent.parent.tag != "Bottom Card")
		{
			// Peek at this card since it's one of our top cards
			ShowFront(true);
			Text cardTransformNumberText = transform.parent.parent.GetChild(1).GetComponent<Text>();
			cardTransformNumberText.gameObject.SetActive(true);
			// Enable the peek card so that player can drag it to discard
			// to show they're done peeking and are ending their turn
			gameManager.EnablePowerCard(true);
			gameManager.EnableTopTwoPlayerCards(false);
			gameManager.peekedCard = gameObject;
		}
	}

	internal bool IsLessThanFour()
	{
		int value = Value();
		return value != -10 && value < 4;
    }

	internal int Value()
	{
		if (IsPowerCard())
		{
			return -10;
		}
		return int.Parse(card.cardType);
	}
}