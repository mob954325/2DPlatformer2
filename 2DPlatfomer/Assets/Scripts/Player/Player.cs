using System;
using System.Collections;
using UnityEngine;

enum PlayerState
{
    Idle = 0,
    Hit,
    Dead,
}

public class Player : MonoBehaviour, IDamageable, IAttacker
{
    private Rigidbody2D rigid2d;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform attackTransform;
    private Transform energyAttackTransform;
    private Transform attackPivot;
    private AttackArea[] attackAreas = new AttackArea[2];
    [SerializeField] private Vector2 moveInput;

    PlayerState state;

    // states
    [Header("Values")]
    [SerializeField] private float baseSpeed = 5.0f;
    [SerializeField] private float currentSpeed = 5.0f;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float jumpForce = 15.0f;
    [SerializeField] private float dashForce = 23.0f;
    [SerializeField] private float dashDuration = 0.2f; // dash Cooldown
    [Space(10f)]
    // states
    private float attackDamage = 2f;
    public float AttackDamage { get => attackDamage; }

    private float maxHp = 0;
    public float MaxHp 
    { 
        get => maxHp; 
        set
        {
            maxHp = value;
            Hp = maxHp;
        }
    }

    private float hp = 0;
    public float Hp 
    { 
        get => hp; 
        set
        {
            hp = Mathf.Clamp(value, 0.0f, MaxHp);
            Debug.Log($"{this.gameObject.name} : {hp}");

            if (hp <= 0)
            {
                // 사망
                OnDead();
            }
        }
    }

    public Action OnHpChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action OnHitPerformed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action OnDeadPerformed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float AttackCooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float MaxAttackCooldown => throw new NotImplementedException();
    public bool CanAttack { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<IDamageable> OnAttackPerformed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsDead => throw new NotImplementedException();


    // ground check
    private LayerMask groundLayer;
    private Transform groundCheck;

    // flag
    [Header("Flag")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isAttack;
    [SerializeField] private bool canMove;
    [SerializeField] private float dashTime;
    [Space(10f)]

    // ray
    float rayLength = 1.5f;

    // timer
    [Header("Timer")]
    [SerializeField] private float maxAttackComboDelayTime = 0.2f; // 다음 콤보까지 기다리는 시간
    [SerializeField] private float attackComboTimer = 0.0f;
    [SerializeField] private float sitTimer = 0.0f;
    [SerializeField] private float sitMaxTimer = 2.0f;
    [Space(10f)]

    // Animator for animations (if you have an Animator for animations)
    private Animator animator;
    private int HashToIsWalk = Animator.StringToHash("IsWalk");
    private int HashToSpeed = Animator.StringToHash("Speed");
    private int HashToIsGrounded = Animator.StringToHash("IsGrounded");
    private int HashToOnJump = Animator.StringToHash("OnJump");
    private int HashToOnAttack = Animator.StringToHash("OnAttack");
    private int HashToAttack1 = Animator.StringToHash("Attack1");
    private int HashToAttack2 = Animator.StringToHash("Attack2");
    private int HashToOnHit = Animator.StringToHash("OnHit");
    private int HashToOnDead = Animator.StringToHash("OnDead");
    private int HashToIsSit = Animator.StringToHash("IsSit");
    private int HashToOnSit = Animator.StringToHash("OnSit");

    public float checkRadius = 1;

    void Start()
    {
        MaxHp = 20;
        Hp = MaxHp;

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

        attackAreas = GetComponentsInChildren<AttackArea>();
        foreach(AttackArea comp in attackAreas)
        {
            comp.OnActiveAttackArea += (target, _) => { OnAttack(target); };
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Handle movement and actions
        HandleMovement();
        HandleBottomMove();
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

        // Get Horizontal input
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
    
    private void HandleBottomMove()
    {
        float inputY = Input.GetAxis("Vertical");
        moveInput.y = inputY;

        // 임시 키 처리
        if(Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger(HashToOnSit);
        }

        // S 누르면 실행
        if (moveInput.y < 0.0f)
        {
            sitTimer += Time.deltaTime;
            Debug.Log("2");

            //sittimer보다 길게 누르면 플랫폼 통과
            if (sitTimer > sitMaxTimer)
            {
                Debug.Log("1");
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);

                if (hit.collider != null)
                {
                    Debug.Log($"{hit.collider.gameObject.name}");
                    GameObject platform = hit.collider.gameObject;

                    // 하단 플랫폼인지 확인
                    if (platform.CompareTag("BottomPlatform"))
                    {
                        Debug.Log("맨 밑바닥이라 내려갈 수 없음");
                        return;
                    }

                    // 내려가기 처리
                    StartCoroutine(DisablePlatformTemporarily(platform));
                }
            }
        }
        else
        {
            sitTimer = 0.0f;
        }

        animator.SetBool(HashToIsSit, Input.GetKey(KeyCode.S));
    }

    private IEnumerator DisablePlatformTemporarily(GameObject platform)
    {
        //PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
        Collider2D col = platform.GetComponent<Collider2D>();

        // ray로 찾은 플랫폼 오브젝트 비활성화
        if (col != null)
        {
            col.enabled = false;
            yield return new WaitForSeconds(0.5f);
            col.enabled = true;
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
        if (Input.GetMouseButtonDown(0) && !isAttack && isGrounded) // 기본 공격
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

        isAttack = false;
        canMove = true;
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
            float moveValue = moveInput.x != 0 ? 1f : 0f;
            if(canMove) animator.SetFloat(HashToSpeed, moveValue); 
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

    // IDamageable -------------------------------------------------------------------------------
    public void TakeDamage(float damageValue)
    {
        Hp -= damageValue;
        animator.SetTrigger(HashToOnHit);
    }

    public void OnDead()
    {
        // 사망 로직 작성
        animator.SetTrigger(HashToOnDead);
        Debug.Log("플레이어 사망");

        StartCoroutine(OnDeadProcess());
    }

    private IEnumerator OnDeadProcess()
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.SetActive(false);
    }

    // Debug ------------------------------------------------------------------------------------

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
