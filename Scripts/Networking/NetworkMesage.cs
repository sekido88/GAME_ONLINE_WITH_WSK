using System.Collections.Generic;
using System.Data;
using UnityEngine;

[System.Serializable]
public class NetworkMessage
{
    public string action;
    public string playerId;
    public string playerName;
    public Vector3 position;
    public Quaternion rotation;
    public string roomId;
    public string error;
    public float currentLap;
    public int currentCheckpoint;
    public float raceTime;
    public bool isReady;
    public bool isFinished;
    public List<PlayerInfo> players = new List<PlayerInfo>();
    public bool isHost;
}
