using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] private GameObject spaceSpawnPlayer;
    [SerializeField] private GameObject prefabCardSpawn;
    [SerializeField] private Sprite notReadySprite;
    [SerializeField] private Sprite isReadySprite;


    public Dictionary<string, GameObject> cardPlayers = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> playersInRoom = new Dictionary<string, GameObject>();
    public void SetPlayerNameInCard(string id, string playerName)
    {
        cardPlayers[id].transform.Find("PlayerName").gameObject.GetComponent<Text>().text = playerName;
    }

    public void SetReadySprite(string id, bool isReady)
    {
        Sprite sprite = (isReady) ? isReadySprite : notReadySprite;
        string ready = (isReady) ? "Ready" : "Not Ready";

        GameObject obj = cardPlayers[id].transform.Find("Ready").gameObject;
        obj.GetComponent<Image>().sprite = sprite;
        obj.transform.GetChild(0).gameObject.GetComponent<Text>().text = ready;
    }

    public GameObject GetSpaceSpawnPlayer()
    {
        return spaceSpawnPlayer;
    }

    public GameObject GetPrefabCardSpawn()
    {
        return prefabCardSpawn;
    }

    public void SetInfoCard(string id, string playerName, bool isReady) {
        SetPlayerNameInCard(id, playerName);
        SetReadySprite(id, isReady);
    }

    public String GetPlayerName(string playerId) {
        if(cardPlayers.ContainsKey(playerId))
             return cardPlayers[playerId].transform.GetChild(1).gameObject.GetComponent<Text>().text;
        return null;
    }
}
