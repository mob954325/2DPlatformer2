using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackArea : MonoBehaviour
{
    public Action<IDamageable> OnActiveAttackArea;

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IDamageable damageable);
        if (damageable != null && damageable != GetComponentInParent<IDamageable>())
        {
            OnActiveAttackArea?.Invoke(damageable);
        }
    }
}