using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject roomPanel;
    public GameObject racePanel;
    public GameObject listRoomPanel;
    public GameObject selectCharacterPanel;
    public GameObject winBoardPanel;

    [Header("Main Menu")]
    public TMP_InputField playerNameInput;
    public Button playButton;
    public Button selectPlayerButton;

    [Header("Race UI")]
    public Text countdownText;
    public Text raceTimeText;

    [Header("Select Character")]
    public SelectCharacter selectCharacter;

    public Button selectSpritePlayers;
    public Button selectSocketEffects;
    public Button selectTrailEffects;

    public Button nextPlayerSprite;
    public Button previousPlayerSprite;

    public Button backMainMenu;

    [Header("List Room")]

    public Button createRoomButton;
    public Button joinRoomButton;
    public TMP_InputField roomCodeInput;
    public Button backMainMenu2;

    public ListRoom listRoomScript;


    [Header("Room Panel")]
    public Text roomCodeText;
    public Text roomStatusText;
    public Button readyButton;
    public Button startRaceButton;
    public Button leaveRoomButton;

    public RoomPanel roomPanelScript;

    [Header("Race Board")]
    public Button backInRoom;

    [Header("Win Board")]
    public WinBoardScript winBoardScript;

    [Header("Chat")]
    public GameObject chatRoom;
    public ChatRoom chatRoomScript;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        roomPanelScript = roomPanel.GetComponent<RoomPanel>();
        listRoomScript = listRoomPanel.GetComponent<ListRoom>();
        winBoardScript = winBoardPanel.GetComponent<WinBoardScript>();
        chatRoomScript = chatRoom.GetComponent<ChatRoom>();
    }

    private void Start()
    {
        InitEventMain();
        InitEventListRoom();
        InitEventSelectCharacter();
        InitEventInRoom();
        InitEventWinBoard();
        ShowMainMenu();
    }
    public void HandleChatMessage(string senderName, string message)
    {
        if (chatRoomScript != null)
        {
            chatRoomScript.AddMessage(senderName, message);
        }
    }
    private void InitEventMain()
    {
        playButton.onClick.AddListener(ShowlistRoomPanel);
        selectPlayerButton.onClick.AddListener(ShowSelectChatacterPanel);
    }
    private void InitEventListRoom()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        backMainMenu2.onClick.AddListener(ShowMainMenu);
    }
    private void InitEventInRoom()
    {
        readyButton.onClick.AddListener(ToggleReady);
        startRaceButton.onClick.AddListener(StartRace);
        leaveRoomButton.onClick.AddListener(LeaveRoom);

    }
    private void InitEventSelectCharacter()
    {
        selectSpritePlayers.onClick.AddListener(() => SelectPartOfPlayer(selectSpritePlayers));
        selectSocketEffects.onClick.AddListener(() => SelectPartOfPlayer(selectSocketEffects));
        selectTrailEffects.onClick.AddListener(() => SelectPartOfPlayer(selectTrailEffects));

        nextPlayerSprite.onClick.AddListener(() => selectCharacter.Next(selectCharacter.GetCurrentType()));
        previousPlayerSprite.onClick.AddListener(() => selectCharacter.Previous(selectCharacter.GetCurrentType()));

        backMainMenu.onClick.AddListener(ShowMainMenu);
    }
    private void InitEventWinBoard()
    {
        backInRoom.onClick.AddListener(() => ShowRoomPanel(NetworkManager.Instance.CurrentRoomId));
    }
    private void CreateRoom()
    {
        string playerName = playerNameInput.text;

        string spriteName = selectCharacter.GetPlayerSprites()[selectCharacter.currentIndexes["sprite"]].name;
        string socketEffectName = selectCharacter.GetPrefabSocketEffects()[selectCharacter.currentIndexes["socketEffect"]].name;
        string trailEffectName = selectCharacter.GetPrefabTtrailEffects()[selectCharacter.currentIndexes["trailEffect"]].name;

        Debug.Log($"Creating room with: Sprite={spriteName}, Socket={socketEffectName}, Trail={trailEffectName}");

        if (!GameManager.Instance.playerSpritesDictionary.ContainsKey(spriteName))
        {
            Debug.LogError($"Sprite {spriteName} not found in dictionary!");
            return;
        }
        if (!GameManager.Instance.socketEffects.ContainsKey(socketEffectName))
        {
            Debug.LogError($"Socket effect {socketEffectName} not found in dictionary!");
            return;
        }
        if (!GameManager.Instance.trailEffects.ContainsKey(trailEffectName))
        {
            Debug.LogError($"Trail effect {trailEffectName} not found in dictionary!");
            return;
        }

        var data = new Dictionary<string, object>
        {
            ["playerName"] = playerName,
            ["spriteName"] = spriteName,
            ["socketEffectName"] = socketEffectName,
            ["trailEffectName"] = trailEffectName,
        };

        if (NetworkManager.Instance.IsConnected())
        {
            NetworkManager.Instance.SendMessage("create_room", data);
            Debug.Log("Sending create room request...");
        }
        else
        {
            Debug.LogError("Not connected to server!");
        }
    }


    private void JoinRoom()
    {
        string roomId = roomCodeInput.text.ToUpper();
        string playerName = playerNameInput.text;

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

    private void ToggleReady()
    {
        PlayerStats playerStats = GameManager.Instance.GetLocalPlayerStats();
        var data = new Dictionary<string, object>
        {
            ["isReady"] = !playerStats.isReady
        };
        playerStats.SetReady((bool)data["isReady"]);
        NetworkManager.Instance.SendMessage("player_ready", data);
    }

    private void StartRace()
    {
        if (NetworkManager.Instance.IsHost)
        {
            NetworkManager.Instance.SendMessage("start_race", null);
            Debug.Log("Sending start race request...");
        }
    }

    private void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
    }

    public void ShowMainMenu()
    {
        ResetShowPanel();
        mainMenuPanel.SetActive(true);
    }

    public void ShowRoomPanel(string roomCode)
    {
        ResetShowPanel();
        roomPanel.SetActive(true);
        roomCodeText.text = $"Room Code: {roomCode}";
        startRaceButton.gameObject.SetActive(NetworkManager.Instance.IsHost);
    }

    public void ShowRaceUI()
    {
        racePanel.SetActive(true);
    }

    public void ShowOffRoomPanel()
    {
        roomPanel.SetActive(false);
    }

    public void ShowlistRoomPanel()
    {
        ResetShowPanel();
        NetworkManager.Instance.SendMessage("get_rooms", null);
        listRoomPanel.SetActive(true);
    }

    public void ShowSelectChatacterPanel()
    {
        ResetShowPanel();
        selectCharacterPanel.SetActive(true);
    }

    public void ShowWinBoardPanel()
    {
        ResetShowPanel();
        winBoardPanel.SetActive(true);
    }
    public void AddPlayerInRoom(string playerId, string playerName, bool isReady, GameObject player)
    {
        
        if (roomPanelScript.playersInRoom.ContainsKey(playerId))
        {
            RemovePlayerAndPont(playerId);
        }

        GameObject pointSpawn = Instantiate(roomPanelScript.GetPrefabCardSpawn(), roomPanelScript.GetSpaceSpawnPlayer().transform);
        GameObject pointSpawnPlayer = pointSpawn.transform.GetChild(0).gameObject;

        GameObject objPlayer = Instantiate(player, pointSpawnPlayer.transform);
        objPlayer.transform.localPosition = new Vector3(0, 0, 0);
        objPlayer.transform.rotation = Quaternion.Euler(0, 0, 0);
        objPlayer.layer = LayerMask.NameToLayer("UI");
        objPlayer.GetComponent<SpriteRenderer>().sortingLayerName = "UI";

        if (GameManager.Instance.playerSpritesDictionary.TryGetValue(player.GetComponent<SpriteRenderer>().sprite.name, out Sprite sprite))
        {
            objPlayer.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        PlayerEffects playerEffects = objPlayer.GetComponent<PlayerEffects>();
        GameObject socketEffect = GameManager.Instance.socketEffects[player.GetComponent<PlayerEffects>().GetSocketEffectName()];
        GameObject trailEffect = GameManager.Instance.trailEffects[player.GetComponent<PlayerEffects>().GetTrailEffectName()];
        playerEffects.SetEffect(socketEffect, trailEffect);

        objPlayer.transform.localScale = new Vector3(objPlayer.transform.localScale.x * 20, objPlayer.transform.localScale.y * 20, 0);

        roomPanelScript.cardPlayers.Add(playerId, pointSpawn);
        roomPanelScript.playersInRoom.Add(playerId, objPlayer);

        roomPanelScript.SetReadySprite(playerId, isReady);
        roomPanelScript.SetPlayerNameInCard(playerId, playerName);
    }

    public void RemovePlayerInRoom(string playerId)
    {
        Destroy(roomPanelScript.playersInRoom[playerId]);
        roomPanelScript.playersInRoom.Remove(playerId);
    }

    public void RemovePointInRoom(string playerId)
    {
        Destroy(roomPanelScript.cardPlayers[playerId]);
        roomPanelScript.cardPlayers.Remove(playerId);

    }

    public void RemovePlayerAndPont(string playerId)
    {
        RemovePlayerInRoom(playerId);
        RemovePointInRoom(playerId);
    }
    public void RemovePlayersInRoom()
    {
        foreach (var player in roomPanelScript.playersInRoom.Values)
        {
            if (player != null)
                Destroy(player);
        }
        roomPanelScript.playersInRoom.Clear();
    }

    public void RemovePointsInRoom()
    {
        foreach (var point in roomPanelScript.cardPlayers.Values)
        {
            if (point != null)
                Destroy(point);
        }
        roomPanelScript.cardPlayers.Clear();
    }

    public void RemovePointsAndPlayers()
    {
        RemovePlayersInRoom();
        RemovePointsInRoom();
    }

    public void ShowCountdown(float time)
    {
        countdownText.gameObject.SetActive(true);
        UpdateCountdown(time);
    }
    public void UpdateCountdown(float timeLeft)
    {
        countdownText.text = timeLeft.ToString();
        if (timeLeft <= 0)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
    public void UpdateRaceTime(float time)
    {
        int minutes = (int)(time / 360);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        raceTimeText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
    public void UpdateFinishTimes(Dictionary<string, float> finishTimes)
    {
        if (finishTimes == null) return;

        List<KeyValuePair<string, float>> sortedPlayers = finishTimes.ToList();
        sortedPlayers.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        foreach (var player in sortedPlayers)
        {
            float time = player.Value;
            string playerName = roomPanelScript.GetPlayerName(player.Key);
            winBoardScript.AddPlayerScore(playerName, FormatTime(time));
        }
    }
    private string FormatTime(float time)
    {
        int minutes = (int)(time / 360);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
    public void SelectPartOfPlayer(Button button)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            string text = buttonText.text.ToLower();
            switch (text)
            {
                case "player": selectCharacter.SetCurrentType("sprite"); break;
                case "socket effect": selectCharacter.SetCurrentType("socketEffect"); break;
                case "trail effect": selectCharacter.SetCurrentType("trailEffect"); break;
            }
        }
        else
        {
            Debug.Log("null button select");
        }
    }
    public void ResetShowPanel()
    {
        winBoardPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        roomPanel.SetActive(false);
        racePanel.SetActive(false);
        listRoomPanel.SetActive(false);
        selectCharacterPanel.SetActive(false);
    }

}