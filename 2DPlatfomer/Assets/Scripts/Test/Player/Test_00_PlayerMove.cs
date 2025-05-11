using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_PlayerMove : TestBase
{
#if UNITY_EDITOR
    public Player player;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player.MaxHp = 10;
        player.Hp = 10;
    }
#endif
}
