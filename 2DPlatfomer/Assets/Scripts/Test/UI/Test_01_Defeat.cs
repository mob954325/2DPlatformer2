using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_01_Defeat : Test_00_PlayerHp
{
#if UNITY_EDITOR

    public DefeatPanel dp;
    private void Start()
    {
        dp.Initialize(player);
        hpUI.Initialize(player);
        skillUI.Initialize(player);
    }
#endif
}
