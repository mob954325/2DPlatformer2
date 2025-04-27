using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1BossManager : MonoBehaviour
{
    public GameObject StartObject;
    public GameObject BossSpawnPoint;

    private EnemyStage1Boss boss;
    private VictoryPanel vPanel;

    private void Start()
    {
        GameManager.Instance.SetSpawnPoint(StartObject.transform.position);
        GameManager.Instance.PlayerSpawn();
        PoolManager.Instance.Pop(PoolType.Stage1Boss, BossSpawnPoint.transform.position);

        boss = FindAnyObjectByType<EnemyStage1Boss>();
        vPanel = FindAnyObjectByType<VictoryPanel>();

        vPanel.Initialize(GameManager.Instance.player);
    }

    private void Update()
    {
        if(boss.IsDead)
        {
            vPanel.Show();
        }
        else if(GameManager.Instance.player.IsDead && boss != null)
        {
            boss.gameObject.SetActive(false);
            boss = null;

            PoolManager.Instance.Pop(PoolType.Stage1Boss, BossSpawnPoint.transform.position);

            boss = FindAnyObjectByType<EnemyStage1Boss>();
        }
    }
}
