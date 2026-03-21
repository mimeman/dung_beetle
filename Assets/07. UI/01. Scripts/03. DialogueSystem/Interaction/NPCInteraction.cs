using Cinemachine;
using UnityEngine;
// using Unity.Cinemachine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    // [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private CinemachineVirtualCamera _cinemachineCamera;
    [Header("Data Source")]
    [SerializeField] private NPCDialogueSO _npcData;

    [Header("Options")]
    [Tooltip("체크 해제: 종료 시 0으로 저장 (대화 불가)\n체크: 종료 시 상태 유지 (무한 반복)")]
    public bool IsRepeatable = false;

    [Tooltip("대화 종료 후 말을 걸었을 때 출력할 메시지")]
    [TextArea] public string FinishMessage = "...";

    [Header("UI References")]
    [SerializeField] private GameObject _interactionIndicator;

    [Tooltip("WorldSpace 대화 UI (연결 시 Screen-Space 대신 WorldSpace에서 대화 진행)")]
    [SerializeField] private NPCInteraction_Ambient _worldDialogueUI;
    [SerializeField] private UI_Bark _barkUI;

    public int NpcID => _npcData != null ? _npcData.NPC_ID : 0;

    private int _currentStep;
    private bool _isDialogueActive;
    Vector3 target;

    private void SetCameraPriority(int priority)
    {
        if (_cinemachineCamera != null)
            _cinemachineCamera.Priority = priority;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  IInteractable
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public InteractionType InteractType => InteractionType.Press;
    public string InteractionPrompt { get { return "[F] " + _npcData.NPC_Name + " 와 대화하기"; } }
    public bool CanInteract => !_isDialogueActive;

    public bool OnInteract(GameObject instigator)
    {
        // 대화 진행 중이면 재시작 방지
        if (_isDialogueActive) return false;

        Interact(instigator.transform.position);
        return true;
    }

    // 플레이어의 시선이 닿았을 때 (외곽선, UI 팝업)
    public void OnFocus()
    {

    }

    // 플레이어의 시선이 벗어났을 때
    public void OnLoseFocus()
    {

    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Unity Lifecycle
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void OnEnable()
    {
        DialogueController.OnDialogueStateChanged += OnDialogueStateNotified;
    }

    private void OnDisable()
    {
        DialogueController.OnDialogueStateChanged -= OnDialogueStateNotified;
    }

    /// <summary>
    /// 대화 상태 이벤트 수신. ESC 등 어떤 경로로 종료되든 _isDialogueActive 리셋 보장.
    /// </summary>
    private void OnDialogueStateNotified(bool isActive)
    {
        if (!isActive)
            _isDialogueActive = false;
        SetCameraPriority(0);
    }

    private void Start()
    {
        if (_npcData == null)
        {
            Debug.LogError($"[NPC] {gameObject.name}에 NPCDialogueSO가 연결되지 않았습니다.");
            return;
        }

        // 매니저에 등록하고 저장된 스텝 가져오기
        if (DialogueFlowManager.Instance != null)
        {
            _currentStep = DialogueFlowManager.Instance.RegisterNPC(NpcID, _npcData.GetDefaultStartStep());
        }
        else
        {
            _currentStep = _npcData.GetDefaultStartStep();
        }

        UpdateState();
    }

    private void LateUpdate()
    {
        // Model
        GameObject model = transform.GetComponentInChildren<Animator>().gameObject;
        if (model && _isDialogueActive)
        {
            // 1. 플레이어를 향하는 방향 벡터 계산
            Vector3 direction = target - transform.position;

            // 2. NPC가 위아래로 기울어지지 않도록 Y축 높이 차이 무시 (중요!)
            direction.y = 0f;

            // 3. 목표 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 4. Slerp를 이용해 현재 회전값에서 목표 회전값으로 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Public API
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public void SetFocused(bool isFocused)
    {
        if (_currentStep == 0 || _npcData == null)
        {
            if (_interactionIndicator) _interactionIndicator.SetActive(false);
            return;
        }

        if (_interactionIndicator != null)
            _interactionIndicator.SetActive(isFocused);
    }

    public void Interact(Vector3 target)
    {
        if (_npcData == null) return;

        // 최신 상태 동기화
        if (DialogueFlowManager.Instance != null)
            _currentStep = DialogueFlowManager.Instance.RegisterNPC(NpcID, _npcData.GetDefaultStartStep());

        this.target = target;

        // 대화 종료 상태면 메시지 출력
        if (_currentStep == 0)
        {
            if (_barkUI != null) _barkUI.Show(FinishMessage);
            else Debug.Log(FinishMessage);
            return;
        }

        RequestDialogueStart();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Dialogue Flow
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void RequestDialogueStart()
    {
        _isDialogueActive = true;

        // WorldSpace UI가 있으면 우선 사용
        if (_worldDialogueUI != null)
        {
            DialogueController.NotifyDialogueState(true);
            _worldDialogueUI.Setup(_npcData, _currentStep, OnDialogueEnded);

            return;
        }

        // 기존 Screen-Space 경로
        var controller = DialogueController.Instance;
        if (controller != null)
        {
            controller.StartDialogue(NpcID, _currentStep, OnDialogueEnded);
        }

        SetCameraPriority(10);
    }

    private void OnDialogueEnded(int nextStep)
    {
        _isDialogueActive = false;

        // 반복 가능한 NPC는 종료(0) 상태를 저장하지 않음
        if (IsRepeatable && nextStep == 0)
        {
            return;
        }

        // 진행 상황 저장
        _currentStep = nextStep;

        if (DialogueFlowManager.Instance != null)
        {
            DialogueFlowManager.Instance.UpdateProgress(NpcID, nextStep);
        }

        UpdateState();
        SetCameraPriority(0);
    }

    private void UpdateState()
    {
        if (_currentStep == 0 && _interactionIndicator != null)
        {
            _interactionIndicator.SetActive(false);
        }
    }
}