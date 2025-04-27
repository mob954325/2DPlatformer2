using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_GameManager : TestBase
{
#if UNITY_EDITOR

    public Transform spawnTransform;
    public PoolType type;


    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(type, spawnTransform.position, Quaternion.identity);
    }
#endif
}
