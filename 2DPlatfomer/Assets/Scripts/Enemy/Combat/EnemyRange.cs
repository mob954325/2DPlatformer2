using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRange : EnemyCombat
{
    private Animator animator;

    private Transform bulletTransform;
    public GameObject bulletPrefab;

    [SerializeField] private float backStepSpeed = 0f;
    [SerializeField] private float closestDistance = 2f;

    int HashToSpeed = Animator.StringToHash("Speed");
    //int HashToOnAttack = Animator.StringToHash("OnAttack");
    int HashToOnDead = Animator.StringToHash("OnDead");

    protected override void Start()
    {
        backStepSpeed = speed * 0.5f;
        sightRadius = 10f;
        attackRange = 5f;

        base.Start();

        animator = GetComponent<Animator>();

        bulletTransform = transform.GetChild(1);

        OnHitPerformed += () => { StartCoroutine(ColorChangeProcess()); };
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
    }

    protected override void OnChaseStateStart()
    {
        base.OnChaseStateStart();
    }

    protected override void OnAttackStateStart()
    {
        base.OnAttackStateStart();
    }

    protected override void OnDeadStateStart()
    {
        animator.SetTrigger(HashToOnDead);
        Destroy(this.gameObject, 0.5f); // юс╫ц
    }

    protected override void OnIdleState()
    {
        base.OnIdleState();
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnChasingState()
    {
        base.OnChasingState();
        animator.SetFloat(HashToSpeed, 0.0f);
    }

    protected override void OnAttackState()
    {
        SetBulletPosition();
        MaintainDistanceFromTarget();

        base.OnAttackState();
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();
    }

    // Functions ---------------------------------------------------------------------------------------

    protected override void PerformAttack(IDamageable target)
    {
        base.PerformAttack(target);
        Bullet bullet = Instantiate(bulletPrefab, bulletTransform.position, Quaternion.identity).GetComponent<Bullet>();

        if(bullet != null)
        {
            bullet.Initialize(this.gameObject, moveDirection, 10f, 1, 1, 0.5f);
        }
    }

    private void SetBulletPosition()
    {
        Vector3 localPosition = bulletTransform.localPosition;
        float desiredDirection = isFacingLeft ? -1f : 1f;
        bulletTransform.localPosition = new Vector3(Mathf.Abs(localPosition.x) * desiredDirection, localPosition.y, localPosition.z);
    }

    private void MaintainDistanceFromTarget()
    {
        if (distanceToTarget < closestDistance)
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

    IEnumerator ColorChangeProcess()
    {
        spriteRenderer.color = Color.blue;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }
}