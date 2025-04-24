using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyMelee : EnemyCombat
{
    protected Animator animator;

    int HashToSpeed = Animator.StringToHash("Speed");
    int HashToOnAttack = Animator.StringToHash("OnAttack");
    int HashToOnDead = Animator.StringToHash("OnDead");

    protected override void OnEnable()
    {
        base.OnEnable();

        animator = GetComponent<Animator>();
        OnHitPerformed += () => { StartCoroutine(ColorChangeProcess()); };

        CurrentState = EnemyState.Idle;
    }

    protected override void Update()
    {
        spriteRenderer.flipX = isFacingLeft;

        base.Update();
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    {
        moveDirection = Vector2.zero;
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnChaseStateStart() 
    {
    }

    protected override void OnAttackStateStart() 
    {
        rigid2d.velocity = new Vector2(0.0f, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, 0.0f);
        animator.SetTrigger(HashToOnAttack);
        OnAttack(attackArea.Info.target);
    }

    protected override void OnDeadStateStart()
    {
        animator.SetTrigger(HashToOnDead);
        gameObject.SetActive(false);
    }

    protected override void OnIdleState()
    {
        if (attackArea.Info.target != null && IsInsight(attackArea.Info.targetObj.transform))
        {
            CurrentState = EnemyState.Chasing;
        }
    }

    protected override void OnChasingState()
    {
        base.OnChasingState();

        rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));

        if (attackArea.Info.targetObj == null || !IsInsight(attackArea.Info.targetObj.transform)) CurrentState = EnemyState.Idle;
        if (distanceToTarget <= attackRange && CanAttack) CurrentState = EnemyState.Attack;
    }

    protected override void OnAttackState()
    {
        base.OnAttackState();

        if(CheckAnimationEnd())
        {
            if(AttackCooldown <= 0.0f) OnAttack(attackArea.Info.target);

            else if (distanceToTarget > attackRange) CurrentState = EnemyState.Chasing;
        }
    }

    protected override void OnDeadState()
    {
        Debug.Log($"{gameObject.name} | 사망");

        base.OnDeadState();
    }

    // Functions ---------------------------------------------------------------------------------------

    protected override void OnTargetInSight()
    {
        base.OnTargetInSight();
    }

    protected override void PerformAttack(IDamageable target)
    {
        base.PerformAttack(target);
        animator.SetTrigger(HashToOnAttack);
        target.TakeDamage(AttackDamage);
    }

    IEnumerator ColorChangeProcess()
    {
        spriteRenderer.color = Color.blue;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }

    private bool CheckAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}