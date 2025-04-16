using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackArea : MonoBehaviour
{
    private Collider2D attackCollider;
    public Action<IDamageable, Transform> OnActiveAttackArea;

    private void Start()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            OnActiveAttackArea?.Invoke(damageable, collision.transform);
        }
    }
}