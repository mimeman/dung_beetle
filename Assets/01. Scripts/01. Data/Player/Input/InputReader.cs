using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dung.Inputs
{
    /// <summary>
    /// 입력 신호를 받아 C# 이벤트로 알려줌
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "DungBeetle/Input/Input Reader")]
    public class InputReader : ScriptableObject, PlayerActionMap.IPlayerActions, IRebind
    {
        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> LookEvent;
        public event Action JumpStartedEvent;
        public event Action JumpCanceledEvent;
        public event Action<bool> DashEvent;
        public event Action<bool> ActionEvent;
        public event Action<bool> AimEvent;
        public event Action<bool> InteractEvent;
        public event Action ToggleViewEvent;

        private PlayerActionMap _inputActions;
        public InputActionAsset InputActions => _inputActions.asset;

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
            if (context.performed) InteractEvent?.Invoke(true);
            if (context.canceled) InteractEvent?.Invoke(false);
        }

        public void OnToggleView(InputAction.CallbackContext context)
        {
            if (context.performed) ToggleViewEvent?.Invoke();
        }

        #region Rebinding
        /// <summary>
        /// Key Binding 재설정 메소드
        /// Runtime Rebinding API 방식 사용
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="bindingIndex"></param>
        /// <param name="onComplete"></param>
        public void StartRebinding(string actionName, int bindingIndex, Action onComplete, Action onCancel = null)
        {
            var action = _inputActions.FindAction(actionName);
            action.Disable();

            action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape") // ESC로 취소
                .OnComplete(op =>
                {
                    op.Dispose();
                    action.Enable();
                    SaveBindings();
                    onComplete?.Invoke();
                })
                .OnCancel(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onCancel?.Invoke(); // 취소 콜백
                })
                .Start();
        }
        public void SaveBindings()
        {
            var json = _inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("InputBindings", json);
            PlayerPrefs.Save();
            Debug.Log($"[InputReader] 바인딩 저장 완료: {json}");
        }
        public void LoadBindings()
        {
            if (PlayerPrefs.HasKey("InputBindings"))
            {
                var json = PlayerPrefs.GetString("InputBindings");
                _inputActions.asset.LoadBindingOverridesFromJson(json);
                Debug.Log($"[InputReader] 바인딩 로드 완료: {json}"); // 로드 내용 확인
            }
            else
            {
                Debug.Log("[InputReader] 저장된 바인딩 없음");
            }
        }
        #endregion
    }
}