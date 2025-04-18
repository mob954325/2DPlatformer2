﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    BeforeSpawn = 0,
    Idle,
    Chasing,
    Attack,
    Dead,
}

[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigid2d;

    [SerializeField] private EnemyState currentState;
    public EnemyState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState == value) return; // 중복 방지

            currentState = value;
            InitializeState(currentState);
        }
    }

    // stats
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
            Debug.Log($"{gameObject.name} : {Hp}");

            if (hp <= 0.0f)
            {
                CurrentState = EnemyState.Dead;
                OnDead();
            }
            else
            {
                OnHpChange?.Invoke();
            }
        }
    }

    private float maxHitDelay = 0.25f;
    private float hitDelay = 0.0f;

    public Action OnHpChange { get; set; }
    public Action OnHitPerformed { get; set; }
    public Action OnDeadPerformed { get; set; }

    virtual protected void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid2d = GetComponent<Rigidbody2D>();
    }

    virtual protected void OnEnable()
    {
        CurrentState = EnemyState.BeforeSpawn;
    }

    virtual protected void OnDisable()
    {
        OnHpChange = null;
        OnHitPerformed = null;
        OnDeadPerformed = null;
    }

    protected virtual void Update()
    {
        if (hitDelay >= 0.0f) hitDelay -= Time.deltaTime;
        UpdateByState();
    }


    public virtual void Initialize()
    {
        CurrentState = EnemyState.Idle;
    }

    // State ---------------------------------------------------------------------------------------

    public void UpdateByState()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                OnIdleState();
                break;
            case EnemyState.Chasing:
                OnChasingState();
                break;
            case EnemyState.Attack:
                OnAttackState();
                break;
            case EnemyState.Dead:
                OnDeadState();
                break;
        }
    }

    /// <summary>
    /// 상태 변경 후 상태 진입 전 초기화 함수 호출
    /// </summary>
    /// <param name="state">변경할 상태</param>
    public void InitializeState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                OnIdleStateStart();
                break;
            case EnemyState.Chasing:
                OnChaseStateStart();
                break;
            case EnemyState.Attack:
                OnAttackStateStart();
                break;
            case EnemyState.Dead:
                OnDeadStateStart();
                break;
        }
    }

    protected virtual void OnIdleStateStart() { }
    protected virtual void OnChaseStateStart() { }
    protected virtual void OnAttackStateStart() { }
    protected virtual void OnDeadStateStart() { }

    protected virtual void OnIdleState() { } 
    protected virtual void OnChasingState() { }
    protected virtual void OnAttackState() { }
    protected virtual void OnDeadState() { } // 상태 업데이트 용

    // IDamageable ---------------------------------------------------------------------------------------

    public void TakeDamage(float damageValue)
    {
        if (hitDelay > 0.0f) return;

        hitDelay = maxHitDelay;
        Hp -= damageValue;
        OnHitPerformed?.Invoke();

        Debug.Log($"{gameObject.name} hit!!!");
    }

    public void OnDead()
    {
        // 사망로직
        OnDeadPerformed?.Invoke();
        Debug.Log($"{gameObject.name} 사망 ");
    }
}