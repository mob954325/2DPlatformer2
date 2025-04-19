using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// �þ�
/// ���� ����
/// ������ ����
/// ŬŸ��
/// ��� ���� ����
///     ���ݹ���� ���� �ۼ��� �� �ֵ��� �����


/// <summary>
/// ��� �����ϴ� ���� �������� �޴� Ŭ����
/// </summary>
public class EnemyCombat : EnemyBase, IAttackable
{

    protected Transform targetTransform;
    protected IDamageable target;

    protected AttackArea attackArea;
    protected CircleCollider2D attackAreaCollider;
    protected Vector3 sightOffset = Vector3.up; // �ӽ�

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
        isFacingLeft = targetTransform.position.x - transform.position.x < 0 ? true : false; // �÷��̾ ���� �ȿ� ���� ���� �ٶ󺸴� ��ġ ����

        // �þ߰��� �ִ��� Ȯ��
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
    /// target�� �þ߿� ���Դ��� Ȯ��
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
            // �þ߰� 
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