﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

enum PlayerState
{
    Idle = 0,
    Move,
    Dash,
    Jump,
    Attack,
    Crouch,
    Roll,
    Dead,
}

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IDamageable, IAttacker
{
    PlayerInput input;
    PlayerInteractDetect interactDectect;
    AttackArea attackArea;
    Vector3 attackAreaVec = Vector3.zero;

    GameObject InteractionGuideObject;
    GameObject specialAttackAreaPivot;
    AttackArea specialAttackArea;

    Collider2D playerCollider;
    Collider2D crouchCollider;
    Rigidbody2D rigid2d;
    SpriteRenderer spriteRenderer;
    TrailRenderer dashTrail;
    Animator anim;

    // ground check
    private LayerMask groundLayer;
    private Transform groundCheck;

    PlayerState state;
    PlayerState State
    {
        get => state;
        set
        {
            if (state == value) return;
            StateEnd(state);
            state = value;

            StateStart(state);
        }
    }

    // States
    [Header("Values")]
    private float baseSpeed = 5.0f;
    //private float currentSpeed = 5.0f;
    private float currentSpeedWhileJump = 8.0f;
    //private float walkSpeed = 2.0f;
    private float jumpPower = 17.0f;
    private float dashPower = 12.0f;
    private float dashDuration = 0.6f; // 대쉬 지속시간
    private float maxDashCoolDown = 1f; // 대쉬 후 다음 대쉬사용하기 까지 기다려야하는 시간
    private float rollPower = 5f;

    public GameObject specialAttackFX;

    [Space(10f)]

    private float attackDamage = 1f;
    public float AttackDamage { get => attackDamage; }

    private float maxHp = 0;
    public float MaxHp 
    { 
        get => maxHp; 
        set
        {
            maxHp = value;
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
    private bool isGrounded = true;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isDashing = false;
    private bool isCrouching = false;
    private bool isHit = false;
    private bool isRolling = false;
    private bool isSpecialAttacking = false;
    private bool hasSpecialAttack = true;
    private bool isImmune = false; // true면 무적

    public bool HasSpecialAttack { get => hasSpecialAttack; } // UI확인용

    // Timer
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    // ray
    float rayLength = 1.5f;

    public float checkGroundRadius = 0.2f;

    private Vector2 lastInputVec = Vector2.zero;
    private float stateTimer = 0f;
    private int attackCount = 1;
    private int maxAttackCount = 3;
    public float specialAttackTimer = 0f;
    private float maxSpecialAttackTime = 2f; // 임시

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

    private void OnEnable()
    {
        input = GetComponent<PlayerInput>();
        attackArea = GetComponentInChildren<AttackArea>(true);
        attackAreaVec = attackArea.transform.localPosition;

        playerCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rigid2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashTrail = GetComponentInChildren<TrailRenderer>();

        crouchCollider = transform.GetChild(3).GetComponent<Collider2D>(); // 
        specialAttackAreaPivot = transform.GetChild(4).gameObject;
        specialAttackArea = specialAttackAreaPivot.GetComponentInChildren<AttackArea>(true); //
        interactDectect = GetComponentInChildren<PlayerInteractDetect>();
        InteractionGuideObject = transform.GetChild(6).gameObject;

        groundCheck = transform.GetChild(0); 
        groundLayer = LayerMask.GetMask("Ground");

        dashTrail.enabled = false;

        attackArea.OnActiveAttackArea += (target, _) => { OnAttack(target); };
        specialAttackArea.OnActiveAttackArea += (target, _) => {
            target.TakeDamage(AttackDamage * 2);
        };

        input.OnInteract += () => { if(interactDectect.Target != null) interactDectect.Target.Interact(); };

        attackArea.gameObject.SetActive(false);
        specialAttackArea.gameObject.SetActive(false);
        InteractionGuideObject.gameObject.SetActive(false);

        ChangeToCrouchCollider(false);
        hasSpecialAttack = true;
    }

    private void OnDisable()
    {
        OnHpChange = null;
        OnDeadPerformed = null;
        OnHitPerformed = null;
        attackArea.OnActiveAttackArea = null;
        specialAttackArea.OnActiveAttackArea = null;
        input.OnInteract = null;
    }

    private void FixedUpdate()
    {
        StateUpdate(State);
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkGroundRadius, groundLayer);

        KeyUpdate();
        AnimationUpdate();
        TimerUpdate();
        GuidUpdate();
    }

    private void GuidUpdate()
    {
        if (interactDectect.Target != null)
        {
            InteractionGuideObject.SetActive(true);
        }
        else
        {
            InteractionGuideObject.SetActive(false);
        }

    }

    private void TimerUpdate()
    {
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

        if (specialAttackTimer > 0f) specialAttackTimer -= Time.deltaTime;
        else if (specialAttackTimer <= 0.0f)
        {
            isSpecialAttacking = false;
            specialAttackArea.gameObject.SetActive(false);
        }
    }

    private void KeyUpdate()
    {
        if (IsDead || isHit || isRolling || isSpecialAttacking) return;

        // dash
        if (input.IsDash && !isDashing && dashCooldownTimer <= 0f)
        {
            State = PlayerState.Dash;
            return;
        }

        // attack
        if (input.IsAttack)
        {
            if(!isAttacking)
            {
                isAttacking = true;
                State = PlayerState.Attack;
                return;
            }
        }

        // sAttack
        if(!input.IsAttack && input.IsSpecialAttack && hasSpecialAttack)
        {
            if (!isSpecialAttacking)
            {
                isSpecialAttacking = true;
                hasSpecialAttack = false;
                anim.Play("Idle", 0);
                State = PlayerState.Attack;
                return;
            }
        }

        // jump
        if (input.IsJump && isGrounded && !isCrouching && !isJumping) // TODO : 점프가 어려번 눌림
        {
            State = PlayerState.Jump;
            return;
        }

        // sit
        if (input.IsCrouch)
        {
            if(!isCrouching) State = PlayerState.Crouch;
            return;
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

            return;
        }
    }

    private void AnimationUpdate()
    {
        if(!isSpecialAttacking && input.InputVec.x != 0) spriteRenderer.flipX = input.InputVec.x < 0f;
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
            case PlayerState.Attack:
                AttackStateStart();
                break;
            case PlayerState.Crouch:
                CrouchStateStart();
                break;
            case PlayerState.Roll:
                RollStateStart();
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
            case PlayerState.Attack:
                AttackState();
                break;
            case PlayerState.Crouch:
                CrouchState();
                break;
            case PlayerState.Roll:
                RollState();
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
            case PlayerState.Attack:
                AttackStateEnd();
                break;
            case PlayerState.Crouch:
                CrouchStateEnd();
                break;
            case PlayerState.Roll:
                RollStateEnd();
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

    private void AttackStateStart()
    {
        if(isAttacking)
        {
            // 일반 공격
            anim.Play("Attack" + attackCount, 0);
            attackCount++;

            if (attackCount > maxAttackCount) attackCount = 1;

            AttackCooldown = MaxAttackCooldown;

            SetAttackAreaPosition();
            attackArea.gameObject.SetActive(true);

            GameObject fx = PoolManager.Instance.Pop(PoolType.HitFX1);
            fx.transform.position = attackArea.transform.position;
        }
        else if (isSpecialAttacking)
        {
            // 특수 공격        
            specialAttackArea.gameObject.SetActive(true);
            specialAttackTimer = maxSpecialAttackTime;

            specialAttackFX = PoolManager.Instance.Pop(PoolType.Beam, transform.position + Vector3.back, Quaternion.identity);
            if (lastInputVec.x < 0f)
            {
                specialAttackAreaPivot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                specialAttackFX.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }
            else
            {
                specialAttackAreaPivot.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                specialAttackFX.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            rigid2d.velocity = Vector2.zero;
        }
    }

    private void CrouchStateStart()
    {
        isCrouching = true;
        anim.Play("Crouch", 0);
        ChangeToCrouchCollider(true);
    }

    private void RollStateStart()
    {
        isRolling = true;
        anim.Play("Roll", 0);
        rigid2d.AddForce(lastInputVec * rollPower, ForceMode2D.Impulse);
        isImmune = true;
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
            anim.Play("Run");
            rigid2d.velocity = new Vector2(input.InputVec.x * baseSpeed, rigid2d.velocity.y);
            lastInputVec = input.InputVec;

            if(input.IsRoll && !isRolling)
            {
                State = PlayerState.Roll;                
            }
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
        anim.Play("Jump", 0);

        if (isGrounded && rigid2d.velocity.y <= 0.01f)
        {
            State = PlayerState.Idle;
        }

        MoveWhileOtherState(currentSpeedWhileJump);
    }

    private void AttackState()
    {
        AttackCooldown -= Time.deltaTime;

        if(isAttacking)
        {
            if (CheckAnimationEnd())
            {
                isAttacking = false;
                State = PlayerState.Idle;
            }
        }
        else if(isSpecialAttacking)
        {
            if(specialAttackTimer <= 0.0f)
            {
                specialAttackFX.SetActive(false);
                specialAttackFX = null;
                State = PlayerState.Idle;
            }
        }
    }

    private void CrouchState()
    {
        MoveWhileOtherState(currentSpeedWhileJump);

        if (input.IsJump)
        {
            GoToPlatform();
        }

        if(!input.IsCrouch)
        {
            State = PlayerState.Idle;
        }
    }

    private void RollState()
    {
        if(CheckAnimationEnd())
        {
            State = PlayerState.Idle;
        }
    }

    private void DeadState()
    {
        if (CheckAnimationEnd())
        {
            OnDeadPerformed?.Invoke();
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
        dashTimer = 0.0f;
        rigid2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        rigid2d.velocity = Vector2.zero;
    }

    private void JumpStateEnd()
    {
        isJumping = false;
    }
    private void AttackStateEnd()
    {
        isAttacking = false;
        attackArea.gameObject.SetActive(false);
        rigid2d.velocity = Vector2.zero;
    }

    private void CrouchStateEnd()
    {
        isCrouching = false;
        ChangeToCrouchCollider(false);
        rigid2d.velocity = Vector2.zero;
    }

    private void RollStateEnd()
    {
        isRolling = false;
        isImmune = false;
        rigid2d.velocity = Vector2.zero;
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
            GameObject platform = hit.collider.gameObject;

            // 하단 플랫폼인지 확인
            if (platform.CompareTag("BottomPlatform"))
            {
                //Debug.Log("맨 밑바닥이라 내려갈 수 없음");
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

    private void MoveWhileOtherState(float value)  
    {
        rigid2d.velocity = new Vector2(input.InputVec.x * value, rigid2d.velocity.y);
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
        if (IsDead || isImmune) return;

        rigid2d.velocity = Vector2.zero; // 가속도 없애서 경직만들기
        StartCoroutine(HitAnimationProcess());
        Hp -= damageValue;
    }

    private IEnumerator HitAnimationProcess()
    {
        isHit = true;
        State = PlayerState.Idle;
        anim.Play("Hit", 0);

        yield return new WaitForSeconds(0.25f);

        anim.Play("Idle", 0);
        isHit = false;
    }

    public void OnDead()
    {
        // 사망 로직 작성
        anim.Play("Dead", 0);
    }

    // Debug ------------------------------------------------------------------------------------

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkGroundRadius);
        }
#endif
    }
}
