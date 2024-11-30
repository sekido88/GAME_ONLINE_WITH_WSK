using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Main Menu")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Room Panel")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startRaceButton;
    [SerializeField] private TextMeshProUGUI playerListText;


    [Header("Room Info")]
    [SerializeField] private TextMeshProUGUI roomStatusText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button leaveRoomButton;
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
    }

    private void Start()
    {
        // Đăng ký các event
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        readyButton.onClick.AddListener(ToggleReady);
        startRaceButton.onClick.AddListener(StartRace);
        leaveRoomButton.onClick.AddListener(LeaveRoom);
        // Mặc định hiển thị main menu
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public void ShowRoomPanel(string roomCode)
    {
        mainMenuPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomCodeText.text = $"Room Code: {roomCode}";

        startRaceButton.gameObject.SetActive(NetworkManager.Instance.IsHost);
    }

    private void CreateRoom()
    {

        string playerName = playerNameInput.text;

        var data = new Dictionary<string, object>
        {
            ["playerName"] = playerName
        };

        if (NetworkManager.Instance.IsConnected() )
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

        var data = new Dictionary<string, object>
        {
            ["playerName"] = playerName,
            ["roomId"] = roomId
        };
        if (!string.IsNullOrEmpty(roomId))
        {
            NetworkManager.Instance.SendMessage("join_room", data);
            Debug.Log("Sending join room request..." + roomId);
        }
    }

    private void ToggleReady()
    {
        // Sẽ implement sau
    }

    private void StartRace()
    {
        if (NetworkManager.Instance.IsHost) {
            NetworkManager.Instance.SendMessage("start_race", null);
            Debug.Log("Sending start race request...");
        }
    }

    public void UpdatePlayerList(List<PlayerInfo> data)
    {
        playerListText.text = "";
        foreach (var player in data)
        {
            playerListText.text += $"Player: {player.name} - {(player.isReady ? "Ready" : "Not Ready")}\n";
        }
        Debug.Log("Player list updated: " + playerListText.text);
        Debug.Log(data.ToString());
    }

    private void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
    }
}