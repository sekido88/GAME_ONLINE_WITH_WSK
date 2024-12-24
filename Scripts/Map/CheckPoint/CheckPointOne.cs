using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointOne : MonoBehaviour
{
    public int checkpointIndex = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.PlayerHitCheckpoint(checkpointIndex);
        }
    }
}
