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

    private string playerId;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();


    public System.Action<string> OnConnected;
    public System.Action<string> OnPlayerJoined;
    public System.Action<string> OnPlayerLeft;


    private string currentRoomId;


    public string CurrentRoomId { get; private set; }
    public bool IsHost { get; private set; }

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
            Debug.Log("Connected to server");
            playerId = System.Guid.NewGuid().ToString();
            OnConnected?.Invoke(playerId);

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
                playerId = message.playerId;
                Debug.Log($"Received player ID: {playerId}");
                break;
            case "room_created":
                HandleRoomCreated(message);
                break;
            case "room_joined":
                HandleRoomJoined(message);
                break;
            case "player_joined":
                HandlePlayerJoined(message);
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
        if (players.TryGetValue(message.playerId, out GameObject playerObj))
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
        UIManager.Instance.UpdatePlayerList(message.players);
        Debug.Log($"Room created: {CurrentRoomId}");
        UIManager.Instance.ShowRoomPanel(CurrentRoomId);
    }

    private void HandleRoomJoined(NetworkMessage message)
    {
        CurrentRoomId = message.roomId;
        IsHost = message.isHost;
        UIManager.Instance.ShowRoomPanel(CurrentRoomId);
        UIManager.Instance.UpdatePlayerList(message.players);
        Debug.Log($"Joined room: {CurrentRoomId}");
    }

    private void HandleGameStarting(NetworkMessage message)
    {
        // UIManager.Instance.StartCountdown(3); // 3 giây đếm ngược
    }

    private void HandlePlayerJoined(NetworkMessage message)
    {
        SpawnPlayer(message.playerId, message.position, message.rotation);
        UIManager.Instance.UpdatePlayerList(message.players);
        Debug.Log($"Player joined: {message.playerId}");
    }

    private void SpawnPlayer(string playerId, Vector3 position, Quaternion rotation)
    {
        if (!players.ContainsKey(playerId))
        {
            GameObject playerObj = Instantiate(playerPrefab, position, rotation);
            PlayerController controller = playerObj.GetComponent<PlayerController>();
            controller.Initialize(playerId, $"Player_{playerId.Substring(0, 4)}", playerId == this.playerId);
            players.Add(playerId, playerObj);
        }
    }


    private void HandlePlayerLeft(NetworkMessage message)
    {
        if (players.ContainsKey(message.playerId))
        {
            Destroy(players[message.playerId]);
            players.Remove(message.playerId);
        }
        UIManager.Instance.UpdatePlayerList(message.players);
        Debug.Log($"Player left: {message.playerId}");
    }
    private void HandlePlayerMoved(NetworkMessage message)
    {
        if (players.TryGetValue(message.playerId, out GameObject playerObj))
        {
            if (message.playerId != playerId)
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
                playerId = playerId
            };


            switch (action)
            {
                case "create_room":
                    if(data.ContainsKey("playerName")) 
                    {
                        messageObj.playerName = data["playerName"].ToString();
                    };
                    break;

                case "join_room":
                    if (data != null && data.ContainsKey("roomId"))
                    {
                        messageObj.roomId = data["roomId"].ToString();
                        Debug.Log($"Joining room with ID: {messageObj.roomId}");
                    }
                    break;

                case "player_ready":
                    if (data != null && data.ContainsKey("isReady"))
                    {
                        messageObj.isReady = (bool)data["isReady"];
                        Debug.Log($"Processing player_ready: {messageObj.isReady}");
                    }
                    break;

                case "player_moved":
                    if (data != null && data.ContainsKey("position") && data.ContainsKey("rotation"))
                    {
                        messageObj.position = (Vector3)data["position"];
                        messageObj.rotation = (Quaternion)data["rotation"];
                        Debug.Log($"Processing player_moved: pos={messageObj.position}, rot={messageObj.rotation}");
                    }
                    break;
            }

            string json = JsonUtility.ToJson(messageObj);
            Debug.Log($"Final message to send: {json}");

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendText(json);
                Debug.Log("Message sent successfully");
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

        // // Gửi vị trí player local
        // if (players.TryGetValue(playerId, out GameObject localPlayer))
        // {
        //     SendMessage("player_moved", new
        //     {
        //         playerId = playerId,
        //         position = localPlayer.transform.position,
        //         rotation = localPlayer.transform.rotation
        //     });
        // }
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

   public void SendPlayerReady(bool isReady)
    {
        var data = new Dictionary<string, object>
        {
            ["isReady"] = isReady
        };
        SendMessage("player_ready", data);
    }

}

