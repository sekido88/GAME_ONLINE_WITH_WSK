using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;
    public List<GameObject> checkpointsOrder = new List<GameObject>();
    private List<int> passedCheckpoints = new List<int>();

    // public GameObject localPlayer;
    private void Awake()
    {
      Instance = this;
      InitCheckpointsOrder();
    }

    private void InitCheckpointsOrder()
    {
    
        for (int i = 0; i < transform.childCount; i++)
        {
            checkpointsOrder.Add(transform.GetChild(i).gameObject);
            checkpointsOrder[i].GetComponent<CheckPointOne>().checkpointIndex = i;
        }
        checkpointsOrder[transform.childCount - 1].AddComponent<EndPoint>();
    }
    public void PlayerHitCheckpoint(int checkpointIndex)
    {
        if (passedCheckpoints.Contains(checkpointIndex)) return;
        if (checkpointIndex == 0 || (!passedCheckpoints.Contains(checkpointIndex) && passedCheckpoints.Contains(checkpointIndex - 1)) )
        {
            passedCheckpoints.Add(checkpointIndex);
            Debug.Log($"Checkpoint {checkpointIndex} đã đạt!");
        }
    }
    public bool AllCheckpointsCleared()
    {
        Debug.Log(passedCheckpoints.Count);
        Debug.Log(checkpointsOrder.Count);
        return passedCheckpoints.Count == checkpointsOrder.Count;
    }


}


