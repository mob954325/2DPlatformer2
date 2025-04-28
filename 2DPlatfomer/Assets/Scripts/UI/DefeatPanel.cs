using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class DefeatPanel : MonoBehaviour
{
    Player player;
    PlayerInputActions input;
    CanvasGroup cg;

    private float timer = 0;

    private bool isClick = false;

    public Action OnClick;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        Close();
    }

    private void Update()
    {
        if (cg != null && cg.alpha > 0f)
        {
            timer += Time.deltaTime;
        }
    }

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

        if (input != null) input.UI.Disable();
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
        if (timer < 1.5f) return;
        if (isClick) return;

        isClick = true;
        Close();
        input.UI.Click.performed -= Click_started;
        OnClick?.Invoke();
    }
}
