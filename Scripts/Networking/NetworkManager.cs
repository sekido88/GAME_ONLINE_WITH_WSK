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
    [SerializeField] private string serverUrl = "ws://localhost:8080";
    [SerializeField] private GameObject playerPrefab;




    public System.Action<string> OnConnected;
    public System.Action<string> OnPlayerJoined;
    public System.Action<string> OnPlayerLeft;


    private string currentRoomId;


    public string CurrentRoomId { get; private set; }
    public bool IsHost { get; private set; }
    public string PlayerId { get; private set; }

    private float sendInterval = 0.1f;
    private float timeSinceLastSend = 0f;
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
        Debug.Log($"Parsed message action: {json}");

        switch (message.action)
        {
            case "connected":
                PlayerId = message.playerId;
                Debug.Log($"Received player ID: {PlayerId}");
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
            case "player_left":
                HandlePlayerLeft(message);
                break;
            case "error":
                // HandleError(message);
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


    private void HandleRoomCreated(NetworkMessage message)
    {
        CurrentRoomId = message.roomId;
        IsHost = message.isHost;
        UIManager.Instance.ShowRoomPanel(CurrentRoomId);

  
        GameManager.Instance.SpawnPlayer(message.playerId, Quaternion.identity);
     
    }

    private void HandleRoomJoined(NetworkMessage message)
    {
        CurrentRoomId = message.roomId;
        IsHost = message.isHost;
        UIManager.Instance.ShowRoomPanel(CurrentRoomId);
        Debug.Log($"Joined room: {CurrentRoomId}");
    }

    private void HandleGameStarting(NetworkMessage message)
    {
        UIManager.Instance.ShowOffRoomPanel();
        GameManager.Instance.StartCountdown();
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
            if(!GameManager.Instance.players.ContainsKey(player.id))
                GameManager.Instance.SpawnPlayer(player.id, player.rotation);
        }
        Debug.Log($"Player joined: {message.playerId}");
    }

    private void HandlePlayerReady(NetworkMessage message)
    {
        Debug.Log($"Player {message.playerId} ready status: {message.isReady}");
    }

    private void HandlePlayerLeft(NetworkMessage message)
    {
        if (GameManager.Instance.players.ContainsKey(message.playerId))
        {
            Destroy(GameManager.Instance.players[message.playerId]);
            GameManager.Instance.players.Remove(message.playerId);
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
                case "create_room":
                    if (data.ContainsKey("playerName"))
                    {
                        messageObj.playerName = data["playerName"].ToString();
                    };
                    break;

                case "join_room":
                    if (data != null && data.ContainsKey("roomId"))
                    {
                        messageObj.roomId = data["roomId"].ToString();
                        messageObj.playerName = data["playerName"].ToString();
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
        if (!string.IsNullOrEmpty(CurrentRoomId))
        {
            SendMessage("leave_room", new { roomId = CurrentRoomId });
            CurrentRoomId = null;
            IsHost = false;
            UIManager.Instance.ShowMainMenu();
            if (GameManager.Instance.players.ContainsKey(NetworkManager.Instance.PlayerId))
            {
                Destroy(GameManager.Instance.players[NetworkManager.Instance.PlayerId]);
                GameManager.Instance.players.Remove(NetworkManager.Instance.PlayerId);
            }
        }

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


}

