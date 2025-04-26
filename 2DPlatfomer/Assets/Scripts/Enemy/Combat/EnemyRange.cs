using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRange : EnemyCombat
{
    private Animator animator;

    private Transform bulletTransform;
    private Vector3 bulletLocalPosition;
    private GameObject bulletPrefab;

    private float backStepSpeed = 0f;
    private float minAttackDistance = 2f;

    int HashToSpeed = Animator.StringToHash("Speed");
    int HashToOnAttack = Animator.StringToHash("OnAttack");
    int HashToOnDead = Animator.StringToHash("OnDead");

    protected override void OnEnable()
    {
        base.OnEnable();

        animator = GetComponent<Animator>();
        bulletTransform = transform.GetChild(1);
        bulletLocalPosition = bulletTransform.localPosition;

        OnHitPerformed += () => { StartCoroutine(ColorChangeProcess()); };

        CurrentState = EnemyState.Idle;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnHitPerformed = null;
    }

    protected override void Update()
    {
        spriteRenderer.flipX = isFacingLeft;

        base.Update();
    }

    protected override void SetData(EnemyDataSO data)
    {
        if(data.isRanged)
        {
            minAttackDistance = data.minAttackDistance;
            backStepSpeed = data.backstepSpeed;
            bulletPrefab = data.bulletPrefab;
        }

        base.SetData(data);
    }

    // State ---------------------------------------------------------------------------------------

    protected override void OnIdleStateStart()
    {
        base.OnIdleStateStart();
    }

    protected override void OnChaseStateStart()
    {
        base.OnChaseStateStart();
    }

    protected override void OnAttackStateStart()
    {
        base.OnAttackStateStart();
        OnAttack(attackArea.Info.target);
        animator.SetTrigger(HashToOnAttack);

        bulletTransform.localPosition = isFacingLeft ? new Vector3(bulletLocalPosition.x * -1, bulletLocalPosition.y, 0f) :
                                                         new Vector3(bulletLocalPosition.x, bulletLocalPosition.y, 0f);

        SpawnBullet();
    }

    protected override void OnDeadStateStart()
    {
        animator.SetTrigger(HashToOnDead);
    }

    protected override void OnIdleState()
    {
        animator.SetFloat(HashToSpeed, 0.0f);
        if(attackArea.Info.target != null) 
        {
            CurrentState = EnemyState.Chasing;
        }
    }

    protected override void OnChasingState()
    {
        base.OnChasingState();

        rigid2d.velocity = new Vector2(moveDirection.x * speed, rigid2d.velocity.y);
        animator.SetFloat(HashToSpeed, Mathf.Abs(moveDirection.x));

        if (attackArea.Info.target == null) CurrentState = EnemyState.Idle;
        if (distanceToTarget <= attackRange && CanAttack) CurrentState = EnemyState.Attack;
    }

    protected override void OnAttackState()
    {
        MaintainDistanceFromTarget();

        if (AttackCooldown <= 0.0f)
        {
            bulletTransform.localPosition = isFacingLeft ? new Vector3(bulletLocalPosition.x * -1, bulletLocalPosition.y, 0f) :
                                                           new Vector3(bulletLocalPosition.x, bulletLocalPosition.y, 0f);

            OnAttack(attackArea.Info.target);
            animator.SetTrigger(HashToOnAttack);
        }

        if (distanceToTarget > attackRange) CurrentState = EnemyState.Chasing;
        base.OnAttackState();
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();
        if (CheckAnimationEnd())
        {
            gameObject.SetActive(false);
        }
    }

    // Functions ---------------------------------------------------------------------------------------

    protected override void PerformAttack(IDamageable target)
    {
        Bullet bullet = Instantiate(bulletPrefab, bulletTransform.position, Quaternion.identity).GetComponent<Bullet>(); // 오브젝트 생성

        if(bullet != null)
        {
            bullet.Initialize(this.gameObject, moveDirection, 10f, 1, 1, 0.5f);
        }

        base.PerformAttack(target);
    }

    private void SpawnBullet()
    {
        Vector3 localPosition = bulletTransform.localPosition;
        float desiredDirection = isFacingLeft ? -1f : 1f;
        bulletTransform.localPosition = new Vector3(Mathf.Abs(localPosition.x) * desiredDirection, localPosition.y, localPosition.z);
    }

    private void MaintainDistanceFromTarget()
    {
        if (distanceToTarget < minAttackDistance)
        {
            rigid2d.velocity = new Vector2(-moveDirection.x * backStepSpeed, rigid2d.velocity.y);
            animator.SetFloat(HashToSpeed, 1.0f);
        }
        else
        {
            rigid2d.velocity = new Vector2(0.0f, rigid2d.velocity.y);
            animator.SetFloat(HashToSpeed, 0.0f);
        }
    }
    private bool CheckAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }

    IEnumerator ColorChangeProcess()
    {
        spriteRenderer.color = Color.blue;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }
}