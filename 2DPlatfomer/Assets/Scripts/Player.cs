using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigid2d;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 moveInput;

    [SerializeField] private float baseSpeed = 5.0f;
    [SerializeField] private float currentSpeed = 5.0f;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float jumpForce = 10.0f;
    [SerializeField] private float dashForce = 20.0f;
    [SerializeField] private float dashDuration = 0.2f; // dash Cooldown

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float dashTime;

    // Animator for animations (if you have an Animator for animations)
    private Animator animator;

    void Start()
    {
        rigid2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
                animator.SetBool("IsWalk", true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                animator.SetBool("IsWalk", false);
                currentSpeed = baseSpeed;
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rigid2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x)); // abs -> need positive value
            animator.SetBool("IsGrounded", isGrounded);
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
