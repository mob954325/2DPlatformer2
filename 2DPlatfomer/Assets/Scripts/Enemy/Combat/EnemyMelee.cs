using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;

public class EnemyMelee : EnemyCombat
{
    private Animator animator;

    int HashToSpeed = Animator.StringToHash("Speed");
    int HashToOnAttack = Animator.StringToHash("OnAttack");
    int HashToOnDead = Animator.StringToHash("OnDead");

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        OnHitPerformed += () => { StartCoroutine(ColorChangeProcess()); };

        Initialize(); // 임시
    }

    protected override void Update()
    {
        base.Update();

        spriteRenderer.flipX = isFacingLeft;
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    {
        base.OnIdleStateStart();

        moveDirection = Vector2.zero;
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
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
        Destroy(this.gameObject, 0.5f); // 임시
    }

    protected override void OnChasingState()
    {
        rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));

        base.OnChasingState();
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();

        Debug.Log($"{gameObject.name} | 사망");
    }

    // Functions ---------------------------------------------------------------------------------------

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