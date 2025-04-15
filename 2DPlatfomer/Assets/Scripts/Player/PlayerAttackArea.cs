using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackArea : MonoBehaviour
{
    public Action<IDamageable> OnActiveAttackArea;

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out EnemyBase enemy);
        if (enemy != null)
        {
            IDamageable damageable = enemy as IDamageable;
            OnActiveAttackArea?.Invoke(damageable);
        }
    }
}