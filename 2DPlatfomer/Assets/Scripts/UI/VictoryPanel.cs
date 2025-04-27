using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    Player player;
    PlayerInputActions input;
    CanvasGroup cg;

    private float timer = 0;
    private bool isClick = false;

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
        isClick = false;

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        input.UI.Enable();

        // ���� �̺�Ʈ ���� ���� (�ߺ� ����)
        input.UI.Click.performed -= Click_started;
        input.UI.Click.performed += Click_started;
    }

    private void Click_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isClick || timer < 0f) return;

        isClick = true;

        // Ŭ�� �̺�Ʈ�� �ߺ� ��ϵ��� �ʵ��� ���� ����
        input.UI.Click.performed -= Click_started;

        // UI �ݱ�
        Close();

        // �� ��ȯ
        GameManager.Instance.ChangeScene(0); // menu

        // �Է� �ý��� ��Ȱ��ȭ
        input.UI.Disable();
    }
}
