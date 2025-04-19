using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IAttacker
{
    private Collider2D collider2d;
    private Rigidbody2D rigid2d;

    private GameObject owner;
    private LayerMask ownerLayer;
    private Vector2 direction;
    private float speed;

    private int pierceLeft = 0;

    private float attackDamage = 0f;
    public float AttackDamage => attackDamage;

    private float attackCooldown = 0f;
    public float AttackCooldown 
    { 
        get => attackCooldown; 
        set => attackCooldown = Mathf.Clamp(value, 0.0f, MaxAttackCooldown); 
    }

    private float maxAttackCooldown = 0f;
    public float MaxAttackCooldown => maxAttackCooldown;

    bool canAttack = true;
    public bool CanAttack 
    { 
        get => canAttack; 
        set
        {
            canAttack = value;
            if (!canAttack) AttackCooldown = MaxAttackCooldown;
        }
    }

    float lifeTime = 5f;

    public Action<IDamageable> OnAttackPerformed { get; set; }

    private void Start()
    {
        collider2d = GetComponent<Collider2D>();
        rigid2d = GetComponent<Rigidbody2D>();

        collider2d.isTrigger = true;
    }

    private void Update()
    {
        if (pierceLeft < 0f || lifeTime <= 0f) Destroy(this.gameObject);

        lifeTime -= Time.deltaTime;
        AttackCooldown -= Time.deltaTime;
        if (AttackCooldown <= 0.0f) CanAttack = true;

        rigid2d.velocity = new Vector2(direction.x * speed, direction.y * speed);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return; // 생성자 공격 막기
        if (collision.gameObject.layer == ownerLayer) return;

        collision.gameObject.TryGetComponent(out IDamageable target);
        OnAttack(target);
    }
    public void Initialize(GameObject obj, Vector2 dir, float speedValue = 1, int dmg = 1, int pierceCount = 1, float maxCooldownTime = 1, float maxLifeTime = 1) // 임시, 여기도 데이터로 바꾸기
    {
        owner = obj;
        ownerLayer = obj.layer;
        direction = dir;
        speed = speedValue;
        attackDamage = dmg;
        pierceLeft = pierceCount;
        maxAttackCooldown = maxCooldownTime;
        lifeTime = maxLifeTime;
    }

    // IAttackable --------------------------------------------------------------------------------------

    public void OnAttack(IDamageable target)
    {
        if (CanAttack && target != null)
        {
            target.TakeDamage(AttackDamage);
            pierceLeft--;
            CanAttack = false;
        }
    }
}