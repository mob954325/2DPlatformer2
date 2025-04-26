﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;


/// <summary>
/// 모든 전투하는 적이 공통으로 받는 클래스
/// </summary>
public class EnemyCombat : EnemyBase, IAttacker
{
    protected AttackArea attackArea;
    protected CircleCollider2D attackAreaCollider;

    protected float sightAngle = 20.0f;
    protected float sightRadius = 5.0f;

    [SerializeField] protected Vector2 moveDirection = Vector2.zero;
    protected float attackRange = 5.0f;
    protected float speed = 3;
    protected float distanceToTarget = 0;
    protected bool isFacingLeft = true;
    protected bool isFacingLock = false; // 시선 고정 여부

    private float attackDamage = 1f;
    public float AttackDamage => attackDamage;

    private float currentattackCooldown = 0f;
    public float AttackCooldown
    {
        get => currentattackCooldown;
        set
        {
            currentattackCooldown = Mathf.Clamp(value, 0.0f, MaxAttackCooldown);
        }
    }

    private float maxAttackCooldown = 2f;
    public float MaxAttackCooldown => maxAttackCooldown;

    [SerializeField] private bool canAttack = true;
    public bool CanAttack
    {
        get => canAttack;
        set => canAttack = value;

    }

    protected float attackDelayTimer = 0.0f;
    protected float maxAttackDelayTime = 1.0f;

    public Action<IDamageable> OnAttackPerformed { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        attackArea = transform.GetChild(0).GetComponent<AttackArea>();
        attackAreaCollider = attackArea.gameObject.GetComponent<CircleCollider2D>();

        attackArea.OnActiveAttackArea += HandleTargetDetected;
        attackAreaCollider.radius = sightRadius;
        CanAttack = true;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        attackArea.OnActiveAttackArea = null; //remove listener 형태 사용하기 AttackArea에서
    }

    protected override void Update()
    {
        HandleCooldown();
        base.Update();
    }

    protected override void SetData(EnemyDataSO data)
    {

        if(data.isCombat)
        {
            sightAngle = data.sightAngle;
            sightRadius = data.sightRange;
            attackRange = data.attackRange;
            speed = data.moveSpeed;
            attackDamage = data.damage;
            maxAttackCooldown = data.attackCooldown;
        }

        base.SetData(data);
    }

    // Functions ---------------------------------------------------------------------------------------

    /// <summary>
    /// Chasing Update 문 | base 갱신할 때 마지막에 갱신하기
    /// </summary>
    protected override void OnChasingState()
    {
        if(attackArea.Info.targetObj != null) // 거리 업데이트
        {
            distanceToTarget = Vector2.Distance(attackArea.Info.targetObj.transform.position, (transform.position));
        }
    }

    /// <summary>
    /// Attack Update 문 | base 갱신할 때 마지막 갱신하기
    /// </summary>
    protected override void OnAttackState()
    {
        if(attackArea.Info.targetObj != null)
        {
            distanceToTarget = Vector2.Distance(attackArea.Info.targetObj.transform.position, (transform.position));
        }
    }

    // Functions ---------------------------------------------------------------------------------------

    // 계속 업데이트됨
    private void HandleTargetDetected(IDamageable target, Transform targetTransform)
    {       
        if (attackArea.Info.target == null || attackArea.Info.targetObj == null) return;

        if(!isFacingLock) // 임시
        {
            isFacingLeft = attackArea.Info.targetObj.transform.position.x - transform.position.x < 0 ? true : false; // 플레이어가 범위 안에 있을 때만 바라보는 위치 갱신
            moveDirection = isFacingLeft ? Vector2.left : Vector2.right;
        }
    }

    /// <summary>
    /// target이 시야에 들어왔는지 확인
    /// </summary>
    protected bool IsInsight(Transform target)
    {
        if (target == null) return false;

        Vector2 dir = target.position - (transform.position);
        float dot = Vector2.Dot(dir.normalized, GetFactingDirection());

        return dot > Mathf.Cos(sightAngle * 0.5f * Mathf.Deg2Rad);
    }

    Vector2 GetFactingDirection()
    {
        return isFacingLeft ? Vector2.left : Vector2.right;
    }

    /// <summary>
    /// CanAttack 비활성화, attack cooldown 초기화 하는 함수
    /// </summary>
    protected void StartAttackCoolDown()
    {
        CanAttack = false;
        AttackCooldown = MaxAttackCooldown;
    }

    // IAttackable -------------------------------------------------------------------------------------------

    public void OnAttack(IDamageable target)
    {
        if (CanAttack && target != null)
        {
            PerformAttack(target);
            StartAttackCoolDown();
        }
    }

    /// <summary>
    /// 공격 시 실행하는 추가로 실행되는 내용
    /// </summary>
    protected virtual void PerformAttack(IDamageable target)
    {
        OnAttackPerformed?.Invoke(target);
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

            Vector3 origin = transform.position;
            Vector3 rightDir = Quaternion.Euler(0, 0, sightAngle * 0.5f) * GetFactingDirection();
            Vector3 leftDir = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * GetFactingDirection();

            Handles.DrawLine(origin, origin + rightDir * sightRadius);
            Handles.DrawLine(origin, origin + leftDir * sightRadius);
            Vector3 fromDir = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * GetFactingDirection();
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sightAngle, sightRadius);

            Handles.color = Color.yellow;
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sightAngle, attackRange);
        }
    }
}