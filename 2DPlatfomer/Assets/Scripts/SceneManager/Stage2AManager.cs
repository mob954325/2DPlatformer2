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
        GameManager.Instance.SetSpawnPoint(StartObject.transform.position);
        GameManager.Instance.PlayerSpawn();

        foreach (var obj in meleeSpawnPoints)
        {
            PoolManager.Instance.Pop(PoolType.EnemyMelee, obj.transform.position);
        }

        foreach(var obj in rangeSpawnPoints)
        {
            PoolManager.Instance.Pop(PoolType.EnemyRange, obj.transform.position);
        }
    }
}
