using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_PlayerHp : TestBase
{
    public Player player;
    public float damage = 1f;

    public PlayerHpUI hpUI;
    public PlayerSkillUI skillUI;

    private void Start()
    {
        hpUI.Initialize(player);
        skillUI.Initialize(player);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player.MaxHp = 20;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        player.TakeDamage(damage);
    }
}
