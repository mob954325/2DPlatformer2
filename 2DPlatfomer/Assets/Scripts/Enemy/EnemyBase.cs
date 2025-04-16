using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle = 0,
    Search,
    Attack,
    Dead,
}

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    EnemyState state;

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
                ChangeState(EnemyState.Dead);
                OnDead();
            }
            else
            {
                OnHpChange?.Invoke();
            }
        }
    }

    // sight
    [SerializeField] float sieghtAngle = 20.0f;
    [SerializeField] float sieghtRadius = 5.0f;

    public Action OnHpChange { get; set; }
    public Action OnHitAction { get; set; }
    public Action OnDeadAction { get; set; }

    private float maxHitDelay = 0.25f;
    private float hitDelay = 0.0f;


    // unity ---------------------------------------------------------------------------------------

    virtual protected void Start()
    {
    }

    virtual protected void OnEnable()
    {

    }

    virtual protected void OnDisable()
    {
        OnHpChange = null;
        OnHitAction = null;
        OnDeadAction = null;
    }

    protected virtual void Update()
    {
        if (hitDelay >= 0.0f) hitDelay -= Time.deltaTime;
        UpdateByState();
    }

    // functions ---------------------------------------------------------------------------------------

    public void Initialize(int maxHp)
    {
        Hp = MaxHp; // 임시
        state = EnemyState.Idle;
    }

    public void UpdateByState()
    {
        switch (state)
        {
            case EnemyState.Idle:
                OnIdleState();
                break;
            case EnemyState.Search:
                OnSearchState();
                break;
            case EnemyState.Attack:
                OnAttackState();
                break;
            case EnemyState.Dead:
                OnDeadState();
                break;
        }
    }

    public void ChangeState(EnemyState state)
    {
        this.state = state;
    }

    protected virtual void OnIdleState() { } 
    protected virtual void OnSearchState() { }
    protected virtual void OnAttackState() { }
    protected virtual void OnDeadState() { } // 상태 업데이트 용

    public void TakeDamage(float damageValue)
    {
        if (hitDelay > 0.0f) return;

        hitDelay = maxHitDelay;
        Hp -= damageValue;
        OnHitAction?.Invoke();

        Debug.Log($"{gameObject.name} hit!!!");
    }

    public void OnDead()
    {
        // 사망로직
        Debug.Log($"{gameObject.name} 사망 ");
    }
}