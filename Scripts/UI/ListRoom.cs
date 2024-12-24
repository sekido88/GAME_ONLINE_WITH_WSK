using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListRoom : MonoBehaviour
{
    public GameObject roomContainer;
    public GameObject prefabRoomInfo;

    public Dictionary<string,GameObject> roomsInfo = new Dictionary<string, GameObject>();
    public Dictionary<string,Button> listButton = new Dictionary<string,Button>();

    private void Start() {

    }
    public void AddRoom(string roomId,int playerCount, string status) {

        if(roomsInfo.ContainsKey(roomId)) return;

        GameObject roomInfo = Instantiate(prefabRoomInfo, roomContainer.transform);

        Button button  = roomInfo.transform.GetChild(3).GetComponent<Button>();
        button.onClick.AddListener(() => JoinRoom(roomId));

        roomInfo.transform.GetChild(0).GetComponent<Text>().text = roomId;
        roomInfo.transform.GetChild(1).GetComponent<Text>().text = playerCount.ToString();
        roomInfo.transform.GetChild(2).GetComponent<Text>().text = status;

        roomsInfo.Add(roomId, roomInfo);
        listButton.Add(roomId, button);
    }

    private void JoinRoom(string roomId)
    {
        string playerName = UIManager.Instance.playerNameInput.text;

        SelectCharacter selectCharacter = UIManager.Instance.selectCharacter;

        string spriteName = selectCharacter.GetPlayerSprites()[selectCharacter.currentIndexes["sprite"]].name;
        string socketEffectName = selectCharacter.GetPrefabSocketEffects()[selectCharacter.currentIndexes["socketEffect"]].name;
        string trailEffectName = selectCharacter.GetPrefabTtrailEffects()[selectCharacter.currentIndexes["trailEffect"]].name;

        var data = new Dictionary<string, object>
        {
            ["playerName"] = playerName,
            ["roomId"] = roomId,
            ["spriteName"] = spriteName,
            ["socketEffectName"] = socketEffectName,
            ["trailEffectName"] = trailEffectName,
        };
        if (!string.IsNullOrEmpty(roomId))
        {
            NetworkManager.Instance.SendMessage("join_room", data);
            Debug.Log("Sending join room request..." + roomId);
        }
    }
}
