using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameManager gameManager;

    public void OnBeginDrag (PointerEventData eventData)
    {
        gameManager.draggingCard = true;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag (PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag (PointerEventData eventData)
    {
        gameManager.draggingCard = false;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        GameObject otherCard = gameManager.draggingOverCard;
        // If we let go of a card over another one, swap the two of them
        if (otherCard != null)
        {
            // If we are swapping the drawn card or the top discard for one of our own
            // (We can swap from either direction)
            if (CanSwapDrawnCard(otherCard) || CanSwapDiscard(otherCard))
            {
                SwapForCard(otherCard);
            }
            // If we are dropping the drawn card in the discard pile
            else if (GetComponent<CardDisplay>().isDrawnCard &&
                otherCard.GetComponent<CardDisplay>().isInDiscard)
            {
                DiscardDrawnCard();
            }
        }
        else
        {
            // We're dragging and dropping the drawn card into the discard pile
            if (gameManager.draggingOverDiscard && GetComponent<CardDisplay>().isDrawnCard)
            {
                DiscardDrawnCard();
            }
        }
        transform.localPosition = Vector2.zero;
    }

    private void DiscardDrawnCard ()
    {
        transform.SetParent(gameManager.discardTransform);
        GetComponent<CardDisplay>().isDrawnCard = false;
        GetComponent<CardDisplay>().isInDiscard = true;
        gameManager.endTurnButton.interactable = true;
    }

    private void SwapForCard (GameObject otherCard)
    {
        // Player can either drag the non-player card or the player card
        bool thisCardBelongsToPlayer = GetComponent<CardDisplay>().belongsToPlayer;
        // We're dragging the non-player card
        if (!thisCardBelongsToPlayer)
        {
            transform.SetParent(otherCard.transform.parent);
            otherCard.transform.SetParent(gameManager.discardTransform);
        }
        // We're dragging the player card
        else
        {
            otherCard.transform.SetParent(transform.parent);
            transform.SetParent(gameManager.discardTransform);
        }
        transform.localPosition = Vector2.zero;
        otherCard.transform.localPosition = Vector2.zero;
        UpdateCardStatus(gameObject);
        UpdateCardStatus(otherCard);
        gameManager.endTurnButton.interactable = true;
    }

    private void UpdateCardStatus (GameObject card)
    {
        CardDisplay cardD = card.GetComponent<CardDisplay>();
        // If we belong to the player
        if (card.transform.parent.parent.gameObject == gameManager.playerField)
        {
            cardD.belongsToPlayer = true;
            cardD.isInDiscard = false;
            // Only show the front of the card if it's in the bottom two
            cardD.ShowFront(card.transform.parent.tag == "Bottom Card");
        }
        // If we are in the discard
        else
        {
            cardD.belongsToPlayer = false;
            cardD.isInDiscard = true;
            // Show the front of the card if it's in the discard pile
            cardD.ShowFront(true);
        }
        cardD.isDrawnCard = false;
    }

    private bool CanSwapDrawnCard (GameObject otherCard)
    {
        if ((GetComponent<CardDisplay>().belongsToPlayer && 
            otherCard.GetComponent<CardDisplay>().isDrawnCard) ||
            GetComponent<CardDisplay>().isDrawnCard &&
            otherCard.GetComponent<CardDisplay>().belongsToPlayer)
        {
            return true;
        }
        return false;
    }

    private bool CanSwapDiscard(GameObject otherCard)
    {
        if ((GetComponent<CardDisplay>().isInDiscard &&
            otherCard.GetComponent<CardDisplay>().belongsToPlayer) ||
            (GetComponent<CardDisplay>().belongsToPlayer &&
            otherCard.GetComponent<CardDisplay>().isInDiscard))
        {
            return true;
        }
        return false;
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        
    }
}