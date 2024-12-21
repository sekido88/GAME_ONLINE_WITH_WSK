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
    [SerializeField] private GameObject racePanel;

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

    [Header("Race UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI raceTimeText;

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
        racePanel.SetActive(false);
    }

    public void ShowRoomPanel(string roomCode)
    {
        mainMenuPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomCodeText.text = $"Room Code: {roomCode}";

        startRaceButton.gameObject.SetActive(NetworkManager.Instance.IsHost);
    }

    public void ShowRaceUI() {
        racePanel.SetActive(true);
    }

    public void ShowOffRoomPanel() {
        roomPanel.SetActive(false);
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
        PlayerStats playerStats = GameManager.Instance.GetLocalPlayerStats();
        var data = new Dictionary<string, object>
        {
            ["isReady"] = !playerStats.isReady
        };
        playerStats.SetReady((bool)data["isReady"]);
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
    }

    private void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
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
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 100) % 100);
        raceTimeText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

}