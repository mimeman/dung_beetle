using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Dung.Inputs
{
    /// <summary>
    /// 입력 신호를 받아 C# 이벤트로 알려줌
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "DungBeetle/Input/Input Reader")]
    public class InputReader : ScriptableObject, PlayerActionMap.IPlayerActions
    {
        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> LookEvent;
        public event Action JumpStartedEvent;
        public event Action JumpCanceledEvent;
        public event Action<bool> DashEvent;
        public event Action<bool> ActionEvent;
        public event Action<bool> AimEvent;
        public event Action InteractEvent;

        private PlayerActionMap _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerActionMap();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context) => MoveEvent?.Invoke(context.ReadValue<Vector2>());
        public void OnLook(InputAction.CallbackContext context) => LookEvent?.Invoke(context.ReadValue<Vector2>());

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpStartedEvent?.Invoke();
            if (context.canceled) JumpCanceledEvent?.Invoke();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed) DashEvent?.Invoke(true);
            if (context.canceled) DashEvent?.Invoke(false);
        }

        public void OnAction(InputAction.CallbackContext context)
        {
            if (context.performed) ActionEvent?.Invoke(true);
            if (context.canceled) ActionEvent?.Invoke(false);
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed) AimEvent?.Invoke(true);
            if (context.canceled) AimEvent?.Invoke(false);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) InteractEvent?.Invoke();
        }
    }
}