using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float raceTime { get; private set; }
    public bool isReady { get; private set; }
    public bool isRacing { get; private set; }
    public bool isFinished { get; private set; }
    
    private NetworkManager networkManager;
    private PlayerController playerController;

    private void Awake()
    {
        networkManager = NetworkManager.Instance;
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (isRacing && !isFinished)
        {
            raceTime += Time.deltaTime;
        }
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
        networkManager.SendMessage("player_ready", new { 
            playerId = playerController.PlayerId,
            isReady = ready 
        });
    }

    public void StartRace()
    {
        isRacing = true;
        raceTime = 0f;
    }

    public void FinishRace()
    {
        isFinished = true;
        isRacing = false;
        networkManager.SendMessage("race_finished", new {
            playerId = playerController.PlayerId,
            raceTime = raceTime
        });
    }
}
