using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class DefeatPanel : MonoBehaviour
{
    Player player;
    PlayerInputActions input;
    CanvasGroup cg;

    private bool isClick = false;

    public void Initialize(Player player)
    {
        this.player = player;
        cg = GetComponent<CanvasGroup>();
        input = new PlayerInputActions();

        player.OnDeadPerformed += Show;

        Close();
    }

    public void Close()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        input.UI.Disable();
    }

    public void Show()
    {
        isClick = false;

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        input.UI.Enable();

        // 기존 이벤트 먼저 제거 (중복 방지)
        input.UI.Click.performed -= Click_started;
        input.UI.Click.performed += Click_started;
    }

    private void Click_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isClick) return;

        isClick = true;
        GameManager.Instacne.PlayerSpawn();
        input.UI.Click.performed -= Click_started;
        Close();
    }
}
