using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointOne : MonoBehaviour
{
    public int checkpointIndex = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ColliderPlayer") && other.transform.parent.parent.GetComponent<PlayerController>().IsLocalPlayer)
        {
            Debug.Log("Rat dep trai");
            CheckpointManager.Instance.PlayerHitCheckpoint(checkpointIndex);
        }
    }
}
