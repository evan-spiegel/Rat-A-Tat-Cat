using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardPile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("enter");
        if (gameManager.draggingCard)
        {
            gameManager.draggingOverDiscard = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameManager.draggingOverDiscard = false;
    }
}