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
        CardDisplay cardD = GetComponent<CardDisplay>();
        gameManager.draggingCard = true;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        // If we are the top card of the deck
        if (transform.parent == gameManager.deckTransform)
        {
            // First, check if it's a power card
            if (cardD.card.cardType == "draw two")
            {

            }
            else if (cardD.card.cardType == "peek")
            {

            }
            else if (cardD.card.cardType == "swap")
            {
                // Don't put it in the discard right away;
                // have it on the field at first before the swap
                // is completed, then move it to the discard after
                // to show it's completed and the turn is over
                transform.SetParent(gameManager.drawnCardTransform);
                // Don't allow player to swap the swap card for one of their own
                gameManager.EnablePlayerCards(false);
            }
            // It's a number card
            else
            {
                transform.SetParent(gameManager.drawnCardTransform);
                // We only need to enable the player cards if
                // there is nothing in the discard
                if (gameManager.discardTransform.childCount == 0)
                {
                    gameManager.EnablePlayerCards(true);
                }
                // Enable the discard if we are dragging the drawn card
                // (and it's a number card)
                gameManager.EnableDiscard(true);
            }

            cardD.isDrawnCard = true;
            cardD.ShowFront(true);
        }
        // If we're picking up the drawn card again after
        // dropping it onto the field
        else if (transform.parent == gameManager.drawnCardTransform)
        {
            gameManager.EnableDiscard(true);
        }
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
            // If we are swapping one of our cards for one of the computer's cards
            else if (CanSwapWithComputer(otherCard))
            {
                SwapForComputerCard(otherCard);
            }
        }
        else
        {
            // Check if it's a swap card
            if (GetComponent<CardDisplay>().card.cardType == "swap")
            {
                // Start the swap process
                gameManager.EnablePlayerCards(true);
                gameManager.EnableComputerCards(true);
                gameManager.EnableDiscard(false);
                gameManager.EnableDrawnCard(false);
                gameManager.swapping = true;
            }
            // We're dragging and dropping the drawn card into the discard pile
            else if (gameManager.draggingOverDiscard && GetComponent<CardDisplay>().isDrawnCard)
            {
                DiscardDrawnCard();
            }
            // We're just dropping the drawn card onto the field for now
            else
            {
                // Discard should be disabled if we have already drawn a card from the deck
                gameManager.EnableDiscard(false);
            }
        }
        transform.localPosition = Vector2.zero;
    }

    private void SwapForComputerCard(GameObject otherCard)
    {
        // Swap parents
        Transform thisCardParent = transform.parent;
        Transform otherCardParent = otherCard.transform.parent;
        transform.SetParent(otherCardParent);
        otherCard.transform.SetParent(thisCardParent);

        transform.localPosition = Vector2.zero;
        otherCard.transform.localPosition = Vector2.zero;
        UpdateCardStatus(gameObject);
        UpdateCardStatus(otherCard);
        // Move swap card to discard pile
        MovePowerCardToDiscard();
        gameManager.swapping = false;
        gameManager.EnableComputerCards(false);
        gameManager.TurnOver();
    }

    private void MovePowerCardToDiscard()
    {
        GameObject powerCard = gameManager.drawnCardTransform.GetChild(0).gameObject;
        powerCard.transform.SetParent(gameManager.discardTransform);
        powerCard.transform.localPosition = Vector2.zero;
        UpdateCardStatus(powerCard);
    }

    private bool CanSwapWithComputer(GameObject otherCard)
    {
        if (gameManager.swapping)
        {
            if ((GetComponent<CardDisplay>().belongsToPlayer &&
                otherCard.GetComponent<CardDisplay>().belongsToComputer) ||
                (GetComponent<CardDisplay>().belongsToComputer &&
                otherCard.GetComponent<CardDisplay>().belongsToPlayer))
            {
                return true;
            }
            return false;
        }
        return false;
    }

    private void DiscardDrawnCard ()
    {
        transform.SetParent(gameManager.discardTransform);
        transform.localPosition = Vector2.zero;
        GetComponent<CardDisplay>().isDrawnCard = false;
        GetComponent<CardDisplay>().isInDiscard = true;
        gameManager.TurnOver();
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
        gameManager.TurnOver();
    }

    private void UpdateCardStatus (GameObject card)
    {
        CardDisplay cardD = card.GetComponent<CardDisplay>();
        // If we belong to the player
        if (card.transform.parent.parent == gameManager.playerField)
        {
            cardD.belongsToPlayer = true;
            cardD.belongsToComputer = false;
            cardD.isInDiscard = false;
            // Only show the front of the card if it's in the bottom two
            cardD.ShowFront(card.transform.parent.tag == "Bottom Card");
        }
        // If we belong to the computer
        else if (card.transform.parent.parent == gameManager.computerField)
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