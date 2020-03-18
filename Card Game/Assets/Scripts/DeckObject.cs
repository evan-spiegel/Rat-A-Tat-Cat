using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckObject : MonoBehaviour, IPointerDownHandler
{
    private GameManager gameManager;
    public void OnPointerDown(PointerEventData eventData)
    {
        // Instantiate a card at the mouse position
        //gameManager.DrawCard();
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        
    }
}