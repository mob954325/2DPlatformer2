using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE : 적 상태머신은 상태를 계속 Update하는 형태여서 추후 리펙터링할 때 한 번만 실행되게 수정할 것

public class EnemyStage1Boss : EnemyCombat
{
    protected Animator animator;
    private Transform groundCheck;
    public GameObject specialAttackAreaPivot;
    public AttackArea specialAttackArea;

    [SerializeField] int attackComboCount = 1;
    [SerializeField] int attackMaxComboCount = 2;

    private float checkGroundRadius = 0.2f;

    public float restTimer = 0;
    public float maxRestTimer = 1.5f;
    public float maxSpecialAttackTimer = 7f;
    public float specialAttackRange = 4f;

    [SerializeField] private bool isGrounded = false;
    private bool[] UseSpecialAttackPhase = { false, false };
    private bool isSpecialAttackPhase = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        animator = GetComponent<Animator>();

        specialAttackAreaPivot = transform.GetChild(1).gameObject;
        specialAttackArea = specialAttackAreaPivot.transform.GetChild(0).GetComponent<AttackArea>();
        groundCheck = transform.GetChild(2);

        OnHitPerformed += () => 
        {
            StartCoroutine(ColorChangeProcess());
            if (isSpecialAttackPhase) return;
            StartCoroutine(EvadeOnHit());
        };
        OnAttackPerformed = PlayerAttack;
        specialAttackArea.gameObject.SetActive(false);
        CurrentState = EnemyState.Idle;
    }

    protected override void Update()
    {
        base.Update();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkGroundRadius, LayerMask.GetMask("Ground"));
        if (!isSpecialAttackPhase) spriteRenderer.flipX = isFacingLeft;

        SpecialAttackPhase();
    }

    protected override void OnIdleStateStart()
    {
        if (!isSpecialAttackPhase) animator.Play("Idle", 0);
    }

    protected override void OnChaseStateStart()
    {
        animator.Play("Walk", 0);
    }

    protected override void OnAttackStateStart()
    {
        base.OnAttackStateStart();
        rigid2d.velocity = Vector2.zero;
        animator.Play("Attack" + attackComboCount, 0);
        OnAttack(attackArea.Info.target);
    }

    protected override void OnDeadStateStart()
    {
        animator.Play("Dead", 0);
    }

    protected override void OnIdleState()
    {
        if (restTimer > 0.0f)
        {
            if (isSpecialAttackPhase && restTimer < 5f)
            {
                // 특수 공격 진행
                hitDelay = 1000f;
                specialAttackArea.gameObject.SetActive(true);
                isSpecialAttackPhase = true;
                if(CanAttack && distanceToTarget < specialAttackRange && IsInsight(attackArea.Info.targetObj != null ? attackArea.Info.targetObj.transform : null))
                {
                    OnAttack(attackArea.Info.target);
                }
            }

            restTimer -= Time.deltaTime;
        }
        else // restTimer가 끝나면 행동 시작
        {
            if(isSpecialAttackPhase) // 특수 공격 비활성화
            {
                hitDelay = 0f;
                isSpecialAttackPhase = false;
                isFacingLock = false;
                specialAttackArea.gameObject.SetActive(false);
            }

            // 시야 안에 플레이어 감지
            if (isGrounded && attackArea.Info.target != null && IsInsight(attackArea.Info.targetObj.transform))
            {
                CurrentState = EnemyState.Chasing;
            }
        }
    }

    protected override void OnChasingState()
    {
        base.OnChasingState();

        rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);

        if (attackArea.Info.targetObj == null || !IsInsight(attackArea.Info.targetObj.transform)) CurrentState = EnemyState.Idle;
        if (distanceToTarget <= attackRange)
        {            
            CurrentState = EnemyState.Attack;
        }
    }

    // NOTE : 공격 루프 안됨
    protected override void OnAttackState()
    {
        base.OnAttackState();

        if (CheckAnimationEnd())
        {
            // 공격 범위 밖으로 벗어남
            if (distanceToTarget > attackRange)
            {
                CurrentState = EnemyState.Chasing;
            }
            else // 사거리 안에 있음
            {
                attackComboCount++;

                if (attackComboCount > attackMaxComboCount) // 공격 콤보를 끝냄
                {
                    attackComboCount = 1;

                    rigid2d.AddForce(moveDirection * 8f, ForceMode2D.Impulse);
                    restTimer = maxRestTimer;

                    CurrentState = EnemyState.Idle;
                }
                else
                {
                    animator.Play("Attack" + attackComboCount, 0);
                    OnAttack(attackArea.Info.target);
                }
            }
        }
    }

    protected override void OnDeadState()
    {
        Debug.Log($"{gameObject.name} | 사망");

        if (CheckAnimationEnd())
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnAttackStateEnd()
    {
        Debug.Log("End");
    }

    // Functions ---------------------------------------------------------------------------------------

    private void PlayerAttack(IDamageable target)
    {
        target.TakeDamage(AttackDamage);
    }

    private bool CheckSpecialAttackPhase()
    {
        // float형이라 확실하게 체력 조절할 것
        if (!UseSpecialAttackPhase[0] && Hp <= 10.0f)
        {
            UseSpecialAttackPhase[0] = true;
            return true;
        }
        if (!UseSpecialAttackPhase[1] && Hp <= 3.0f)
        {
            UseSpecialAttackPhase[1] = true;
            return true;
        }

        return false; 
    }

    private void SpecialAttackPhase()
    {
        if (CheckSpecialAttackPhase() && !isSpecialAttackPhase)
        {
            restTimer = maxSpecialAttackTimer;
            isSpecialAttackPhase = true;
            rigid2d.velocity = Vector2.zero;
            animator.Play("Attack_S", 0);

            // 쏘는 방향 고정
            if (moveDirection.x < 0f)
            {
                specialAttackAreaPivot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                specialAttackAreaPivot.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            isFacingLock = true;
            CurrentState = EnemyState.Idle;
        }
    }

    private IEnumerator EvadeOnHit()
    {
        float rand = Random.value;

        if(rand <= 0.5f)
        {
            // 구르기 후 점프
            hitDelay = 1000f;
            CanAttack = false;
            isFacingLock = true;

            yield return StartCoroutine(Roll());
            Jump();

            yield return new WaitForSeconds(0.3f);

            hitDelay = 0f;
            CanAttack = true;
            isFacingLock = false;
            CurrentState = EnemyState.Idle;
        }
    }

    private void Jump()
    {
        if (isSpecialAttackPhase) return;

        animator.Play("Jump", 0);
        rigid2d.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }

    private IEnumerator Roll()
    {
        animator.Play("Roll", 0);
        float moveDirectionXValue = moveDirection.x;
        rigid2d.AddForce((moveDirectionXValue < 0f ? Vector2.left : Vector2.right) * 7f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        rigid2d.velocity = Vector2.zero;
    }

    private bool CheckAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }

    IEnumerator ColorChangeProcess()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }
}