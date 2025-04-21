using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private AttackArea attackArea;

    private void Awake()
    {
        attackArea = GetComponentInChildren<AttackArea>();
        attackArea.OnActiveAttackArea += attack;
    }

    void attack(IDamageable target, Transform targetPosition)
    {
        target.TakeDamage(1);
    }
}