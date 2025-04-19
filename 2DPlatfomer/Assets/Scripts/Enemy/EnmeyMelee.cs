using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnmeyMelee : EnemyBase, IAttackable
{
    private Transform targetTransform;
    private Animator animator;
    private IDamageable target;
    private Vector3 sightOffset = Vector3.up;

    [SerializeField] private Vector2 moveDirection = Vector2.zero;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float speed = 3;
    [SerializeField] private bool isFacingLeft = true;

    private float attackDamage = 1f;
    public float AttackDamage => attackDamage;

    private float attackCooldown = 0f;
    public float AttackCooldown
    {
        get => attackCooldown;
        set
        {
            attackCooldown = Mathf.Clamp(value, 0.0f, MaxAttackCooldown);            
        }
    }

    private float maxAttackCooldown = 2f;
    public float MaxAttackCooldown => maxAttackCooldown;

    private bool canAttack = true;
    public bool CanAttack 
    { 
        get => canAttack; 
        set
        {
            canAttack = value;
            if (!canAttack) AttackCooldown = MaxAttackCooldown;
        }
    }
    public Action OnAttackPerformed { get; set; }

    int HashToSpeed = Animator.StringToHash("Speed");
    int HashToOnAttack = Animator.StringToHash("OnAttack");

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();

        Initialize(10); // 임시
        attackArea.OnActiveAttackArea += HandleTargetDetected;
        CanAttack = true;
    }

    protected override void Update()
    {
        base.Update();
        HandleCooldown();

        moveDirection = isFacingLeft ? Vector2.left : Vector2.right;
        spriteRenderer.flipX = isFacingLeft;
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    { 
        moveDirection = Vector2.zero;
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnSearchStateStart() 
    { 
        moveDirection = Vector2.left; // 처음 바라보는 위치
    }

    protected override void OnAttackStateStart() 
    {
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnDeadStateStart() { }

    protected override void OnIdleState() { }

    protected override void OnChasingState()
    {
        if(targetTransform != null)
        {
            rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
            Vector2 dir = targetTransform.position - (transform.position + sightOffset);

            // 타겟이 범위에 벗어남
            if (dir.magnitude > 5f)
            {
                targetTransform = null;
                target = null;
            }

            animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));
        }
        else // 시야 밖으로 벗어남
        {
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnAttackState()
    {
        if(targetTransform != null)
        {
            // 사거리안에 플레이어가 들어옴
            OnAttack(target);
        }
        else // 시야 밖으로 벗어남
        {
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnDeadState()
    {
        Debug.Log($"{gameObject.name} | 사망");
    }

    // Functions ---------------------------------------------------------------------------------------

    private void HandleTargetDetected(IDamageable target, Transform targetTransform)
    {
        isFacingLeft = targetTransform.position.x - transform.position.x < 0 ? true : false; // 플레이어가 범위 안에 있을 때만 바라보는 위치 갱신

        // 시야각에 있는지 확인
        if (IsInsight(targetTransform, out float distance))
        {
            this.targetTransform = targetTransform;
            if (distance < attackRange)
            {
                this.target = target;
                CurrentState = EnemyState.Attack;
            }
            else
            {
                CurrentState = EnemyState.Chasing;
            }
        }
        else
        {
            this.targetTransform = null;
            this.target = null;

        }
    }

    /// <summary>
    /// target이 시야에 들어왔는지 확인
    /// </summary>
    bool IsInsight(Transform target, out float distance)
    {
        Vector2 dir = (target.position - (transform.position + sightOffset));
        distance = dir.magnitude;
        float dot = Vector2.Dot(dir.normalized, isFacingLeft ? Vector2.left : Vector2.right);

        return dot > Mathf.Cos(sightAngle * 0.5f * Mathf.Deg2Rad);
    }

    void HandleCooldown()
    {
        if (!CanAttack)
        {
            AttackCooldown -= Time.deltaTime;

            if(AttackCooldown <= 0.0f)
            {
                CanAttack = true;
            }
        }
    }

    // IAttackable -------------------------------------------------------------------------------------------
    public void OnAttack(IDamageable target)
    {
        if (CanAttack && target != null)
        {
            target.TakeDamage(AttackDamage);
            OnAttackPerformed?.Invoke();
            CanAttack = false;

            animator.SetTrigger(HashToOnAttack);
        }
    }

    // Debug -------------------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        if (attackArea != null)
        {
            // 시야각 
            Handles.color = CurrentState == EnemyState.Attack ? Color.red : Color.green;

            Vector3 origin = transform.position + sightOffset;
            Vector3 rightDir = Quaternion.Euler(0, 0, sightAngle * 0.5f) * moveDirection;
            Vector3 leftDir = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * moveDirection;

            Handles.DrawLine(origin, origin + rightDir * sightRadius);
            Handles.DrawLine(origin, origin + leftDir * sightRadius);
            Vector3 fromDir = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * moveDirection;
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sightAngle, sightRadius);

            Handles.color = Color.yellow;
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sightAngle, attackRange);
        }
    }
}