using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFX : MonoBehaviour, IPoolable
{
    public Action ReturnAction { get; set; }
    private ParticleSystem ps;

    float timer = 0f;
    float maxTimer = 0f;

    private void OnEnable()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        maxTimer = ps.main.duration;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > maxTimer)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnDespawn()
    {
    }

    public void OnSpawn()
    {
    }
}
