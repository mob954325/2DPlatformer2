using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    float MaxHp { get; set; }
    float Hp { get; set; }
    bool IsDead { get; }

    public Action OnHpChange { get; set; }
    public Action OnHitPerformed { get; set; }
    public Action OnDeadPerformed { get; set; }

    /// <summary>
    /// 피격시 호출되는 함수
    /// </summary>
    /// <param name="damageValue">입히는 데미지 값</param>
    public void TakeDamage(float damageValue);

    /// <summary>
    /// Hp가 다 닳았을 때 호출되는 함수
    /// </summary>
    void OnDead();

}
