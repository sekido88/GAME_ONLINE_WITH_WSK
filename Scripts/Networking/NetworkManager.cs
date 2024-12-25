using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;


public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance => instance;

    private WebSocket webSocket;
    [SerializeField] private string serverUrl = "wss://game-online-with-wsk.onrender.com";


    public System.Action<string> OnConnected;
    public System.Action<string> OnPlayerJoined;
    public System.Action<string> OnPlayerLeft;

    public string CurrentRoomId { get; private set; }
    public bool IsHost { get; private set; }
    public string PlayerId { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await ConnectToServer();
    }

    private async Task ConnectToServer()
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {

        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("Disconnected from server");
        };

        await webSocket.Connect();
    }

    private void HandleMessage(string json)
    {
        NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
        // Debug.Log($"Parsed message action: {json}");

        switch (message.action)
        {

            case "connected":
                PlayerId = message.playerId;
                Debug.Log($"Received player ID: {PlayerId}");
                break;
            case "chat_message":
                UIManager.Instance.HandleChatMessage(message.senderName, message.chatMessage);
                break;
            case "rooms_list":
                HandleGetRooms(message);
                break;

            case "room_created":
                HandleRoomCreated(message);
                break;
            case "room_joined":
                HandleRoomJoined(message);
                break;
            case "game_starting":
                HandleGameStarting(message);
                break;

            case "game_started":
                HandleGameStarted(message);
                break;
            case "player_joined":
                HandlePlayerJoined(message);
                break;
            case "player_ready":
                HandlePlayerReady(message);
                break;
            case "player_moved":
                HandlePlayerMoved(message);
                break;
            case "player_leave":
                HandlePlayerLeave(message);
                break;
            case "player_left":
                HandlePlayerLeft(message);
                break;
            case "error":
                break;
            case "race_time":
                HandleRaceTime(message);
                break;
            case "player_finished":
                HandlePlayerFinished(message);
                break;
            case "race_ended":
                HandleRaceEnded(message);
                break;
        }
    }

    private void HandlePlayerCheckpoint(NetworkMessage message)
    {
        if (GameManager.Instance.players.TryGetValue(message.playerId, out GameObject playerObj))
        {
            PlayerController controller = playerObj.GetComponent<PlayerController>();
            // controller.CurrentCheckpoint = message.currentCheckpoint;
            // controller.CurrentLap = message.currentLap;
        }
    }
    public void SendChatMessage(string senderName,string message)
    {
        var data = new Dictionary<string, object>
        {
            ["senderName"] = senderName,
            ["chatMessage"] = message
        };
        SendMessage("chat_message", data);
    }
    private void HandleRoomCreated(NetworkMessage message)
    {
        CurrentRoomId = message.roomId;
        IsHost = message.isHost;

        // SelectCharacter selectCharacter = UIManager.Instance.selectCharacter;
        // string spriteName = selectCharacter.GetPlayerSprites()[selectCharacter.currentIndexes["sprite"]].name;
        // string socketEffectName = selectCharacter.GetPrefabSocketEffects()[selectCharacter.currentIndexes["socketEffect"]].name;
        // string trailEffectName = selectCharacter.GetPrefabTtrailEffects()[selectCharacter.currentIndexes["trailEffect"]].name;

        Debug.Log(message.spriteName);
        Debug.Log(message.socketEffectName);
        Debug.Log(message.trailEffectName);

        if (GameManager.Instance.playerSpritesDictionary.TryGetValue(message.spriteName, out Sprite sprite) &&
            GameManager.Instance.socketEffects.TryGetValue(message.socketEffectName, out GameObject socketEffect) &&
            GameManager.Instance.trailEffects.TryGetValue(message.trailEffectName, out GameObject trailEffect))
        {
            GameManager.Instance.SpawnPlayer(message.playerId, sprite, socketEffect, trailEffect, Quaternion.identity);
            UIManager.Instance.ShowRoomPanel(CurrentRoomId);
            UIManager.Instance.AddPlayerInRoom(message.playerId, message.playerName, message.isReady, GameManager.Instance.players[message.playerId]);
        }
        
        // CheckpointManager.Instance.localPlayer = 
    
    }

    private void HandleRaceTime(NetworkMessage message)
    {
        if (GameManager.Instance.isRaceStarted)
        {
            UIManager.Instance.UpdateRaceTime(message.currentTime);
            UIManager.Instance.UpdateFinishTimes(message.finishTimes);
        }
    }

    private void HandlePlayerFinished(NetworkMessage message)
    {
        UIManager.Instance.ShowWinBoardPanel();

        Debug.Log(message.finishTimes[0].time);
        Debug.Log(message.finishTimes[0].playerId);
        
        UIManager.Instance.UpdateFinishTimes(message.finishTimes);

        if (message.playerId == PlayerId)
        {
            GameManager.Instance.players[PlayerId].GetComponent<PlayerMovement>().canMove = false;
        }
    }

    private void HandleRoomJoined(NetworkMessage message)
    {
        CurrentRoomId = message.roomId;
        IsHost = message.isHost;
        foreach (var player in message.players)
        {
            if (!GameManager.Instance.players.ContainsKey(player.id))
            {
                if (GameManager.Instance.playerSpritesDictionary.TryGetValue(player.spriteName, out Sprite sprite) &&
                    GameManager.Instance.socketEffects.TryGetValue(player.socketEffectName, out GameObject socketEffect) &&
                    GameManager.Instance.trailEffects.TryGetValue(player.trailEffectName, out GameObject trailEffect))
                {
                    GameManager.Instance.SpawnPlayer(player.id, sprite, socketEffect, trailEffect, Quaternion.identity);
                    UIManager.Instance.AddPlayerInRoom(player.id, player.name, player.isReady, GameManager.Instance.players[player.id]);
                }
            }
        }
        UIManager.Instance.ShowRoomPanel(CurrentRoomId);
        Debug.Log($"Joined room: {CurrentRoomId}");
    }

    private void HandleGameStarting(NetworkMessage message)
    {
        UIManager.Instance.ShowOffRoomPanel();
        GameManager.Instance.StartCountdown();
        GameManager.Instance.SetPlayerTarget();
    }

    private void HandleGameStarted(NetworkMessage message)
    {
        Debug.Log("Game has started!");
        GameManager.Instance.StartRace();
    }

    private void HandlePlayerJoined(NetworkMessage message)
    {
        foreach (var player in message.players)
        {
            Sprite sprite = GameManager.Instance.playerSpritesDictionary[player.spriteName];
            GameObject socketEffect = GameManager.Instance.socketEffects[player.socketEffectName];
            GameObject trailEffect = GameManager.Instance.trailEffects[player.trailEffectName];

            GameManager.Instance.SpawnPlayer(player.id, sprite, socketEffect, trailEffect, Quaternion.identity);

            UIManager.Instance.AddPlayerInRoom(player.id, player.name, player.isReady, GameManager.Instance.players[player.id]);

        }
        Debug.Log($"Player joined: {message.playerId}");
    }

    private void HandlePlayerReady(NetworkMessage message)
    {
        foreach (var player in message.players)
        {
            UIManager.Instance.roomPanelScript.SetInfoCard(player.id, player.name, player.isReady);
        }
    }
    private void HandleRaceEnded(NetworkMessage message)
    {
        GameManager.Instance.isRaceStarted = false;
        UIManager.Instance.ShowWinBoardPanel();
        UIManager.Instance.UpdateFinishTimes(message.finishTimes);
    }

    private void HandlePlayerLeave(NetworkMessage message)
    {
        if (GameManager.Instance.players.ContainsKey(message.playerId))
        {
            Destroy(GameManager.Instance.players[message.playerId]);
            GameManager.Instance.players.Remove(message.playerId);
            UIManager.Instance.RemovePlayerAndPont(message.playerId);
        }

        Debug.Log($"Player leave: {message.playerId}");
    }
    private void HandlePlayerLeft(NetworkMessage message)
    {
        if (GameManager.Instance.players.ContainsKey(message.playerId))
        {
            Destroy(GameManager.Instance.players[message.playerId]);
            GameManager.Instance.players.Remove(message.playerId);
            UIManager.Instance.RemovePlayerInRoom(message.playerId);
        }

        Debug.Log($"Player left: {message.playerId}");
    }
    private void HandlePlayerMoved(NetworkMessage message)
    {
        if (GameManager.Instance.players.TryGetValue(message.playerId, out GameObject playerObj))
        {
            if (message.playerId != PlayerId)
            {
                playerObj.transform.position = message.position;
                playerObj.transform.rotation = message.rotation;
            }
        }
    }
    private void HandleGetRooms(NetworkMessage message)
    {
        foreach (var room in message.rooms)
        {
            UIManager.Instance.listRoomScript.AddRoom(room.id, room.playerCount, room.status);
        }
    }
    public async void SendMessage(string action, Dictionary<string, object> data = null)
    {
        try
        {
            NetworkMessage messageObj = new NetworkMessage
            {
                action = action,
                playerId = PlayerId
            };


            switch (action)
            {
                case "chat_message" :
                    if (data.ContainsKey("chatMessage") && data.ContainsKey("senderName")) {
                        messageObj.senderName = data["senderName"].ToString();
                        messageObj.chatMessage = data["chatMessage"].ToString();
                    }
                break;
                case "create_room":
                    if (data.ContainsKey("playerName"))
                    {
                        messageObj.playerName = data["playerName"].ToString();
                        messageObj.spriteName = data["spriteName"].ToString();
                        messageObj.socketEffectName = data["socketEffectName"].ToString();
                        messageObj.trailEffectName = data["trailEffectName"].ToString();

                    };
                    break;
                case "join_room":
                    if (data != null && data.ContainsKey("roomId"))
                    {
                        messageObj.roomId = data["roomId"].ToString();
                        messageObj.playerName = data["playerName"].ToString();
                        messageObj.spriteName = data["spriteName"].ToString();
                        messageObj.socketEffectName = data["socketEffectName"].ToString();
                        messageObj.trailEffectName = data["trailEffectName"].ToString();
                    }
                    break;
                case "start_race":
                    break;
                case "player_ready":
                    if (data != null && data.ContainsKey("isReady"))
                    {
                        messageObj.isReady = (bool)data["isReady"];
                    }
                    break;
                case "player_moved":
                    if (data != null && data.ContainsKey("position") && data.ContainsKey("rotation"))
                    {
                        messageObj.position = (Vector3)data["position"];
                        messageObj.rotation = (Quaternion)data["rotation"];
                        // Debug.Log($"Processing player_moved: pos={messageObj.position}, rot={messageObj.rotation}");
                    }
                    break;
                case "leave_room":
                    if (data != null && data.ContainsKey("roomId"))
                    {
                        messageObj.roomId = data["roomId"].ToString();
                    }
                    break;
            }

            string json = JsonUtility.ToJson(messageObj);
            // Debug.Log($"Final message to send: {json}");

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendText(json);
                // Debug.Log("Message sent successfully");
            }
            else
            {
                Debug.LogError($"WebSocket not open. Current state: {webSocket.State}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendMessage: {e.Message}");
            if (data != null)
            {
                Debug.LogError("error send message");
            }
        }
    }
    public void LeaveRoom()
    {
        UIManager.Instance.ShowMainMenu();
        UIManager.Instance.RemovePointsAndPlayers();
        GameManager.Instance.ResetPlayers();

        var data = new Dictionary<string, object>()
        {
            ["roomId"] = CurrentRoomId,
        };

        SendMessage("leave_room", data);
        CurrentRoomId = null;
        IsHost = false;

    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
        if (GameManager.Instance.isRaceStarted)
        {
            SendLocalPlayerPosition();
            SendGetTimeRace();
        }

    }

    public void SendLocalPlayerPosition()
    {
        if (GameManager.Instance.players.TryGetValue(PlayerId, out GameObject localPlayer))
        {
            SendPlayerMove(localPlayer.transform.position, localPlayer.transform.rotation);
        }

    }
    public bool IsConnected()
    {
        return webSocket != null && webSocket.State == WebSocketState.Open;
    }
    public void SendPlayerMove(Vector3 position, Quaternion rotation)
    {
        var data = new Dictionary<string, object>
        {
            ["position"] = position,
            ["rotation"] = rotation
        };
        SendMessage("player_moved", data);
    }

    private void SendGetTimeRace()
    {
        SendMessage("get_race_time", null);
    }

}

