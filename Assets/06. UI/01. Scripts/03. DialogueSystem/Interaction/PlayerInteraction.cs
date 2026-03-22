using UnityEngine;

/// <summary>
/// 플레이어 주변의 IInteractable 오브젝트를 감지하고 상호작용하는 컴포넌트.
/// 기존 PlayerInteraction + InteractableSensor 역할을 하나로 통합.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float _detectRadius = 3.0f;
    [SerializeField] private LayerMask _interactLayer;
    [SerializeField] private float _holdTimeout;
    [Space(10)]
    [SerializeField] private bool _debug;

    // ── State ──
    private IInteractable _currentTarget;
    private IInteractable _lastFixTarget;
    private PlayerInputHandler _input;
    private bool _isInteractFocus = false;
    private bool _interactedThisPress = false;
    private float _holdDelta;
    private bool _lastCanInteractState = false;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Unity Lifecycle
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void Start()
    {
        _input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        DetectClosestInteractable();
        HandleInput();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  1. 주변에서 가장 가까운 IInteractable 찾기
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void DetectClosestInteractable()
    {
        Vector3 origin = transform.position + _offset;
        Collider[] colliders = Physics.OverlapSphere(origin, _detectRadius, _interactLayer);

        IInteractable closest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(origin, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = interactable;
            }
        }

        if (_currentTarget != closest)
        {
            // 기존 대상 포커스 해제
            if (_currentTarget != null)
                SetFocusOnTarget(_currentTarget, false);

            _currentTarget = closest;
            _lastCanInteractState = _currentTarget != null ? _currentTarget.CanInteract : false;

            // 새 대상 포커스
            if (_currentTarget != null)
                SetFocusOnTarget(_currentTarget, true);

            UpdateInteractUI();
        }
        else
        {
            // 같은 Interactable인 상황에서 상호작용 가능 여부(CanInteract) 상태가 변했는지 체크
            if (_currentTarget != null && _lastCanInteractState != _currentTarget.CanInteract)
            {
                _lastCanInteractState = _currentTarget.CanInteract;
                UpdateInteractUI();
            }
            else if (_isInteractFocus)
            {
                UpdateInteractUI();
            }
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  2. 입력 처리 (Press / Hold 분기)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void HandleInput()
    {
        if (_currentTarget == null)
        {
            _holdDelta = 0;
            _interactedThisPress = false;
            return;
        }

        if (_input.IsInteractPressed)
        {
            if (_currentTarget.InteractType == InteractionType.Press)
            {
                // Press 타입: 버튼 1회 누름당 1회만 실행
                if (!_interactedThisPress)
                {
                    _currentTarget.OnInteract(gameObject);
                    _interactedThisPress = true;
                }
            }
            else
            {
                // Hold 타입 (대화 및 컷씬용)
                _holdDelta += Time.deltaTime;
                if (_holdDelta >= _holdTimeout && !_isInteractFocus)
                {
                    _isInteractFocus = _currentTarget.OnInteract();
                    _lastFixTarget = _currentTarget;
                }
            }
        }
        else
        {
            _holdDelta = 0;
            _interactedThisPress = false;
        }

        // ESC로 Hold 상호작용 해제
        if (Input.GetKeyDown(KeyCode.Escape) && _isInteractFocus)
        {
            if (_lastFixTarget != null)
                _isInteractFocus = _lastFixTarget.OnInteract();
        }

        if (_input.InteractFocus != _isInteractFocus)
            _input.InteractFocus = _isInteractFocus;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  3. 헬퍼
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// IInteractable 대상에 SetFocused가 있으면 호출 (NPCInteraction 등).
    /// </summary>
    private void SetFocusOnTarget(IInteractable target, bool focused)
    {
        if (target is NPCInteraction npc)
            npc.SetFocused(focused);
    }

    private void UpdateInteractUI()
    {
        if (_currentTarget == null || _isInteractFocus || !_currentTarget.CanInteract)
        {
            UIManager.Instance?.HideInteractPrompt();
            return;
        }
        UIManager.Instance?.ShowInteractPrompt(_currentTarget.InteractionPrompt);
    }

    // 4. 에디터에서 감지 범위 시각화
    private void OnDrawGizmos()
    {
        if (!_debug) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + _offset, _detectRadius);
    }
}