using System;
using System.Collections;
using UnityEngine;

enum PlayerState
{
    Idle = 0,
    Hit,
    Dead,
}

public class Player : MonoBehaviour, IAttackable
{
    private Rigidbody2D rigid2d;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform attackTransform;
    private Transform energyAttackTransform;
    private Transform attackPivot;
    private PlayerAttackArea[] attackAreas = new PlayerAttackArea[2]; 
    private Vector2 moveInput;

    PlayerState state;

    // states
    private float baseSpeed = 5.0f;
    private float currentSpeed = 5.0f;
    private float walkSpeed = 2.0f;
    private float jumpForce = 15.0f;
    private float dashForce = 20.0f;
    private float dashDuration = 0.2f; // dash Cooldown

    // states
    private float attackDamage = 2f;
    public float AttackDamage { get => attackDamage; }


    // ground check
    private LayerMask groundLayer;
    private Transform groundCheck;

    // flag
    private bool isGrounded;
    private bool isAttack;
    private bool canMove;
    private float dashTime;

    // timer
    private float maxAttackComboDelayTime = 1f; // 다음 콤보까지 기다리는 시간
    private float attackComboTimer = 0.0f;      

    // Animator for animations (if you have an Animator for animations)
    private Animator animator;
    private int HashToIsWalk = Animator.StringToHash("IsWalk");
    private int HashToSpeed = Animator.StringToHash("Speed");
    private int HashToIsGrounded = Animator.StringToHash("IsGrounded");
    private int HashToOnJump = Animator.StringToHash("OnJump");
    private int HashToOnAttack = Animator.StringToHash("OnAttack");
    private int HashToAttack1 = Animator.StringToHash("Attack1");
    private int HashToAttack2 = Animator.StringToHash("Attack2");


    void Start()
    {
        rigid2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();

        groundLayer = LayerMask.GetMask("Ground");

        Transform child = transform.GetChild(0);
        groundCheck = child;
        child = transform.GetChild(1);
        attackPivot = child;
        attackTransform = attackPivot.GetChild(0);
        child = attackPivot.GetChild(1);
        energyAttackTransform = child;

        attackAreas = GetComponentsInChildren<PlayerAttackArea>();
        foreach(PlayerAttackArea comp in attackAreas)
        {
            comp.OnActiveAttackArea += OnAttack;
        }

        trailRenderer.enabled = false;
        attackTransform.gameObject.SetActive(false);
        energyAttackTransform.gameObject.SetActive(false);
        animator.SetBool(HashToAttack1, true);
        animator.SetBool(HashToAttack2, false);
    }

    void Update()
    {
        // Check for ground status
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Handle movement and actions
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleAttack();
        AttackAreaUpate();

        // anim
        AnimationPramaterUpdate();
        AttackComboDelayUpdate();
    }

    private void AttackAreaUpate()
    {
        if(moveInput.x > 0)
        {
            attackPivot.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if(moveInput.x < 0)
        {
            attackPivot.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    // Movement, Action Handle -----------------------------------------------------------------------

    private void HandleMovement()
    {
        if (!canMove) return;

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
            animator.SetBool(HashToAttack1, true); // 임시, 나중에 함수로 하나로 수정하기
            animator.SetBool(HashToAttack2, false);

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

            StartCoroutine(DashEffect());
        }

        // Countdown dash time
        if (dashTime > 0)
        {
            dashTime -= Time.deltaTime;
        }
    }

    // 임시
    private IEnumerator DashEffect()
    {
        trailRenderer.enabled = true;
        yield return new WaitForSeconds(0.2f);
        trailRenderer.enabled = false;
    }

    private void HandleAttack()
    {
        animator.SetFloat(HashToSpeed, 0);
        if (Input.GetMouseButtonDown(0) && !isAttack) // 기본 공격
        {
            animator.SetTrigger(HashToOnAttack);
            StartCoroutine(ComboAttackProcess());
        }

        if(Input.GetMouseButtonDown(1)) // 에너지파
        {
            StartCoroutine(EnergyAttackProcess());
        }
    }

    IEnumerator ComboAttackProcess()
    {
        canMove = false;
        isAttack = true;
        attackTransform.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        attackTransform.gameObject.SetActive(false);

        isAttack = false;
        canMove = true;
        
        // 다음 콤보 플래그 설정
        if (animator.GetBool(HashToAttack1))
        {
            attackComboTimer = maxAttackComboDelayTime;
            animator.SetBool(HashToAttack1, false);
            animator.SetBool(HashToAttack2, true);
        }
        else if(animator.GetBool(HashToAttack2))
        {
            animator.SetBool(HashToAttack1, true);
            animator.SetBool(HashToAttack2, false);
        }
    }

    IEnumerator EnergyAttackProcess()
    {
        canMove = false;
        isAttack = true;
        energyAttackTransform.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        energyAttackTransform.gameObject.SetActive(false);

        isAttack = false;
        canMove = true;
    }

    // Animation ------------------------------------------------------------------------------------

    /// <summary>
    /// Update animator parameters if using animation
    /// </summary>
    private void AnimationPramaterUpdate()
    {
        if (animator != null)
        {
            if(canMove) animator.SetFloat(HashToSpeed, Mathf.Abs(moveInput.x)); // abs -> need positive value
            animator.SetBool(HashToIsGrounded, isGrounded);
        }
    }

    private void AttackComboDelayUpdate()
    {
        if (attackComboTimer < 0.0f)
        {
            animator.SetBool(HashToAttack1, true);
            animator.SetBool(HashToAttack2, false);
            attackComboTimer = 0.0f;
            return;
        }

        attackComboTimer -= Time.deltaTime; 
    }

    // IAttackalb -------------------------------------------------------------------------------

    public void OnAttack(IDamageable target)
    {
        target.TakeDamage(AttackDamage);
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
