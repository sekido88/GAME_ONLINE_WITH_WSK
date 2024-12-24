using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceAreaTrigger : MonoBehaviour
{
    [SerializeField] private float boostForce = 30f;
    private Rigidbody2D playerRb;

    private PlayerMovement playerMovement;
    public bool isDrifting = false;

    void Start()
    {
        playerRb = transform.parent.GetComponent<Rigidbody2D>();
        playerMovement = transform.parent.GetComponent<PlayerMovement>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") && playerMovement.canMove)
        {
         
                isDrifting = true;
                Vector2 wallNormal = (transform.position - collision.transform.position).normalized;
                Vector2 driftDirection = Vector2.Perpendicular(wallNormal);
                
                float currentSpeed = playerRb.velocity.magnitude;
                float maxDriftSpeed = 100f;
                
                if (currentSpeed < maxDriftSpeed)
                {
                    playerRb.velocity += driftDirection * boostForce * Time.fixedDeltaTime;
                }
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            isDrifting = false;
        }
    }


}
