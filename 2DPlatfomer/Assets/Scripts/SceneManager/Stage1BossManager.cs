using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1BossManager : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject BossSpawnPoint;

    private EnemyStage1Boss boss;
    private VictoryPanel victoryPanel;
    private DefeatPanel defeatPanel;

    private void Start()
    {
        GameManager.Instance.SetSpawnPoint(StartObject.transform.position);
        SpawnObjects();

        boss = FindAnyObjectByType<EnemyStage1Boss>();
        victoryPanel = FindAnyObjectByType<VictoryPanel>();
        defeatPanel = FindAnyObjectByType<DefeatPanel>();

        victoryPanel.Initialize(GameManager.Instance.player);

        defeatPanel.OnClick += DespawnObjects;
        defeatPanel.OnClick += SpawnObjects;
    }
    private void OnDestroy()
    {
        defeatPanel.OnClick -= SpawnObjects;
        defeatPanel.OnClick -= DespawnObjects;
    }

    private void Update()
    {
        if(boss != null &&boss.IsDead)
        {
            victoryPanel.Show();
        }
        else if(GameManager.Instance.player.IsDead && boss != null)
        {
            boss.gameObject.SetActive(false);
            boss = null;
        }
    }

    private void DespawnObjects()
    {
        PoolManager.Instance.DisablePool(PoolType.Stage1Boss);
    }

    private void SpawnObjects()
    {
        GameManager.Instance.PlayerSpawn();
        PoolManager.Instance.Pop(PoolType.Stage1Boss, BossSpawnPoint.transform.position);
        boss = FindAnyObjectByType<EnemyStage1Boss>();
    }
}
