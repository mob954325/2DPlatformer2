using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIDamageable : MonoBehaviour, IDamageable
{
    public float MaxHp { get; set; }
    public float Hp { get; set; }
    public Action OnHpChange { get; set; }
    public Action OnHitAction { get; set; }
    public Action OnDeadAction { get; set; }

    public void OnDead()
    {
    
    }

    public void TakeDamage(float damageValue)
    {
        
    }
}
