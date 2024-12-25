using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinBoardScript : MonoBehaviour 
{
   public GameObject spaceDisplay;
   public GameObject prefabCardPlayer;
   
   private Dictionary<string, GameObject> cardPlayers = new Dictionary<string, GameObject>();

   public void AddPlayerScore(string playerName, string time)
   {
       GameObject card = Instantiate(prefabCardPlayer, spaceDisplay.transform);
       
       Text nameText = card.transform.Find("PlayerName").GetComponent<Text>();
       Text timeText = card.transform.Find("PlayerTime").GetComponent<Text>();
       
       nameText.text = playerName;
       timeText.text = time;

       cardPlayers.Add(playerName, card);
   }

   public void ClearBoard()
   {
       foreach(var card in cardPlayers.Values)
       {
           Destroy(card);
       }
       cardPlayers.Clear();
   }
}

