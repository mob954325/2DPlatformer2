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

        base.OnIdleStateStart();
    }

    protected override void OnChaseStateStart() 
    {
        base.OnChaseStateStart();
    }

    protected override void OnAttackStateStart() 
    {
        rigid2d.velocity = new Vector2(0.0f, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, 0.0f);

        base.OnAttackStateStart();
    }

    protected override void OnDeadStateStart()
    {
        animator.SetTrigger(HashToOnDead);
        gameObject.SetActive(false);
    }

    protected override void OnChasingState()
    {
        base.OnChasingState();

        rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));

        if (attackArea.Info.targetObj == null) CurrentState = EnemyState.Idle;
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
}