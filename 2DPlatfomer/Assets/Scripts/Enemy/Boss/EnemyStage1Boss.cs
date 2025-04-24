using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE : 적 상태머신은 상태를 계속 Update하는 형태여서 추후 리펙터링할 때 한 번만 실행되게 수정할 것

public class EnemyStage1Boss : EnemyMelee
{
    public int currentAttackedCount = 0;
    public int maxAttackedCount = 3;

    public float restTimer = 0;
    public float maxRestTimer = 2.0f;

    protected override void OnAttackStateStart()
    {
        base.OnAttackStateStart();

        int prev = currentAttackedCount;
        if(currentAttackedCount == prev) currentAttackedCount++; // 임시 

        if(currentAttackedCount > maxAttackedCount)
        {
            restTimer = maxRestTimer;
            currentAttackedCount = 0;
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnIdleState()
    {
        base.OnIdleState();

        if (restTimer > 0f) restTimer -= Time.deltaTime;

        if(restTimer <= 0f) // aaaa
        {
            if(attackArea.Info.target != null)
            {
                CurrentState = EnemyState.Chasing;
            }
        }
    }

    // NOTE : 공격 루프 안됨
    protected override void OnAttackState()
    {
        base.OnAttackState();

        if (CheckAnimationEnd())
        {
            Debug.Log("Asdf");
            CurrentState = EnemyState.Idle;
        }
    }

    // Functions ---------------------------------------------------------------------------------------

    protected override void OnTargetInSight()
    {
        if (restTimer > 0f) return;

        base.OnTargetInSight();
    }

    private bool CheckAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}