using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// 시야
/// 공격 범위
/// 데미지 전달
/// 클타임
/// 상속 받은 것은
///     공격방식을 따로 작성할 수 있도록 만들기


/// <summary>
/// 모든 전투하는 적이 공통으로 받는 클래스
/// </summary>
public class EnemyCombat : EnemyBase, IAttackable
{

    protected Transform targetTransform;
    protected IDamageable target;

    protected AttackArea attackArea;
    protected CircleCollider2D attackAreaCollider;
    protected Vector3 sightOffset = Vector3.up; // 임시

    [SerializeField] protected float sightAngle = 20.0f;
    [SerializeField] protected float sightRadius = 5.0f;

    [SerializeField] protected Vector2 moveDirection = Vector2.zero;
    [SerializeField] protected float attackRange = 2.0f;
    [SerializeField] protected float speed = 3;
    [SerializeField] protected bool isFacingLeft = true;

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

    public Action<IDamageable> OnAttackPerformed { get; set; }

    protected override void Start()
    {
        base.Start();

        attackArea = GetComponentInChildren<AttackArea>();
        attackAreaCollider = attackArea.gameObject.GetComponent<CircleCollider2D>();

        attackArea.OnActiveAttackArea += HandleTargetDetected;
        attackAreaCollider.radius = sightRadius;
        CanAttack = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        attackArea.OnActiveAttackArea = null;
    }

    protected override void Update()
    {
        base.Update();
        HandleCooldown();

        moveDirection = isFacingLeft ? Vector2.left : Vector2.right;
        spriteRenderer.flipX = isFacingLeft;
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

    // IAttackable -------------------------------------------------------------------------------------------

    protected virtual void PerformAttack(IDamageable target)
    {
        OnAttackPerformed?.Invoke(target);
    }

    public void OnAttack(IDamageable target)
    {
        if (CanAttack && target != null)
        {
            PerformAttack(target);
            CanAttack = false;
        }
    }

    void HandleCooldown()
    {
        if (!CanAttack)
        {
            AttackCooldown -= Time.deltaTime;

            if (AttackCooldown <= 0.0f)
            {
                CanAttack = true;
            }
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