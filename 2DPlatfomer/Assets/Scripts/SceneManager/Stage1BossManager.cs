using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1BossManager : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject BossSpawnPoint;


    private void Start()
    {
        GameManager.Instacne.SetSpawnPoint(StartObject.transform.position);
        GameManager.Instacne.PlayerSpawn();
        PoolManager.Instacne.Pop(PoolType.Stage1Boss, BossSpawnPoint.transform.position);
        PoolManager.Instacne.Pop(PoolType.EnemyRange, BossSpawnPoint.transform.position);
    }
}
