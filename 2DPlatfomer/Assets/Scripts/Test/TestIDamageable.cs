using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIDamageable : MonoBehaviour, IDamageable
{
    float maxHp = 10000;
    public float MaxHp { get => maxHp; set { maxHp = value; Hp = maxHp; } }
    float hp = 0;
    public float Hp { get => hp; set => hp = value; }

    public Action<float> OnHpChange { get; set; }
    public Action OnHitPerformed { get; set; }
    public Action OnDeadPerformed { get; set; }

    public bool IsDead => Hp <= 0;

    public void OnDead()
    {
    
    }

    public void TakeDamage(float damageValue)
    {
        Hp -= damageValue;
        Debug.Log($"테스트 플레이어 공격받음 | 현재 체력 : {Hp}, 받은 데미지 : {damageValue}");
    }
}
