using System.Collections;
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

	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		// Swapping the drawn card for one of our own
		if (gameManager.draggingCard)
		{
			//HighlightCard ();
			gameManager.draggingOverCard = gameObject;
		}
		HighlightCard ();
	}

	void HighlightCard ()
	{
		transform.localScale = new Vector3(
			transform.localScale.x + highlightScale,
			transform.localScale.y + highlightScale,
			transform.localScale.z
		);
	}

	public void OnPointerExit(PointerEventData pointerEventData)
	{
		// Unhighlight the card
		transform.localScale = InitialScale;
		gameManager.draggingOverCard = null;
	}

    internal bool IsPowerCard()
    {
		return card.cardType == "draw two" || card.cardType == "peek" || card.cardType == "swap";
    }

	public void OnPointerDown(PointerEventData eventData)
	{
		if (gameManager.peeking && belongsToPlayer && transform.parent.tag != "Bottom Card")
		{
			// Peek at this card since it's one of our top cards
			ShowFront(true);
			// Enable the peek card so that player can drag it to discard
			// to show they're done peeking and are ending their turn
			gameManager.EnablePowerCard(true);
			gameManager.EnableTopTwoPlayerCards(false);
			gameManager.peekedCard = gameObject;
		}
	}
}