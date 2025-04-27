using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_PoolManager : TestBase
{
#if UNITY_EDITOR

    public Transform spawnPosition;
    public GameObject[] enemyPrefabs;

    public PoolType type;

    private void Start()
    {
        PoolManager.Instance.Register(PoolType.EnemyMelee.ToString(), enemyPrefabs[0]);
        PoolManager.Instance.Register(PoolType.EnemyRange.ToString(), enemyPrefabs[1]);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(type.ToString(), spawnPosition.position);
    }
#endif
}