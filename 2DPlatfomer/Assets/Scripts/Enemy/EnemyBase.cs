using System;
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
public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] protected EnemyDataSO data;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigid2d;

    [SerializeField] private EnemyState currentState;
    public EnemyState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState == value) return; // 중복 방지

            StateEnd(currentState);
            currentState = value;
            StateStart(currentState);
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

            if (hp <= 0.0f)
            {
                CurrentState = EnemyState.Dead;
                OnDead();
            }
            else
            {
                OnHpChange?.Invoke(hp);
            }
        }
    }

    public bool IsDead => Hp <= 0f;

    private float maxHitDelay = 0.25f;
    [SerializeField] protected float hitDelay = 0.0f;

    public Action<float> OnHpChange { get; set; }
    public Action OnHitPerformed { get; set; }
    public Action OnDeadPerformed { get; set; }
    public Action ReturnAction { get; set; }

    virtual protected void Start()
    {

    }

    /// <summary>
    /// 상속받은 스크립트는 State Idle로 초기화 해주기
    /// </summary>
    virtual protected void OnEnable()
    {
        Initialize(data);
    }

    virtual protected void OnDisable()
    {
        StopAllCoroutines();
        OnHitPerformed = null;

        ReturnAction?.Invoke();
    }

    protected virtual void Update()
    {
        if (hitDelay >= 0.0f) hitDelay -= Time.deltaTime;
        UpdateByState();
    }

    // OnEnable에서 호출할 것
    public void Initialize(EnemyDataSO data)
    {
        // 임시
        if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if(rigid2d == null ) rigid2d = GetComponent<Rigidbody2D>();

        CurrentState = EnemyState.BeforeSpawn;
        SetData(data);
        SetAdditionalData();
    }

    /// <summary>
    /// 초기화 추가 설정 실행 함수
    /// </summary>
    protected virtual void SetAdditionalData()
    {
        
    }

    protected virtual void SetData(EnemyDataSO data)
    {
        MaxHp = data.maxHp;
    }

    // State ---------------------------------------------------------------------------------------


    /// <summary>
    /// 상태 변경 후 상태 진입 전 초기화 함수 호출
    /// </summary>
    /// <param name="state">변경할 상태</param>
    public void StateStart(EnemyState state)
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
    private void StateEnd(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                OnIdleStateEnd();
                break;
            case EnemyState.Chasing:
                OnChaseStateEnd();
                break;
            case EnemyState.Attack:
                OnAttackStateEnd();
                break;
            case EnemyState.Dead:
                OnDeadStateEnd();
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

    protected virtual void OnDeadStateEnd() { }
    protected virtual void OnAttackStateEnd() { }
    protected virtual void OnChaseStateEnd() { }
    protected virtual void OnIdleStateEnd() { }

    // IDamageable ---------------------------------------------------------------------------------------

    public void TakeDamage(float damageValue)
    {
        if (IsDead) return;
        if (hitDelay > 0.0f) return;

        hitDelay = maxHitDelay;
        Hp -= damageValue;
        OnHitPerformed?.Invoke();
    }

    public void OnDead()
    {
        // 사망로직
        if (IsDead) return;

        OnDeadPerformed?.Invoke();
    }

    public void OnSpawn()
    {
    }

    public void OnDespawn()
    {
        OnHpChange = null;
        OnHitPerformed = null;
        OnDeadPerformed = null;
    }
}