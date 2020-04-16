using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameManager gameManager;
    private Transform canvas;
    private Transform originalParent;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        canvas = FindObjectOfType<Canvas>().transform;
    }

    public void OnBeginDrag (PointerEventData eventData)
    {
        originalParent = transform.parent;
        CardDisplay cardD = GetComponent<CardDisplay>();
        gameManager.draggingCard = true;
        gameManager.draggedCard = gameObject;
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
                // Disable player cards for now since player can't
                // swap for discard; enable once card is drawn from deck
                gameManager.EnablePlayerCards(false);
                gameManager.EnableDiscard(false);
                gameManager.drawTwoIndex = 0;
            }
            else if (cardD.card.cardType == "peek")
            {
                gameManager.EnablePlayerCards(false);
                gameManager.EnableTopTwoPlayerCards(true);
            }
            else if (cardD.card.cardType == "swap")
            {
                // Don't allow player to swap the swap card for one of their own
                gameManager.EnablePlayerCards(false);
                gameManager.EnableDiscard(false);
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
        else if (transform.parent == gameManager.drawnCardTransform.GetChild(0))
        {
            gameManager.EnableDiscard(true);
            gameManager.drawnCardText.gameObject.SetActive(false);
        }
        // If we're dragging the peek card to the discard to
        // show we're done peeking
        else if (transform.parent == gameManager.powerCardTransform.GetChild(0))
        {
            gameManager.EnableDiscard(true);
        }
        transform.SetParent(canvas);
        transform.SetAsLastSibling();
    }

    public void OnDrag (PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag (PointerEventData eventData)
    {
        CardDisplay cardD = GetComponent<CardDisplay>();
        gameManager.draggingCard = false;
        gameManager.draggedCard = null;
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
            // If we are dragging a peek card onto one of our cards in order to peek at it
            else if (cardD.card.cardType == "peek")
            {
                if (!otherCard.GetComponent<CardDisplay>().belongsToPlayer ||
                    otherCard.transform.parent.tag == "Bottom Card")
                {
                    // Snap peek to drawn card area
                    transform.SetParent(gameManager.drawnCardTransform.GetChild(0));
                    gameManager.drawnCardText.gameObject.SetActive(true);
                    gameManager.EnableDiscard(false);
                }
                else
                {
                    StartCoroutine(DragPeek(otherCard));
                }
            }
        }
        else
        {
            // Check if it's a swap card
            if (cardD.card.cardType == "swap")
            {
                // Start the swap process
                transform.SetParent(gameManager.powerCardTransform.GetChild(0));
                gameManager.StartSwap();
            }
            // Check if it's a draw two
            else if (cardD.card.cardType == "draw two")
            {
                // Start the draw two process
                transform.SetParent(gameManager.powerCardTransform.GetChild(0));
                gameManager.StartDrawTwo();
            }
            else if (cardD.card.cardType == "peek")
            {
                // Check if player is dropping peek card onto power card transform,
                // discard pile, or neither
                if (gameManager.draggingOverPowerCard)
                {
                    transform.SetParent(gameManager.powerCardTransform.GetChild(0));
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
                    transform.SetParent(gameManager.drawnCardTransform.GetChild(0));
                    gameManager.drawnCardText.gameObject.SetActive(true);
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
                transform.SetParent(gameManager.drawnCardTransform.GetChild(0));
                gameManager.drawnCardText.gameObject.SetActive(true);
                // Discard should be disabled if we have already drawn a card from the deck
                gameManager.EnableDiscard(false);
            }
        }
        transform.localPosition = Vector2.zero;
    }

    private IEnumerator DragPeek(GameObject otherCard)
    {
        otherCard.GetComponent<CardDisplay>().ShowFront(true);
        transform.SetParent(gameManager.discardTransform.GetChild(0));
        gameManager.EnableTopTwoPlayerCards(false);
        yield return new WaitForSeconds(1.0f);
        otherCard.GetComponent<CardDisplay>().ShowFront(false);
        gameManager.endTurnButton.interactable = true;
        yield return null;
    }

    private void CheckIfDiscardingPowerCard()
    {
        CardDisplay cardD = GetComponent<CardDisplay>();
        if (cardD.card.cardType == "draw two")
        {
            transform.SetParent(gameManager.powerCardTransform.GetChild(0));
            gameManager.StartDrawTwo();
        }
        else if (cardD.card.cardType == "swap")
        {
            transform.SetParent(gameManager.powerCardTransform.GetChild(0));
            gameManager.StartSwap();
        }
        else if (cardD.card.cardType == "peek")
        {
            if (gameManager.peeking)
            {
                // Stop peeking
                gameManager.MovePowerCardToDiscard();
                gameManager.peeking = false;
                // Flip the peeked card back over
                gameManager.peekedCard.GetComponent<CardDisplay>().ShowFront(false);
                gameManager.peekedCard = null;
                gameManager.endTurnButton.interactable = true;
            }
            // Player can choose to discard or play peek card
            else
            {
                DiscardDrawnCard();
            }
        }
        else
        {
            DiscardDrawnCard();
        }
    }

    private void SwapForComputerCard(GameObject otherCard)
    {
        gameManager.UpdateCardsComputerKnows(gameObject, otherCard);

        // Swap parents
        Transform thisCardParent = originalParent;
        Transform otherCardParent = otherCard.transform.parent;
        transform.SetParent(otherCardParent);
        otherCard.transform.SetParent(thisCardParent);

        transform.localPosition = Vector2.zero;
        otherCard.transform.localPosition = Vector2.zero;
        gameManager.UpdateCardStatus(gameObject);
        gameManager.UpdateCardStatus(otherCard);
        // Move swap card to discard pile
        gameManager.MovePowerCardToDiscard();
        gameManager.swapping = false;
        gameManager.EnableComputerCards(false);
        gameManager.endTurnButton.interactable = true;
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
        transform.SetParent(gameManager.discardTransform.GetChild(0));
        transform.localPosition = Vector2.zero;
        gameManager.UpdateCardStatus(gameObject);
        if (gameManager.drawingTwo)
        {
            if (gameManager.drawTwoIndex >= 2)
            {
                gameManager.DrawTwoOver();
                gameManager.endTurnButton.interactable = true;
            }
            else
            {
                gameManager.CreateTopCard();
            }
        }
        else
        {
            gameManager.endTurnButton.interactable = true;
        }
    }

    private void SwapForCard (GameObject otherCard)
    {
        if (otherCard.GetComponent<CardDisplay>().isInDiscard)
        {
            // Computer now knows this player card, since it
            // came from the discard pile
            gameManager.cardsComputerKnows.Add(transform.parent);
        }
        else if (GetComponent<CardDisplay>().isInDiscard)
        {
            gameManager.cardsComputerKnows.Add(otherCard.transform.parent);
        }
        // Player can either drag the non-player card or the player card
        bool thisCardBelongsToPlayer = GetComponent<CardDisplay>().belongsToPlayer;
        // We're dragging the non-player card
        if (!thisCardBelongsToPlayer)
        {
            transform.SetParent(otherCard.transform.parent);
            otherCard.transform.SetParent(gameManager.discardTransform.GetChild(0));
        }
        // We're dragging the player card
        else
        {
            otherCard.transform.SetParent(originalParent);
            transform.SetParent(gameManager.discardTransform.GetChild(0));
        }
        if (gameManager.drawingTwo)
        {
            gameManager.DrawTwoOver();
        }
        transform.localPosition = Vector2.zero;
        otherCard.transform.localPosition = Vector2.zero;
        gameManager.UpdateCardStatus(gameObject);
        gameManager.UpdateCardStatus(otherCard);
        gameManager.endTurnButton.interactable = true;
    }

    private bool CanSwapDrawnCard (GameObject otherCard)
    {
        if ((GetComponent<CardDisplay>().belongsToPlayer && 
            otherCard.GetComponent<CardDisplay>().isDrawnCard &&
            !otherCard.GetComponent<CardDisplay>().IsPowerCard()) ||
            GetComponent<CardDisplay>().isDrawnCard &&
            otherCard.GetComponent<CardDisplay>().belongsToPlayer &&
            !GetComponent<CardDisplay>().IsPowerCard())
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