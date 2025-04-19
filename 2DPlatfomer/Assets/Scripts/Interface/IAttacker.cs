using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacker
{
    public float AttackDamage { get; }
    public float AttackCooldown { get; set; }
    public float MaxAttackCooldown { get; }
    public bool CanAttack { get; set; }
    public void OnAttack(IDamageable target);

    public Action<IDamageable> OnAttackPerformed { get; set; }
}
