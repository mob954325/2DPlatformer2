using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: 반드시 부모 오브젝트와 같은 레이어를 등록할 것

/// <summary>
/// IDamageable을 가진 오브젝트 정보
/// </summary>
public struct DamageableInfo
{
    public GameObject targetObj;
    public IDamageable target;
}

[RequireComponent(typeof(Collider2D))]
public class AttackArea : MonoBehaviour
{
    private Collider2D attackCollider;

    DamageableInfo info;
    public DamageableInfo Info { get => info; }
    public Action<IDamageable, Transform> OnActiveAttackArea; // TODO : 나중에 제거하고 코드 수정

    private void Start()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (IsSameTeam(collision)) return;

        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            info.targetObj = collision.gameObject;
            info.target = damageable;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (IsSameTeam(collision)) return;

        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            OnActiveAttackArea?.Invoke(damageable, collision.transform);
            //info.targetObj = collision.gameObject; 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (IsSameTeam(collision)) return;

        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            info.targetObj = null;
            info.target = null;
        }
    }

    private void OnDisable()
    {
        info.targetObj = null;
        info.target = null;
    }

    private bool IsSameTeam(Collider2D other)
    {
        Transform parent = transform.parent;
        return parent != null && parent.tag == other.gameObject.tag;
    }
}