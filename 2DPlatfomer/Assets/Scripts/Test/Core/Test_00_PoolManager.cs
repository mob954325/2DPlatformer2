using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_PoolManager : TestBase
{
    public Transform spawnPosition;
    public GameObject[] enemyPrefabs;

    public PoolType type;

    private void Start()
    {
        PoolManager.Instacne.Register(PoolType.EnemyMelee.ToString(), enemyPrefabs[0]);
        PoolManager.Instacne.Register(PoolType.EnemyRange.ToString(), enemyPrefabs[1]);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PoolManager.Instacne.Pop(type.ToString(), spawnPosition.position);
    }
}