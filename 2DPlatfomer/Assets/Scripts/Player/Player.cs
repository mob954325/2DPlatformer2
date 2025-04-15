using System;
using UnityEngine;
using UnityEngine.Playables;

enum PlayerState
{
    Idle = 0,
    Hit,
    Dead,
}

public class Player : MonoBehaviour
{
    private Rigidbody2D rigid2d;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    PlayState state;

    // states
    private float baseSpeed = 5.0f;
    private float currentSpeed = 5.0f;
    private float walkSpeed = 2.0f;
    private float jumpForce = 30.0f;
    private float dashForce = 20.0f;
    private float dashDuration = 0.2f; // dash Cooldown

    // ground check
    private LayerMask groundLayer;
    private Transform groundCheck;

    // flag
    private bool isGrounded;
    private float dashTime;

    // Animator for animations (if you have an Animator for animations)
    private Animator animator;
    private int HashToIsWalk = Animator.StringToHash("IsWalk");
    private int HashToSpeed = Animator.StringToHash("Speed");
    private int HashToIsGrounded = Animator.StringToHash("IsGrounded");
    private int HashToOnJump = Animator.StringToHash("OnJump");

    void Start()
    {
        rigid2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        groundLayer = LayerMask.GetMask("Ground");
        groundCheck = transform.GetChild(0);
    }

    void Update()
    {
        // Check for ground status
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Handle movement and actions
        HandleMovement();
        HandleJump();
        HandleDash();

        // anim
        AnimationPramaterUpdate();
    }

    // Movement, Action Handle -----------------------------------------------------------------------

    private void HandleMovement()
    {
        // Get movement input
        float inputX = Input.GetAxis("Horizontal");
        moveInput = new Vector2(inputX, 0);

        // Move the character
        if (isGrounded && dashTime <= 0)
        {
            rigid2d.velocity = new Vector2(moveInput.x * currentSpeed, rigid2d.velocity.y);

            if(moveInput != Vector2.zero)
            {
                if (inputX > 0) spriteRenderer.flipX = false;
                else spriteRenderer.flipX = true;
            }

            // 앉기
            if (Input.GetKey(KeyCode.LeftControl))
            {
                currentSpeed = walkSpeed;
                animator.SetBool(HashToIsWalk, true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                animator.SetBool(HashToIsWalk, false);
                currentSpeed = baseSpeed;
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rigid2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetTrigger(HashToOnJump);
        }
    }

    private void HandleDash()
    {
        // Dash only when grounded or in mid-air
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashTime <= 0)
        {
            // Dash in the direction the player is facing
            rigid2d.velocity = new Vector2(moveInput.x * dashForce, rigid2d.velocity.y);

            dashTime = dashDuration; // Start dash duration
        }

        // Countdown dash time
        if (dashTime > 0)
        {
            dashTime -= Time.deltaTime;
        }
    }

    // Animation ------------------------------------------------------------------------------------

    /// <summary>
    /// Update animator parameters if using animation
    /// </summary>
    private void AnimationPramaterUpdate()
    {
        if (animator != null)
        {
            animator.SetFloat(HashToSpeed, Mathf.Abs(moveInput.x)); // abs -> need positive value
            animator.SetBool(HashToIsGrounded, isGrounded);
        }
    }

    // Debug ------------------------------------------------------------------------------------

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}
