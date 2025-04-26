using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObstacle : MonoBehaviour, IAttacker
{
    private AttackArea attackArea;

    public float AttackDamage => 1;

    private float attackCooldown = 0f;
    public float AttackCooldown 
    { 
        get => attackCooldown; 
        set
        {
            attackCooldown = value; 
            if(attackCooldown <= 0f)
            {
                CanAttack = true;
                attackCooldown = 0f;
            }
        }
    }

    public float MaxAttackCooldown => 1f;

    private bool canAttack = true;
    public bool CanAttack { get => canAttack; set => canAttack = value; }
    public Action<IDamageable> OnAttackPerformed { get; set; }

    private void Awake()
    {
        attackArea = GetComponentInChildren<AttackArea>();
        attackArea.OnActiveAttackArea += (target, _) => { OnAttack(target); };
    }

    private void Update()
    {
        if(AttackCooldown > 0f)
        {
            AttackCooldown -= Time.deltaTime;
        }
    }

    public void OnAttack(IDamageable target)
    {
        if(CanAttack)
        {
            target.TakeDamage(AttackDamage);
            AttackCooldown = MaxAttackCooldown;
            CanAttack = false;
        }
    }
}