using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            info.targetObj = collision.gameObject;
            info.target = damageable;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            OnActiveAttackArea?.Invoke(damageable, collision.transform);
            //info.targetObj = collision.gameObject; 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        info.targetObj = null;
        info.target = null;
    }
}