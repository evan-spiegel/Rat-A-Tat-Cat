using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerCardTransform : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameManager gameManager;
    float startingAlpha;
    Color hoverColor, leaveHoverColor;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startingAlpha = GetComponent<Image>().color.a;
        Color currentColor = GetComponent<Image>().color;
        hoverColor = new Color(currentColor.r, currentColor.g,
            currentColor.b, gameManager.cardTransformHoverAlpha);
        leaveHoverColor = new Color(currentColor.r, currentColor.g,
            currentColor.b, startingAlpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.draggingCard)
        {
            gameManager.draggingOverPowerCard = true;
            if (gameManager.draggedCard.GetComponent<CardDisplay>().IsPowerCard())
            {
                // Light up when dragging a card over it to show we
                // can drop it here
                ShowHighlightColor(true);
            }
        }
    }

    public void ShowHighlightColor(bool show)
    {
        GetComponent<Image>().color = show ? hoverColor : leaveHoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameManager.draggingOverPowerCard = false;
        if (gameManager.draggedCard == null ||
            (gameManager.draggedCard.GetComponent<CardDisplay>().card.cardType != "draw two" &&
            gameManager.draggedCard.GetComponent<CardDisplay>().card.cardType != "swap"))
        {
            ShowHighlightColor(false);
        }
    }
}