using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playersGameObject;
    [SerializeField] private List<Sprite> playerSprites;
    public Dictionary<string, Sprite> playerSpritesDictionary = new Dictionary<string, Sprite>();

    [Header("Player Effect")]
    [SerializeField] private List<GameObject> socketEffectPrefabs;
    public Dictionary<string, GameObject> socketEffects = new Dictionary<string, GameObject>();

    [SerializeField] private List<GameObject> trailEffectPrefabs;
    public Dictionary<string, GameObject> trailEffects = new Dictionary<string, GameObject>();

    [Header("Point Spawn Player")]
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
        DontDestroyOnLoad(gameObject);
        cameraFollow = GameObject.Find("Camera Holder").GetComponent<CameraFollow>();

        InitPlayerSprites();
        InitSocketEffects();
        InitTrailEffects();
    }

    private void InitPlayerSprites()
    {
        playerSpritesDictionary.Clear();
        for (int i = 0; i < playerSprites.Count; i++)
        {
            if (playerSprites[i] != null)
            {
                playerSpritesDictionary[playerSprites[i].name] = playerSprites[i];

            }
        }
    }




    private void InitSocketEffects()
    {
        socketEffects.Clear();
        for (int i = 0; i < socketEffectPrefabs.Count; i++)
        {
            if (socketEffectPrefabs[i] != null)
            {
                socketEffects[socketEffectPrefabs[i].name] = socketEffectPrefabs[i];

            }
        }
    }

    private void InitTrailEffects()
    {
        trailEffects.Clear();
        for (int i = 0; i < trailEffectPrefabs.Count; i++)
        {
            if (trailEffectPrefabs[i] != null)
            {
                trailEffects[trailEffectPrefabs[i].name] = trailEffectPrefabs[i];
            }
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

        if (playerStats.TryGetValue(NetworkManager.Instance.PlayerId, out PlayerStats stats))
        {
            return stats;
        }
        return null;
    }
    public void SpawnPlayer(string playerId, Quaternion rotation)
    {

        if (players.ContainsKey(playerId)) {
            ResetPlayer(playerId);
        };

        GameObject playerObj = Instantiate(playerPrefab, spawnPoints[players.Count].position, rotation, playersGameObject.transform);
        playerObj.transform.localScale = Vector3.one;
        playerObj.transform.rotation = Quaternion.Euler(0, 0, 90);

        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.Initialize(playerId, $"Player_{playerId}", playerId == NetworkManager.Instance.PlayerId);
        players.Add(playerId, playerObj);
        playerStats.Add(playerId, playerObj.GetComponent<PlayerStats>());


    }

    public void SpawnPlayer(string playerId, Sprite sprite, GameObject socketEffect, GameObject trailEffect, Quaternion rotation)
    {
        if (players.ContainsKey(playerId)) {
            ResetPlayer(playerId);
        };

        Vector3 spawnPosition = spawnPoints[players.Count].position;
        GameObject playerObj = Instantiate(playerPrefab, spawnPosition, rotation, playersGameObject.transform);

        // Set sprite
        SpriteRenderer spriteRenderer = playerObj.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // Set effects properly
        PlayerEffects playerEffects = playerObj.GetComponent<PlayerEffects>();
        playerEffects.SetEffect(socketEffect, trailEffect);

        // Initialize player
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        playerController.Initialize(playerId, $"Player_{playerId}", playerId == NetworkManager.Instance.PlayerId,socketEffect,trailEffect);

        players.Add(playerId, playerObj);
        playerStats.Add(playerId, playerObj.GetComponent<PlayerStats>());
    }

   public void StartRace()
    {
        isRaceStarted = true;
        raceTimer = 0f;
        UIManager.Instance.ShowRaceUI();
        PlayerEffectsManager.Instance.CheckAndApplyEffectsToAllPlayers();
        SetCanMove(true);
    }

    public void SetPlayerTarget()
    {
        cameraFollow.setTarget(players[NetworkManager.Instance.PlayerId].transform);
    }

    public void SetCanMove(bool canMove)
    {
        foreach (var player in players)
        {
            player.Value.GetComponent<PlayerMovement>().canMove = canMove;
        }
    }

    public GameObject GetPlayerPrefab()
    {
        return playerPrefab;
    }

    public void SetPlayerPrefab(GameObject gameObject)
    {
        playerPrefab = gameObject;
    }

    public void ResetPlayer(string playerId) {
        if (players.ContainsKey(playerId)) {
            Destroy(players[playerId]);
            players.Remove(playerId);
            playerStats.Remove(playerId);
        }
    }

    public void ResetPlayers()
    {
        foreach (var player in players.Values)
        {
            Destroy(player);
        }
        players.Clear();
        playerStats.Clear();
    }


}