using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EnemyState
{
    Idle = 0,
    Search,
    Attack,
    Dead,
}

public class EnemyBase : MonoBehaviour
{
    EnemyState state;

    public virtual void OnIdle()
    {

    }

    public virtual void OnAttack()
    {

    }  

    public virtual void 
}