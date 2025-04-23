using System;
using System.Collections;
using UnityEngine;

enum PlayerState
{
    Idle = 0,
    Move,
    Dash,
    Jump,
    Hit,
    Attack,
    Crouch,
    Dead,
}

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IDamageable, IAttacker
{
    PlayerInput input;
    AttackArea attackArea;
    Vector3 attackAreaVec = Vector3.zero;

    Collider2D playerCollider;
    Collider2D crouchCollider;
    Rigidbody2D rigid2d;
    SpriteRenderer spriteRenderer;
    TrailRenderer dashTrail;
    Animator anim;

    // ground check
    private LayerMask groundLayer;
    private Transform groundCheck;

    [SerializeField] PlayerState state;
    PlayerState State
    {
        get => state;
        set
        {
            if (state == value) return;
            StateEnd(state);
            state = value;

            rigid2d.velocity = Vector2.zero;
            StateStart(state);
        }
    }

    // States
    [Header("Values")]
    [SerializeField] private float baseSpeed = 5.0f;
    [SerializeField] private float currentSpeed = 5.0f;
    [SerializeField] private float currentSpeedWhileJump = 8.0f; // ?
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float jumpPower = 10.0f;
    [SerializeField] private float dashPower = 10.0f;
    [SerializeField] private float dashDuration = 0.6f; // 대쉬 지속시간
    [SerializeField] private float maxDashCoolDown = 1f; // 대쉬 후 다음 대쉬사용하기 까지 기다려야하는 시간
    [Space(10f)]

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

    private float currentHp = 0;
    public float Hp 
    { 
        get => currentHp; 
        set
        {
            currentHp = Mathf.Clamp(value, 0.0f, MaxHp);
            OnHpChange?.Invoke(currentHp);

            if (currentHp <= 0)
            {
                // 사망
                State = PlayerState.Dead;
            }
        }
    }

    // flag
    [Header("Flag")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool isCrouching = false;
    [SerializeField] private bool isHit = false;
    [Space(10f)]

    // Timer
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    // ray
    float rayLength = 1.5f;

    public float checkGroundRadius = 0.2f;

    private Vector2 lastInputVec = Vector2.zero;
    private float stateTimer = 0f;
    private int attackCount = 1;
    private int maxAttackCount = 2;

    private float attackCooldown = 0.1f;
    public float AttackCooldown
    {
        get => attackCooldown;
        set => attackCooldown = Mathf.Clamp(value, 0, MaxAttackCooldown);
    }

    private float maxAttackCooldown = 0.1f;
    public float MaxAttackCooldown => maxAttackCooldown;

    private bool canAttack = true;
    public bool CanAttack { get => canAttack; set => canAttack = value; }
    public Action<IDamageable> OnAttackPerformed { get; set; }


    public Action<float> OnHpChange { get; set; }
    public Action OnHitPerformed { get; set; }
    public Action OnDeadPerformed { get; set; }

    public bool IsDead => Hp <= 0;

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        attackArea = GetComponentInChildren<AttackArea>();
        attackAreaVec = attackArea.transform.localPosition;

        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rigid2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashTrail = GetComponentInChildren<TrailRenderer>();
        playerCollider = GetComponent<Collider2D>();
        crouchCollider = transform.GetChild(3).GetComponent<Collider2D>(); //

        groundCheck = transform.GetChild(0); //
        groundLayer = LayerMask.GetMask("Ground");

        dashTrail.enabled = false;

        attackArea.OnActiveAttackArea += (target, _) => { OnAttack(target); };
        attackArea.gameObject.SetActive(false);
        MaxHp = 20;
    }

    private void FixedUpdate()
    {
        StateUpdate(State);
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkGroundRadius, groundLayer | LayerMask.GetMask("Default"));

        KeyUpdate();
        AnimationUpdate();
        TimerUpdate();
    }

    private void TimerUpdate()
    {
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
    }

    private void KeyUpdate()
    {
        if (IsDead || isHit) return;

        // dash
        if (input.IsDash && !isDashing && dashCooldownTimer <= 0f)
        {
            State = PlayerState.Dash;
        }

        // attack
        if (input.IsAttack)
        {
            if(!isAttacking) State = PlayerState.Attack;
        }

        // jump
        if (input.IsJump && isGrounded && !isCrouching && !isJumping) // TODO : 점프가 어려번 눌림
        {
            State = PlayerState.Jump;
            Debug.Log("Jumped\n");
        }

        // sit
        if (input.IsCrouch)
        {
            if(!isCrouching) State = PlayerState.Crouch;
        }

        // move
        if (!isAttacking && !isDashing && !isCrouching && !isJumping)
        {
            if (input.InputVec.x != 0)
            {
                State = PlayerState.Move;
            }
            else
            {
                State = PlayerState.Idle;
            }
        }
    }

    private void AnimationUpdate()
    {
        if( input.InputVec.x != 0 )spriteRenderer.flipX = input.InputVec.x < 0f;
    }

    private void StateStart(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                IdleStateStart();
                break;
            case PlayerState.Move:
                MoveStateStart();
                break;
            case PlayerState.Dash:
                DashStateStart();
                break;
            case PlayerState.Jump:
                JumpStateStart();
                break;
            case PlayerState.Hit:
                HitStateStart();
                break;
            case PlayerState.Attack:
                AttackStateStart();
                break;
            case PlayerState.Crouch:
                CrouchStateStart();
                break;
            case PlayerState.Dead:
                DeadStateStart();
                break;
            default:
                break;
        }
    }

    private void StateUpdate(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                IdleState();
                break;
            case PlayerState.Move:
                MoveState();
                break;
            case PlayerState.Dash:
                DashState();
                break;
            case PlayerState.Jump:
                JumpState();
                break;
            case PlayerState.Hit:
                HitState();
                break;
            case PlayerState.Attack:
                AttackState();
                break;
            case PlayerState.Crouch:
                CrouchState();
                break;
            case PlayerState.Dead:
                DeadState();
                break;
            default:
                break;
        }
    }

    private void StateEnd(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                IdleStateEnd();
                break;
            case PlayerState.Move:
                MoveStateEnd();
                break;
            case PlayerState.Dash:
                DashStateEnd();
                break;
            case PlayerState.Jump:
                JumpStateEnd();
                break;
            case PlayerState.Hit:
                HitStateEnd();
                break;
            case PlayerState.Attack:
                AttackStateEnd();
                break;
            case PlayerState.Crouch:
                CrouchStateEnd();
                break;
            case PlayerState.Dead:
                DeadStateEnd();
                break;
            default:
                break;
        }
    }

    // State Update -------------------------------------------------------------------------------------------------------

    #region State Start
    private void IdleStateStart()
    {
        anim.Play("Idle", 0);
    }

    private void MoveStateStart()
    {
        anim.Play("Run", 0);
    }

    private void DashStateStart()
    {
        if (isDashing || dashTimer > 0f) return;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = maxDashCoolDown;
        Vector2 dashDirection = input.InputVec.x != 0f ? input.InputVec : lastInputVec;
        rigid2d.velocity = Vector2.zero; // 이전 힘 제거
        rigid2d.AddForce(dashDirection * dashPower, ForceMode2D.Impulse);
        rigid2d.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        dashTrail.enabled = true;

    }

    private void JumpStateStart()
    {
        if (isJumping) return;

        anim.Play("Jump", 0);
        rigid2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        isJumping = true;
    }

    private void HitStateStart()
    {
        isHit = true;
        anim.Play("Hit", 0);
    }

    private void AttackStateStart()
    {
        isAttacking = true;
        anim.Play("Attack" + attackCount, 0);
        attackCount++;

        if (attackCount > maxAttackCount) attackCount = 1;

        AttackCooldown = MaxAttackCooldown;

        SetAttackAreaPosition();
        attackArea.gameObject.SetActive(true);
    }

    private void CrouchStateStart()
    {
        isCrouching = true;
        anim.Play("Sit", 0);
        ChangeToCrouchCollider(true);
    }

    private void DeadStateStart()
    {
        OnDead();
    }
    #endregion

    #region State Update
    private void IdleState()
    {
        rigid2d.velocity = new Vector2(0f, rigid2d.velocity.y);
    }

    private void MoveState()
    {
        if(input.InputVec.x != 0)
        {
            rigid2d.velocity = new Vector2(input.InputVec.x * baseSpeed, rigid2d.velocity.y);
            lastInputVec = input.InputVec;
        }
    }

    private void DashState()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer < 0f)
        {
            dashTimer = 0.0f;
            State = PlayerState.Idle;
        }
    }

    private void JumpState()
    {
        // Check for ground status

        if (isGrounded && rigid2d.velocity.y <= 0.01f)
        {
            State = PlayerState.Idle;
        }

        MoveWhileOtherState(currentSpeedWhileJump);
    }

    private void HitState()
    {
        if (CheckAnimationEnd())
        {
            State = PlayerState.Idle;
        }
    }

    private void AttackState()
    {
        AttackCooldown -= Time.deltaTime;

        if (CheckAnimationEnd())
        {
            State = PlayerState.Idle;
        }
    }

    private void CrouchState()
    {
        if (input.IsJump)
        {
            GoToPlatform();
        }

        if(!input.IsCrouch)
        {
            State = PlayerState.Idle;
        }

        MoveWhileOtherState(walkSpeed);
    }

    private void DeadState()
    {
        if (CheckAnimationEnd())
        {
            this.gameObject.SetActive(false);
        }
    }
    #endregion

    #region State End

    private void IdleStateEnd()
    {

    }
    private void MoveStateEnd()
    {

    }
    private void DashStateEnd()
    {
        isDashing = false;
        dashTrail.enabled = false;
        rigid2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void JumpStateEnd()
    {
        isJumping = false;
    }

    private void HitStateEnd()
    {
        isHit = false;
    }

    private void AttackStateEnd()
    {
        isAttacking = false;
        attackArea.gameObject.SetActive(false);
    }

    private void CrouchStateEnd()
    {
        isCrouching = false;
        ChangeToCrouchCollider(false);
    }

    private void DeadStateEnd()
    {

    }
    #endregion

    #region Functions
    private bool CheckAnimationEnd()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer > anim.GetCurrentAnimatorClipInfo(0)[0].clip.length)
        {
            stateTimer = 0f;
            return true;
        }

        return false;
    }

    private void SetAttackAreaPosition()
    {
        if (input.InputVec.x > 0)
        {
            attackArea.gameObject.transform.localPosition = new Vector3(attackAreaVec.x, attackAreaVec.y, attackAreaVec.z);

        }
        else if (input.InputVec.x < 0)
        {
            attackArea.gameObject.transform.localPosition = new Vector3(-attackAreaVec.x, attackAreaVec.y, attackAreaVec.z);
        }
    }

    private void ChangeToCrouchCollider(bool value)
    {
        if(value)
        {
            playerCollider.enabled = false;
            crouchCollider.enabled = true;
        }
        else
        {
            playerCollider.enabled = true;
            crouchCollider.enabled = false;
        }
    }

    private void GoToPlatform()
    {
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

    // NOTE 책임 복잡해지면 리펙토링 고려하기
    private void MoveWhileOtherState(float value)  
    {
        if(input.InputVec.x != 0)
        {
            rigid2d.velocity = new Vector2(input.InputVec.x * value, rigid2d.velocity.y);
        }
    }

    #endregion

    // IAttackable -------------------------------------------------------------------------------

    public void OnAttack(IDamageable target)
    {
        target.TakeDamage(AttackDamage);
    }

    // IDamageable -------------------------------------------------------------------------------
    public void TakeDamage(float damageValue)
    {
        if (IsDead) return;

        State = PlayerState.Hit;
        Hp -= damageValue;
    }

    public void OnDead()
    {
        // 사망 로직 작성
        Debug.Log("플레이어 사망");
        anim.Play("Dead", 0);
    }

    // Debug ------------------------------------------------------------------------------------

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkGroundRadius);
        }
    }
}
