using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static Movement instance;

    private void Awake()
    {
        instance = this;
    }

    public float moveSpeed;
    private Vector2 moveInput;

    // Dash
    private float activeMoveSpeed;
    public float dashSpeed = 8f, dashLength = 0.5f, dashCooldown = 1f;
    private float dashCounter;
    private float dashCooldownCounter;

    private Rigidbody2D theRB;
    private PlayerInvincibility invincibility;

    void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
        theRB.freezeRotation = true; // Lock rotation so the player doesn't spin

        invincibility = GetComponent<PlayerInvincibility>();
    }


    void Update()
    {
        // Movement input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        activeMoveSpeed = moveSpeed;

        // Handle dash input
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownCounter <= 0 && dashCounter <= 0)
        {
            // Start dash
            dashCounter = dashLength;
            dashCooldownCounter = dashCooldown;

            // Set invincible via shared manager
            if (invincibility != null)
            {
                invincibility.SetInvincibleForDuration(dashLength);
            }
        }

        // Dash countdown
        if (dashCounter > 0)
        {
            activeMoveSpeed = dashSpeed;
            dashCounter -= Time.deltaTime;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        // Dash cooldown countdown
        if (dashCooldownCounter > 0)
        {
            dashCooldownCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        theRB.velocity = moveInput * activeMoveSpeed;
    }
}
