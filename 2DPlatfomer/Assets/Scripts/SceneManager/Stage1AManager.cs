using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1AManager : MonoBehaviour
{
    public GameObject StartObject;

    private void Start()
    {
        GameManager.Instacne.SetSpawnPoint(StartObject.transform.position);
        GameManager.Instacne.PlayerSpawn();
    }
}