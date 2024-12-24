using System.Collections.Generic;
using System.Data;
using UnityEngine;

[System.Serializable]
public class NetworkMessage
{
    public List<RoomInfo> rooms = new List<RoomInfo>();
    public string action;
    public string playerId;
    public string playerName;
    public Vector3 position;
    public Quaternion rotation;
    public string spriteName;
    public string socketEffectName;
    public string trailEffectName;
    public string roomId;
    public string error;
    public int currentCheckpoint;
    public float currentTime;
    public Dictionary<string, float> finishTimes;
    public bool isReady;
    public bool isFinished;
    public List<PlayerInfo> players = new List<PlayerInfo>();
    public bool isHost;

    public string chatMessage;
    public string senderName;
}
