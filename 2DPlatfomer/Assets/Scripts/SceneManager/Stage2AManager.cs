using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2AManager : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject[] meleeSpawnPoints;
    public GameObject[] rangeSpawnPoints;


    private void Start()
    {
        GameManager.Instacne.SetSpawnPoint(StartObject.transform.position);
        GameManager.Instacne.PlayerSpawn();

        foreach (var obj in meleeSpawnPoints)
        {
            PoolManager.Instacne.Pop(PoolType.EnemyMelee, obj.transform.position);
        }

        foreach(var obj in rangeSpawnPoints)
        {
            PoolManager.Instacne.Pop(PoolType.EnemyRange, obj.transform.position);
        }
    }
}
