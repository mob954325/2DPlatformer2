using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2AManager : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject[] meleeSpawnPoints;
    public GameObject[] rangeSpawnPoints;
    private DefeatPanel defeatPanel;

    private void Start()
    {
        GameManager.Instance.SetSpawnPoint(StartObject.transform.position);
        SpawnObjects();

        defeatPanel = FindAnyObjectByType<DefeatPanel>();

        defeatPanel.OnClick += DespawnObjects;
        defeatPanel.OnClick += SpawnObjects;
    }

    private void OnDestroy()
    {
        defeatPanel.OnClick -= SpawnObjects;
        defeatPanel.OnClick -= DespawnObjects;
    }

    private void DespawnObjects()
    {
        PoolManager.Instance.DisablePool(PoolType.EnemyMelee);
        PoolManager.Instance.DisablePool(PoolType.EnemyRange);
    }

    private void SpawnObjects()
    {
        GameManager.Instance.PlayerSpawn();

        foreach (var obj in meleeSpawnPoints)
        {
            PoolManager.Instance.Pop(PoolType.EnemyMelee, obj.transform.position);
        }

        foreach (var obj in rangeSpawnPoints)
        {
            PoolManager.Instance.Pop(PoolType.EnemyRange, obj.transform.position);
        }
    }
}
