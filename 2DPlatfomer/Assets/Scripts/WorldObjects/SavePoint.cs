using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : MonoBehaviour, IInteractable
{
    private Player player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player") return;

        other.gameObject.TryGetComponent(out Player player);
        this.player = player;       
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag != "Player") return;

        collision.gameObject.TryGetComponent(out Player player);
        Debug.Log($"{player}");
        this.player = player;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag != "Player") return;

        other.gameObject.TryGetComponent(out Player player);
        this.player = player;
        player = null;
    }

    public void Interact()
    {
        if (player == null) return;

        player.Hp = player.MaxHp;
        Debug.Log($"{player.MaxHp}");
        
        // 스폰 지점 변경
        GameObject obj = GameObject.Find("StartPoint");
        if(obj != null)
        {
            obj.transform.position = this.transform.position;
            GameManager.Instance.SetSpawnPoint(obj.transform.position);
        }

        GameManager.Instance.SetSavePointScene(SceneManager.GetActiveScene().buildIndex);
    }
}
