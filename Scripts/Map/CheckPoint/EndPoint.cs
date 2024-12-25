using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    int checkCount = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(CheckpointManager.Instance.AllCheckpointsCleared());
        
        if (checkCount == 1 && other.CompareTag("ColliderPlayer") &&  CheckpointManager.Instance.AllCheckpointsCleared() && other.transform.parent.parent.GetComponent<PlayerController>().IsLocalPlayer)
        {
            Debug.Log("VaChamVoiPlayerOiemCuoi");
            UIManager.Instance.ShowWinBoardPanel();
            NetworkManager.Instance.SendMessage("player_finished", null);
            other.transform.parent.parent.GetComponent<PlayerMovement>().canMove = false;
           
            GameManager.Instance.isRaceStarted = false;

            checkCount = 0;
        }
    }
}
