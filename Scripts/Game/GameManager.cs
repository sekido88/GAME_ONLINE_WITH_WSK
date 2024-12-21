using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player 
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playersGameObject;
    [SerializeField] private Sprite[] playerSprites;

    // Effect
    [SerializeField] private GameObject socketEffectsPrefab;
    [SerializeField] private Dictionary<string,GameObject> socketEffects = new Dictionary<string, GameObject>();
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float countdownTime = 3f;

    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    public Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();  
    public bool isRaceStarted = false;
    private float raceTimer = 0f;

    [SerializeField] private CameraFollow cameraFollow;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Application.runInBackground = true;
        cameraFollow = GameObject.Find("Camera Holder").GetComponent<CameraFollow>();
        InitSocketEffects();
    }

    private void InitSocketEffects() {
        for(int i = 0; i < socketEffectsPrefab.transform.childCount; i++) {
            Debug.Log(socketEffectsPrefab.transform.GetChild(i).name);
            socketEffects[socketEffectsPrefab.transform.GetChild(i).name] = socketEffectsPrefab.transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (isRaceStarted)
        {
            raceTimer += Time.deltaTime;
            UIManager.Instance.UpdateRaceTime(raceTimer);
        }
    }
    public void StartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        UIManager.Instance.ShowCountdown(countdownTime);

        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
            UIManager.Instance.UpdateCountdown(timeLeft);
        }
        UIManager.Instance.UpdateCountdown(0);
    }
    public PlayerStats GetLocalPlayerStats()
    {
        Debug.Log($"Getting local player stats: {NetworkManager.Instance.PlayerId}");
        if (playerStats.TryGetValue(NetworkManager.Instance.PlayerId, out PlayerStats stats))
        {
            return stats;
        }
        return null;
    }
    public void SpawnPlayer(string playerId, Quaternion rotation)
    {

        if(players.ContainsKey(playerId)) return;

        GameObject playerObj = Instantiate(playerPrefab, spawnPoints[players.Count].position, rotation,playersGameObject.transform);
        
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.Initialize(playerId, $"Player_{playerId}", playerId == NetworkManager.Instance.PlayerId);
        players.Add(playerId, playerObj);
        playerStats.Add(playerId, playerObj.GetComponent<PlayerStats>());

        if(playerId == NetworkManager.Instance.PlayerId) {
            cameraFollow.setTarget(playerObj.transform);
            Debug.Log("Setting camera target to local player");
        }

        Debug.Log($"Spawned player: {playerId}");
    }



    public void StartRace()
    {
        isRaceStarted = true;
        raceTimer = 0f;
        UIManager.Instance.ShowRaceUI();
        SetCanMove(true);
    }

    public void SetCanMove(bool canMove) {
        foreach (var player in players)
        {
            player.Value.GetComponent<PlayerMovement>().canMove = canMove;
        }
    }

    public Dictionary<string,GameObject> getSocketEffects() {
        return socketEffects;
    }

    public Sprite[] getPlayerSprites() {
        return playerSprites;
    }

}