using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Deck")]
public class Deck : ScriptableObject {

	public Sprite artwork;
	public static int numberOfCards = 54;
	public Card[] startingDeck;
	public Card[] currentDeck;

}