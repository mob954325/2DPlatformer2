using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnmeyMelee : EnemyCombat
{
    private Animator animator;

    int HashToSpeed = Animator.StringToHash("Speed");
    int HashToOnAttack = Animator.StringToHash("OnAttack");

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();

        Initialize(); // 임시
    }

    protected override void Update()
    {
        base.Update();
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    { 
        moveDirection = Vector2.zero;
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnSearchStateStart() 
    { 
        moveDirection = Vector2.left; // 처음 바라보는 위치
    }

    protected override void OnAttackStateStart() 
    {
        rigid2d.velocity = Vector2.zero;
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnDeadStateStart() { }

    protected override void OnIdleState() { Debug.Log("Idle"); }

    protected override void OnChasingState()
    {
        Debug.Log("chasing");
        if(targetTransform != null)
        {
            rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
            Vector2 dir = targetTransform.position - (transform.position + sightOffset);

            // 타겟이 범위에 벗어남
            if (dir.magnitude > 5f)
            {
                targetTransform = null;
                target = null;
            }

            animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));
        }
        else // 시야 밖으로 벗어남
        {
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnAttackState()
    {
        Debug.Log("attack");

        if (targetTransform != null)
        {
            // 사거리안에 플레이어가 들어옴
            OnAttack(target);
        }
        else // 시야 밖으로 벗어남
        {
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnDeadState()
    {
        Debug.Log($"{gameObject.name} | 사망");
    }

    // Functions ---------------------------------------------------------------------------------------

    protected override void PerformAttack(IDamageable target)
    {
        base.PerformAttack(target);
        animator.SetTrigger(HashToOnAttack);
        target.TakeDamage(AttackDamage);
    }

    // Debug -------------------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        if (attackArea != null)
        {
            // 시야각 
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