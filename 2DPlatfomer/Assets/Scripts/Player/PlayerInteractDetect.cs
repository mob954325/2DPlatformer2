using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractDetect : MonoBehaviour
{
    IInteractable target;

    public IInteractable Target { get => target; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IInteractable target);
        if(target != null)
        {
            this.target = target;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collision.gameObject.TryGetComponent(out IInteractable target);
        if (target != null)
        {
            this.target = null;
        }
    }
}