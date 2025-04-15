using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Test : EnemyBase
{
    SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        OnHitAction += OnTestEnemyHit;
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();
    }

    private void OnTestEnemyHit()
    {
        StopAllCoroutines();
        StartCoroutine(ColorChangeProcess());
    }

    IEnumerator ColorChangeProcess()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
    }
}
