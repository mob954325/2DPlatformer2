using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    PlayerInputActions inputActions;

    [Header("Player")]
    private Vector3 spawnPosition = Vector3.zero;
    public GameObject playerCam;
    public Player player;
    private float remainHp;
    public int lastSavePointSceneIndex = 0; // 마지막으로 저장한 세이브 포인트 씬 인덱스 데이터

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

    bool isGameStart = false;

    protected override void Awake()
    {
        base.Awake();

        SetPoolManager();
    }

    private void OnEnable()
    {
        inputActions = new PlayerInputActions();

        inputActions.UI.Escape.Enable();
        inputActions.UI.Escape.performed += Escape_performed;
    }

    private void OnDisable()
    {
        inputActions.UI.Escape.performed -= Escape_performed;        
        inputActions.UI.Escape.Disable();     
    }

    private void Start()
    {
    }

    private void SetPoolManager()
    {
        for(int i = 0; i < (int)PoolType.PoolTypeCount; i++)
        {
            PoolManager.Instance.Register(((PoolType)i).ToString(), poolPrefab[i]);
        }
    }

    public void SetSpawnPoint(Vector3 vec)
    {
        spawnPosition = vec;
    }


    public void PlayerSpawn()
    {

        player = null;
        player = PoolManager.Instance.Pop(PoolType.Player, spawnPosition, Quaternion.identity).GetComponent<Player>();

        // 임시
        if(player.MaxHp <= 0f)
        {
            InitializePlayer();
        }

        hpUI = FindAnyObjectByType<PlayerHpUI>();
        skillUI = FindAnyObjectByType<PlayerSkillUI>();
        defeatUI = FindAnyObjectByType<DefeatPanel>();

        if (isPlayerSpawned)
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

        player.OnHpChange += SaveRemainHp;
        player.Hp = remainHp;

        isPlayerSpawned = true;
        isGameStart = true;
    }

    private void InitializePlayer()
    {   
        // note : 작동 순서가 불확실함 나중에 확인할 것
        player.MaxHp = 20f;
        if(remainHp <= 0) remainHp = player.MaxHp; // 체력 없을때만 초기화
    }

    public void ChangeScene(int index)
    {
        player = null;
        playerVcam = null;
        isPlayerSpawned = false;
        PoolManager.Instance.ClearAll();
        SceneManager.LoadScene(index);
    }

    public void SaveRemainHp(float value)
    {
        // 임시
        remainHp = value;
    }

    public void SetSavePointScene(int index)
    {
        lastSavePointSceneIndex = index;
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ChangeScene(0);
    }
}
