using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
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

        GetComponent<NPCInteraction_Ambient>().SetTarget(target);

        // 최신 상태 동기화
        if (DialogueFlowManager.Instance != null)
            _currentStep = DialogueFlowManager.Instance.RegisterNPC(NpcID, _npcData.GetDefaultStartStep());

        // 대화 종료 상태면 메시지 출력
        if (_currentStep == 0)
        {
            if (_barkUI != null) _barkUI.Show(FinishMessage);
            else Debug.Log(FinishMessage);
            return;
        }

        RequestDialogueStart();
    }

    private void RequestDialogueStart()
    {
        // WorldSpace UI가 있으면 우선 사용
        if (_worldDialogueUI != null)
        {
            DialogueController.NotifyDialogueState(true);
            _worldDialogueUI.Setup(_npcData, _currentStep, OnDialogueEnded);
            return;
        }

        // 기존 Screen-Space 경로
        var controller = FindObjectOfType<DialogueController>();
        if (controller != null)
        {
            controller.StartDialogue(NpcID, _currentStep, OnDialogueEnded);
        }
    }

    private void OnDialogueEnded(int nextStep)
    {
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
    }

    private void UpdateState()
    {
        if (_currentStep == 0 && _interactionIndicator != null)
        {
            _interactionIndicator.SetActive(false);
        }
    }
}