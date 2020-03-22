using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PowerCardTransform : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.draggingCard)
        {
            gameManager.draggingOverPowerCard = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameManager.draggingOverPowerCard = false;
    }
}
