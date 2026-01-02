using UnityEngine;
using Dung.Inputs;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private InputReader _inputReader;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsActionPressed { get; private set; }
    public bool IsAiming { get; private set; }

    public bool JumpTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool ActionReleasedTriggered { get; private set; }

    public bool SuppressGameplayInput { get; set; } = false;

    private void OnEnable()
    {
        if (_inputReader == null) { Debug.LogError("InputReader가 연결되지 않았습니다!"); return; }

        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook;
        _inputReader.JumpStartedEvent += OnJumpStart;
        _inputReader.JumpCanceledEvent += OnJumpEnd;
        _inputReader.DashEvent += OnDash;
        _inputReader.InteractEvent += OnInteract;
        _inputReader.ActionEvent += OnAction;
        _inputReader.AimEvent += OnAim;
    }

    private void OnDisable()
    {
        if (_inputReader == null) return;

        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;
        _inputReader.JumpStartedEvent -= OnJumpStart;
        _inputReader.JumpCanceledEvent -= OnJumpEnd;
        _inputReader.DashEvent -= OnDash;
        _inputReader.InteractEvent -= OnInteract;
        _inputReader.ActionEvent -= OnAction;
        _inputReader.AimEvent -= OnAim;
    }

    private void LateUpdate()
    {
        JumpTriggered = false;
        InteractTriggered = false;
        ActionReleasedTriggered = false;
    }

    private void OnMove(Vector2 input) => MoveInput = SuppressGameplayInput ? Vector2.zero : input;
    private void OnLook(Vector2 input) => LookInput = SuppressGameplayInput ? Vector2.zero : input;

    private void OnJumpStart()
    {
        if (SuppressGameplayInput) return;
        IsJumpPressed = true;
        JumpTriggered = true;
    }
    private void OnJumpEnd() => IsJumpPressed = false;

    private void OnDash(bool active) => IsDashPressed = !SuppressGameplayInput && active;
    private void OnInteract() { if (!SuppressGameplayInput) InteractTriggered = true; }

    private void OnAction(bool active)
    {
        if (SuppressGameplayInput) { IsActionPressed = false; return; }
        IsActionPressed = active;
        if (!active) ActionReleasedTriggered = true;
    }

    private void OnAim(bool active) => IsAiming = !SuppressGameplayInput && active;
}