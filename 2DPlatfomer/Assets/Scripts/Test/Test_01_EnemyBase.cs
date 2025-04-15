using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_EnemyBase : TestBase
{
    public EnemyBase enemy;

    public int damage = 2;

    private void Start()
    {
        Debug.Log($"{enemy.Hp}");
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if (!enemy) return;

        enemy.MaxHp = 10;
        enemy.Hp = enemy.MaxHp;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        if (!enemy) return;
        enemy.TakeDamage(damage);
    }
}
