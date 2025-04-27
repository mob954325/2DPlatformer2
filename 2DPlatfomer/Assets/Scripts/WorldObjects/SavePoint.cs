using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    private Player player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.TryGetComponent(out Player player);
        this.player = player;       
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other.gameObject.TryGetComponent(out Player player);
        this.player = player;
        player = null;
    }

    public void Interact()
    {
        player.Hp = player.MaxHp;
        GameObject obj = GameObject.Find("StartPoint");
        if(obj != null)
        {
            obj.transform.position = this.transform.position;
            GameManager.Instance.SetSpawnPoint(obj.transform.position);
        }
    }
}
