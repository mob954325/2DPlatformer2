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
    Sit,
    Dead,
}

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IDamageable, IAttacker
{
    PlayerInput input;
    AttackArea attackArea;
    Vector3 attackAreaVec = Vector3.zero;

    Collider2D playerCollider;
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
            StateEnd(state);
            state = value;
            StateStart(state);
        }
    }

    // States
    [Header("Values")]
    [SerializeField] private float baseSpeed = 5.0f;
    [SerializeField] private float currentSpeed = 5.0f;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float jumpPower = 10.0f;
    [SerializeField] private float dashPower = 10.0f;
    [SerializeField] private float dashDuration = 0.8f; // dash Cooldown
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
    [SerializeField] private bool isSitting = false;
    [SerializeField] private bool isHit = false;
    [Space(10f)]

    // Timer
    private float dashTimer = 0f;

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
        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rigid2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashTrail = GetComponentInChildren<TrailRenderer>();
        attackArea = GetComponentInChildren<AttackArea>();
        attackAreaVec = attackArea.transform.localPosition;

        groundCheck = transform.GetChild(0);
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
        KeyUpdate();
        AnimationUpdate();
        TimerUpdate();
    }

    private void TimerUpdate()
    {
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;    
    }

    private void KeyUpdate()
    {
        if (IsDead || isHit) return;

        // dash
        if (input.IsDash)
        {
            if(!isDashing && dashTimer <= 0f) State = PlayerState.Dash;
        }

        // attack
        if (input.IsAttack)
        {
            if(!isAttacking) State = PlayerState.Attack;
        }

        // jump
        if(input.IsJump)
        {
            if(!isJumping) State = PlayerState.Jump;
        }

        // sit
        if(input.IsCrouch)
        {
            if(!isSitting) State = PlayerState.Sit;
        }

        // move
        if (!isAttacking && !isDashing && !isSitting && !isJumping)
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
            case PlayerState.Sit:
                SitStateStart();
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
            case PlayerState.Sit:
                SitState();
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
            case PlayerState.Sit:
                SitStateEnd();
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
        isDashing = true;

        if(dashTimer <= 0f)
        {
            if (input.InputVec.x != 0f) rigid2d.AddForce(input.InputVec * dashPower, ForceMode2D.Impulse);
            else rigid2d.AddForce(lastInputVec * dashPower, ForceMode2D.Impulse);

            dashTrail.enabled = true;
        }

        dashTimer = dashDuration;
    }

    private void JumpStateStart()
    {
        isJumping = true;
        anim.Play("Jump", 0);
        rigid2d.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
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

    private void SitStateStart()
    {
        isSitting = true;
        anim.Play("Sit", 0);
        Debug.Log("1");
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
        rigid2d.velocity = new Vector2(input.InputVec.x * baseSpeed, rigid2d.velocity.y);
        lastInputVec = input.InputVec;
    }

    private void DashState()
    {
        if (CheckAnimationEnd())
        {
            State = PlayerState.Idle;
        }
    }

    private void JumpState()
    {
        // Check for ground status
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkGroundRadius, groundLayer);

        if(isGrounded)
        {
            State = PlayerState.Idle;
        }
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

    private void SitState()
    {
        Debug.Log("2");

        if (input.IsJump)
        {
            GoToPlatform();
        }

        if(!input.IsCrouch)
        {
            State = PlayerState.Idle;
            Debug.Log("3");
        }
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

    private void SitStateEnd()
    {
        isSitting = false;
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
