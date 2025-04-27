using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public int nextStageIndex = 0;

    public void Interact()
    {
        PoolManager.Instance.ClearAll();
        GameManager.Instance.ChangeScene(nextStageIndex);
    }
}
