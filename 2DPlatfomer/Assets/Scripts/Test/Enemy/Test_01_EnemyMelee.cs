using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_EnemyMelee : TestBase
{
#if UNITY_EDITOR

    public EnemyBase enemy;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        enemy.TakeDamage(1);
    }
#endif
}
