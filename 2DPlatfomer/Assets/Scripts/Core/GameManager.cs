using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Tooltip("PoolType 순서대로 오브젝트를 배치 할 것")]
    public GameObject[] poolPrefab = new GameObject[(int)PoolType.PoolTypeCount];

    protected override void Awake()
    {
        base.Awake();
        SetPoolManager();
    }

    private void SetPoolManager()
    {
        for(int i = 0; i < (int)PoolType.PoolTypeCount; i++)
        {
            PoolManager.Instacne.Register(((PoolType)i).ToString(), poolPrefab[i]);
        }
    }
}
