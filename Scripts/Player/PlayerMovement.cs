using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float rotationSpeed = 260f;
    [SerializeField] private float driftFactor = 0.85f;

    [Header("Drift Settings")]
    [SerializeField] private float wallBoostForce = 30f;
    [SerializeField] private float wallDragMultiplier = 0.5f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Collision Settings")]
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float minBounceSpeed = 5f;
    [SerializeField] private float maxBounceSpeed = 30f;
    
    private Collider2D[] childColliders;
    private bool isColliding = false;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private PlayerEffects playerEffects;
    private PlayerController playerController;
    private float targetRotation;
    private float originalDrag;

    private InfluenceAreaTrigger influenceArea;
    public bool canMove = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerEffects = GetComponent<PlayerEffects>();
        playerController = GetComponent<PlayerController>();
        rb.gravityScale = 0;
        originalDrag = rb.drag;

        influenceArea = transform.GetChild(1).gameObject.GetComponent<InfluenceAreaTrigger>();
   
        Transform colliderParent = transform.Find("Collider"); 
        if(colliderParent != null)
        {
            childColliders = colliderParent.GetComponentsInChildren<Collider2D>();
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            HandleMovement(playerInput.CurrentInput);
        }
    }

    private void HandleMovement(PlayerInputDatas input)
    {
        if (!playerController.IsLocalPlayer)
            return;

        float rotationAmount = input.horizontal * rotationSpeed * Time.fixedDeltaTime;
        targetRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, 0, targetRotation);

        Vector2 force = transform.up * acceleration;
        rb.AddForce(force);

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + rightVelocity * driftFactor;

        if (input.horizontal != 0 && rb.velocity.magnitude < 0.1f)
        {
            rb.angularVelocity = 0f;
        }
    }

    public void HandleWallCollision(Collision2D collision)
    {
        if(!canMove) return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 collisionNormal = contact.normal;
        
        Vector2 currentVelocity = rb.velocity;
        Vector2 reflection = Vector2.Reflect(currentVelocity, collisionNormal);
        
        float currentSpeed = currentVelocity.magnitude;
        float bounceSpeed = Mathf.Clamp(currentSpeed * bounceForce, minBounceSpeed, maxBounceSpeed);
        
        rb.velocity = reflection.normalized * bounceSpeed;
    }

    public bool getIsDrifting() {
        return influenceArea.isDrifting;
    }
 

}