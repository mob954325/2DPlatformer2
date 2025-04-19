using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_EnemyMelee : TestBase
{
    public EnemyBase enemy;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        enemy.TakeDamage(1);
    }
}
