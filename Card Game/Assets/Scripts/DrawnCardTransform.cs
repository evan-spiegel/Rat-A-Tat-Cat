using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawnCardTransform : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameManager gameManager;
    float startingAlpha;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        startingAlpha = GetComponent<Image>().color.a;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.draggingCard)
        {
            // Light up when dragging a card over it to show we
            // can drop it here
            Color currentColor = GetComponent<Image>().color;
            Color hoverColor = new Color(currentColor.r, currentColor.g,
                currentColor.b, gameManager.cardTransformHoverAlpha);
            GetComponent<Image>().color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color currentColor = GetComponent<Image>().color;
        Color leaveHoverColor = new Color(currentColor.r, currentColor.g,
            currentColor.b, startingAlpha);
        GetComponent<Image>().color = leaveHoverColor;
    }
}