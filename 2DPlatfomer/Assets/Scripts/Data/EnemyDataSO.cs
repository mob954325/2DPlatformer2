using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_xxx", menuName = "ScriptableObject/EnemyData", order = 0)]
public class EnemyDataSO : ScriptableObject
{
    [Header("공통 속성")]
    public float maxHp;

    [Header("전투 속성")]
    public bool isCombat;   // true일 때 아래 값 사용 
    public float sightAngle;
    public float sightRange;
    public float attackRange;
    public float moveSpeed;
    public int damage;
    public float attackCooldown;

    [Header("원거리 전용")]
    public bool isRanged; // true일 때 아래 값 사용
    public float minAttackDistance;
    public float backstepSpeed;
    public GameObject bulletPrefab;
}