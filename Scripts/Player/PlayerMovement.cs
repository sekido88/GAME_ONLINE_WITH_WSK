using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float rotationSpeed = 260f;
    [SerializeField] private float driftFactor = 0.95f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private PlayerEffects playerEffects;
    private float targetRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerEffects = GetComponent<PlayerEffects>();
        rb.gravityScale = 0;
    }

    private void FixedUpdate()
    {
        HandleMovement(playerInput.CurrentInput);
    }

    private void HandleMovement(PlayerInputDatas input)
    {
        if (!GetComponent<PlayerController>().IsLocalPlayer)
        return;
        if (!GetComponent<PlayerController>().IsLocalPlayer)
        return;

        float rotationAmount = input.horizontal * rotationSpeed * Time.fixedDeltaTime;
        targetRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, 0, targetRotation);
        rb.AddForceAtPosition(transform.up * acceleration, playerEffects.GetFirePoint().position);
        
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + rightVelocity * driftFactor;

    }
}