using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public float AttackDamage { get; }
    public void OnAttack(IDamageable target);
}
