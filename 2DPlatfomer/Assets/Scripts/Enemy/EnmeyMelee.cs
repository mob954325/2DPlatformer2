using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnmeyMelee : EnemyBase
{
    // TODO : 공격 상태에서 벗어나면 계속 감지되는거 해제해야함

    [SerializeField] private Transform target;

    [SerializeField] private Vector2 moveDirection = Vector2.zero;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float speed = 3;
    [SerializeField] private bool isFaceingLeft = true;

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
        isFaceingLeft = moveDirection.x < 0;
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart() 
    { 
        moveDirection = Vector2.zero;
        CurrentState = EnemyState.Search;
    }

    protected override void OnSearchStateStart() 
    { 
        moveDirection = Vector2.left; // 처음 바라보는 위치
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
            moveDirection = isFaceingLeft ? Vector2.left : Vector2.right;
            //rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);         
        }
        else
        {
            CurrentState = EnemyState.Idle;
        }
    }

    protected override void OnAttackState()
    {
        if(target != null)
        {
            // 사거리안에 플레이어가 들어옴
            moveDirection = isFaceingLeft ? Vector2.left : Vector2.right;

        }
    }

    protected override void OnDeadState()
    {
    }

    // Functions ---------------------------------------------------------------------------------------

    private void HandleTargetDetected(IDamageable target, Transform targetTransform)
    {
        Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(targetTransform.position.x, targetTransform.position.y);
        float distance = Vector2.Distance(enemyPos, targetPos);
        // 플레이어 위치에 따라 왼쪽 오른쪽 보기

        isFaceingLeft = targetTransform.position.x - transform.position.x < 0 ? true : false;
        Debug.Log(Vector2.Dot(targetTransform.position.normalized, transform.position.normalized));
        Debug.Log($"cos {Mathf.Cos(sieghtAngle * 0.5f * Mathf.Deg2Rad)}");

        // 시야각에 있는지 확인
        if (IsInSieght(targetTransform))
        {
            Debug.Log("시야각 안에 있음");
            if (distance < attackRange)
            {
                if (CurrentState != EnemyState.Attack) CurrentState = EnemyState.Attack;
            }
            else
            {
                CurrentState = EnemyState.Search;
            }
        }

        this.target = distance <= sieghtRadius ? targetTransform : null;

    }

    /// <summary>
    /// target이 시야에 들어왔는지 확인
    /// </summary>
    bool IsInSieght(Transform target)
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float dot = Vector2.Dot(directionToTarget, isFaceingLeft ? Vector2.left : Vector2.right); 
        
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
            // 시야각 
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