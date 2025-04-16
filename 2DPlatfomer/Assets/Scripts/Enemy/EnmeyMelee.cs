using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnmeyMelee : EnemyBase
{
    [SerializeField]private Transform target;

    [SerializeField] private Vector2 moveDirection = Vector2.zero;
    private float attackRange = 2.0f;
    [SerializeField] private float speed = 3;

    protected override void Start()
    {
        base.Start();
        Initialize(10); // 임시
        attackArea.OnActiveAttackArea += HandleTargetDetected;
    }

    protected override void Update()
    {
        base.Update();
        spriteRenderer.flipX = moveDirection.x < 0;
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    { 
        moveDirection = Vector2.zero;
        CurrentState = EnemyState.Search;
    }

    protected override void OnSearchStateStart() 
    { 
        moveDirection = Vector2.left;
    }

    protected override void OnAttackStateStart() 
    {
        Debug.Log($"{gameObject} attack 상태 시작");
    }

    protected override void OnDeadStateStart() { }

    protected override void OnIdleState() { }

    protected override void OnSearchState()
    {
        if(target != null)
        {
            moveDirection = (target.position - transform.position).normalized; // 임시
            rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);         
        }
    }

    protected override void OnAttackState()
    {
        if(target != null)
        {
            // 사거리안에 플레이어가 들어옴
            moveDirection = (target.position - transform.position).normalized;
            Debug.Log($"{gameObject} 플레이어 공격");

        }
    }

    protected override void OnDeadState()
    {
    }

    // Functions ---------------------------------------------------------------------------------------

    private void HandleTargetDetected(IDamageable target, Transform targetTransform)
    {
        this.target = targetTransform;

        if (IsInSieght(targetTransform))
        {
            Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(targetTransform.position.x, targetTransform.position.y);
            float distance = Vector2.Distance(enemyPos, targetPos);

            moveDirection = (targetPos - enemyPos).normalized;

            if (distance < attackRange)
            {
                if (CurrentState != EnemyState.Attack) CurrentState = EnemyState.Attack;
            }
            else
            {
                CurrentState = EnemyState.Search;
            }
        }
    }

    bool IsInSieght(Transform target)
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float dot = Vector2.Dot(directionToTarget, Vector3.right); // 임시
        
        if(dot > Mathf.Cos(sieghtAngle * 0.5f * Mathf.Deg2Rad))
        {
            Debug.Log("Player 감지됨");
            return true;
        }

        return false;
    }



    // Debug -------------------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        if (attackArea != null)
        {
            Handles.color = Color.red;
            Handles.DrawLine(new Vector2(transform.position.x, transform.position.y + 1) , new Vector3(transform.position.x + moveDirection.x, transform.position.y + 1));

            Handles.color = Color.blue;
            Handles.DrawWireDisc(attackArea.transform.position, transform.forward, sieghtRadius);

            Handles.color = CurrentState == EnemyState.Attack ? Color.red : Color.green;

            Vector3 origin = transform.position;
            Vector3 rightDir = Quaternion.Euler(0, 0, sieghtAngle * 0.5f) * moveDirection;
            Vector3 leftDir = Quaternion.Euler(0, 0, -sieghtAngle * 0.5f) * moveDirection;

            Handles.DrawLine(origin, origin + rightDir * sieghtRadius);
            Handles.DrawLine(origin, origin + leftDir * sieghtRadius);
            Vector3 fromDir = Quaternion.Euler(0, 0, -sieghtAngle * 0.5f) * moveDirection;
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sieghtAngle, sieghtRadius);

            Handles.color = Color.yellow;
            Handles.DrawWireArc(origin, Vector3.forward, fromDir, sieghtAngle, attackRange);
        }
    }
}