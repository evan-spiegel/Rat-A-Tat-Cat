using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiscardPile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameManager gameManager;
    float startingAlpha;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startingAlpha = GetComponent<Image>().color.a;
    }

    /*
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager.draggingCard)
        {
            gameManager.draggingOverDiscard = true;
            // Only needs to light up in nothing in discard
            if (transform.GetChild(0).childCount == 0)
            {
                // Light up when dragging a card over it to show we
                // can drop it here
                Color currentColor = GetComponent<Image>().color;
                Color hoverColor = new Color(currentColor.r, currentColor.g,
                    currentColor.b, gameManager.cardTransformHoverAlpha);
                GetComponent<Image>().color = hoverColor;
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        gameManager.draggingOverDiscard = false;
        if (transform.GetChild(0).childCount == 0)
        {
            Color currentColor = GetComponent<Image>().color;
            Color leaveHoverColor = new Color(currentColor.r, currentColor.g,
                currentColor.b, startingAlpha);
            GetComponent<Image>().color = leaveHoverColor;
        }
    }
    */

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.draggingCard)
        {
            gameManager.draggingOverDiscard = true;
            // Only needs to light up in nothing in discard
            if (transform.GetChild(0).childCount == 0)
            {
                // Light up when dragging a card over it to show we
                // can drop it here
                Color currentColor = GetComponent<Image>().color;
                Color hoverColor = new Color(currentColor.r, currentColor.g,
                    currentColor.b, gameManager.cardTransformHoverAlpha);
                GetComponent<Image>().color = hoverColor;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameManager.draggingOverDiscard = false;
        if (transform.GetChild(0).childCount == 0)
        {
            Color currentColor = GetComponent<Image>().color;
            Color leaveHoverColor = new Color(currentColor.r, currentColor.g,
                currentColor.b, startingAlpha);
            GetComponent<Image>().color = leaveHoverColor;
        }
    }
}