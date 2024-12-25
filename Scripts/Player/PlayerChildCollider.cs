using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerChildCollider : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            playerMovement.HandleWallCollision(collision);
            AudioManager.Instance.PlaySFX("crash");
        }
    }
}
