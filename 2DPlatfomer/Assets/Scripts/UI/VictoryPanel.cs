using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    Player player;
    PlayerInputActions input;
    CanvasGroup cg;

    private float timer = 0;

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
        if(cg != null && cg.alpha > 0f)
        {
            timer += Time.deltaTime;    
        }
    }

    public void Initialize(Player player)
    {
        this.player = player;
        cg = GetComponent<CanvasGroup>();
        input = new PlayerInputActions();

        Close();
    }

    public void Close()
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Show()
    {
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
        if (timer < 3f) return;

        // 클릭 이벤트가 중복 등록되지 않도록 먼저 제거
        input.UI.Click.performed -= Click_started;

        // UI 닫기
        Close();

        // 씬 전환
        GameManager.Instance.ChangeScene(0); // menu
        GameManager.Instance.SaveRemainHp(20);

        // 입력 시스템 비활성화
        input.UI.Disable();
    }
}
