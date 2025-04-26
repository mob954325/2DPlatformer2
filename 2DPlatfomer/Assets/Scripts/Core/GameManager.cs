using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Player")]
    private Vector3 spawnPosition = Vector3.zero;
    public GameObject playerCam;
    public Player player;

    private Cinemachine.CinemachineVirtualCamera playerVcam;
    bool isPlayerSpawned = false;
    [Space(20f)]

    [Header("UI")]
    PlayerHpUI hpUI;
    PlayerSkillUI skillUI;
    DefeatPanel defeatUI;
    public DefeatPanel DefeatUI { get => defeatUI; }

    [Tooltip("PoolType 순서대로 오브젝트를 배치 할 것")]
    public GameObject[] poolPrefab = new GameObject[(int)PoolType.PoolTypeCount];

    protected override void Awake()
    {
        base.Awake();

        hpUI = FindAnyObjectByType<PlayerHpUI>();
        skillUI = FindAnyObjectByType<PlayerSkillUI>();
        defeatUI = FindAnyObjectByType<DefeatPanel>();

        SetPoolManager();
    }

    private void SetPoolManager()
    {
        for(int i = 0; i < (int)PoolType.PoolTypeCount; i++)
        {
            PoolManager.Instacne.Register(((PoolType)i).ToString(), poolPrefab[i]);
        }
    }

    public void SetSpawnPoint(Vector3 vec)
    {
        spawnPosition = vec;
    }


    public void PlayerSpawn()
    {
        player = null;
        player = PoolManager.Instacne.Pop(PoolType.Player, spawnPosition, Quaternion.identity).GetComponent<Player>();

        if(isPlayerSpawned)
        {
            playerVcam.Follow = player.transform;
            playerVcam.LookAt = player.transform;
        }
        else
        {
            GameObject cam = Instantiate(playerCam);
            playerVcam = cam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            playerVcam.Follow = player.transform;
            playerVcam.LookAt = player.transform;

            CinemachineConfiner2D confiner2D;
            playerVcam.TryGetComponent(out confiner2D);
            if(confiner2D == null)
            {
                confiner2D = playerVcam.AddComponent<CinemachineConfiner2D>();
                confiner2D.m_BoundingShape2D = GameObject.Find("CamBound").GetComponent<CompositeCollider2D>();
            }

        }

        hpUI.Initialize(player);
        skillUI.Initialize(player);
        defeatUI.Initialize(player);

        isPlayerSpawned = true;
    }

    public void ChangeScene(int index)
    {
        player = null;
        playerVcam = null;
        isPlayerSpawned = false;
        PoolManager.Instacne.ClearAll();
        SceneManager.LoadScene(index);
    }
}
