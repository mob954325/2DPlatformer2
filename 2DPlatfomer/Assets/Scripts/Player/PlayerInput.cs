using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions actions;

    Vector2 inputVec;
    public Vector2 InputVec { get => inputVec; }

    bool isAttack = false;
    public bool IsAttack { get => isAttack; }

    bool isDash = false;
    public bool IsDash { get => isDash; }

    bool isJump = false;
    public bool IsJump { get => isJump; }

    bool isCrouch = false;
    public bool IsCrouch { get => isCrouch; }

    private void Awake()
    {
        actions = new PlayerInputActions();

        actions.Player.Move.performed += Move_performed;
        actions.Player.Move.canceled += Move_canceled;

        actions.Player.Attack.performed += Attack_performed;
        actions.Player.Attack.canceled += Attack_canceled;

        actions.Player.Dash.performed += Dash_performed;
        actions.Player.Dash.canceled += Dash_canceled;

        actions.Player.Jump.performed += Jump_performed;
        actions.Player.Jump.canceled += Jump_canceled;

        actions.Player.Crouch.performed += Crouch_performed;
        actions.Player.Crouch.canceled += Crouch_canceled;
    }

    private void OnEnable()
    {
        actions.Player.Enable();

        actions.Player.Move.Enable();
        actions.Player.Attack.Enable();
        actions.Player.Dash.Enable();
        actions.Player.Jump.Enable();
        actions.Player.Crouch.Enable();
    }

    private void OnDisable()
    {
        actions.Player.Dash.Disable();
        actions.Player.Attack.Disable();
        actions.Player.Move.Disable();
        actions.Player.Jump.Disable();
        actions.Player.Crouch.Disable();

        actions.Player.Disable();
    }

    private void Crouch_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isCrouch = false;
    }

    private void Crouch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isCrouch = true;
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = false;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = true;
    }

    private void Dash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDash = false;
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDash = true;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = false;
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = true;
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Vector2 value = obj.ReadValue<Vector2>();
        inputVec = value;
    }

    private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        inputVec = Vector2.zero;
    }
}