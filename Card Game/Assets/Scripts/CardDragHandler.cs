using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnBeginDrag (PointerEventData eventData)
    {
        CardDisplay cardD = GetComponent<CardDisplay>();
        gameManager.draggingCard = true;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        // If we are the top card of the deck
        if (transform.parent == gameManager.deckTransform)
        {
            // First, check if it's a power card.
            // If it is, don't put it in the discard right away;
            // have it on the field at first before the action
            // is completed, then move it to the discard after
            // to show it's completed and the turn is over
            if (cardD.card.cardType == "draw two")
            {
                transform.SetParent(gameManager.powerCardTransform);
                // Disable player cards for now since player can't
                // swap for discard; enable once card is drawn from deck
                gameManager.EnablePlayerCards(false);
                gameManager.drawTwoIndex = 0;
            }
            else if (cardD.card.cardType == "peek")
            {
                
            }
            else if (cardD.card.cardType == "swap")
            {
                transform.SetParent(gameManager.powerCardTransform);
                // Don't allow player to swap the swap card for one of their own
                gameManager.EnablePlayerCards(false);
                gameManager.drawTwoIndex = 0;
            }
            // It's a number card
            else
            {
                if (gameManager.drawingTwo)
                {
                    // If this is the first card drawn for draw two,
                    // this should be 1 after increment
                    gameManager.drawTwoIndex++;
                }
                transform.SetParent(gameManager.drawnCardTransform);
                gameManager.EnablePlayerCards(true);
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
        // If we're dragging the peek card to the discard to
        // show we're done peeking
        else if (transform.parent == gameManager.powerCardTransform)
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
        CardDisplay cardD = GetComponent<CardDisplay>();
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
            else if (cardD.isDrawnCard && otherCard.GetComponent<CardDisplay>().isInDiscard)
            {
                // Check if we're trying to discard a power card
                CheckIfDiscardingPowerCard();
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
            if (cardD.card.cardType == "swap")
            {
                // Start the swap process
                gameManager.StartSwap();
            }
            // Check if it's a draw two
            else if (cardD.card.cardType == "draw two")
            {
                // Start the draw two process
                gameManager.StartDrawTwo();
            }
            else if (cardD.card.cardType == "peek")
            {
                // Check if player is dropping peek card onto power card transform,
                // discard pile, or neither
                if (gameManager.draggingOverPowerCard)
                {
                    transform.SetParent(gameManager.powerCardTransform);
                    gameManager.StartPeek();
                }
                else if (gameManager.draggingOverDiscard)
                {
                    CheckIfDiscardingPowerCard();
                }
                else
                {
                    // Just place peek on the field while player decides whether
                    // to use it or not (by dragging it to power card transform)
                    transform.SetParent(gameManager.drawnCardTransform);
                    gameManager.EnableDiscard(false);
                }
            }
            // We're dragging and dropping the drawn card into the discard pile
            else if (gameManager.draggingOverDiscard && cardD.isDrawnCard)
            {
                CheckIfDiscardingPowerCard();
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

    private void CheckIfDiscardingPowerCard()
    {
        CardDisplay cardD = GetComponent<CardDisplay>();
        if (cardD.card.cardType == "draw two")
        {
            gameManager.StartDrawTwo();
        }
        else if (cardD.card.cardType == "swap")
        {
            gameManager.StartSwap();
        }
        else if (cardD.card.cardType == "peek")
        {
            if (gameManager.peeking)
            {
                // Stop peeking
                MovePowerCardToDiscard();
                gameManager.peeking = false;
                // Flip the peeked card back over
                gameManager.peekedCard.GetComponent<CardDisplay>().ShowFront(false);
                gameManager.peekedCard = null;
                gameManager.TurnOver();
            }
            else
            {
                DiscardDrawnCard();
            }
        }
        // Player can choose to discard or play peek card
        else
        {
            DiscardDrawnCard();
        }
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
        if (gameManager.drawingTwo)
        {
            if (gameManager.drawTwoIndex >= 2)
            {
                gameManager.DrawTwoOver();
                gameManager.TurnOver();
                MovePowerCardToDiscard();
            }
            else
            {
                gameManager.CreateTopCard();
            }
        }
        else
        {
            gameManager.TurnOver();
        }
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
        if (gameManager.drawingTwo)
        {
            gameManager.DrawTwoOver();
            MovePowerCardToDiscard();
        }
        transform.localPosition = Vector2.zero;
        otherCard.transform.localPosition = Vector2.zero;
        UpdateCardStatus(gameObject);
        UpdateCardStatus(otherCard);
        gameManager.TurnOver();
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
}