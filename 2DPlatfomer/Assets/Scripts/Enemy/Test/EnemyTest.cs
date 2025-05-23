﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : EnemyBase
{
    SpriteRenderer sprite;

    protected override void OnEnable()
    {
        base.OnEnable();
        sprite = GetComponent<SpriteRenderer>();

        OnHitPerformed += OnTestEnemyHit;
        MaxHp = 10;

        CurrentState = EnemyState.Idle;
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();
        Destroy(this.gameObject); // 임시
    }

    private void OnTestEnemyHit()
    {
        StopAllCoroutines();
        StartCoroutine(ColorChangeProcess());
    }

    IEnumerator ColorChangeProcess()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        sprite.color = Color.white;
    }
}
