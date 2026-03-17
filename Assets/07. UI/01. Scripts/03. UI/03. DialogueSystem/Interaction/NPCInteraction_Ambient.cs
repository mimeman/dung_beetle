using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WorldSpace Canvas에서 NPC 대화를 진행하는 컴포넌트.
/// NPCInteraction에서 Setup()을 호출하여 대화를 시작합니다.
/// </summary>
public class NPCInteraction_Ambient : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _txtName;
    [SerializeField] private TextMeshProUGUI _txtContent;

    [Header("Choices")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private ChoiceButton[] _choiceButtons;

    [Header("Next Indicator")]
    [SerializeField] private GameObject _nextIndicator;

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.03f;
    [SerializeField] private float _fastTypingSpeed = 0.01f;
    [SerializeField] private bool _useBillboard = true;

    // ── State ──
    private NPCDialogueSO _npcData;
    private DialogueData _currentDialogue;
    private int _currentStep;
    private Action<int> _onComplete;

    private Coroutine _typingCo;
    private bool _isTyping;
    private bool _isActive;
    private string _fullText;
    private string _npcName;

    private Vector3 targetPos;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Unity Lifecycle
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void Awake()
    {
        SetActive(_panelRoot, false);
        SetActive(_choicePanel, false);
        SetActive(_nextIndicator, false);
    }

    private void Update()
    {
        if (!_isActive) return;

        // 다음 넘기기
        if (Input.GetKeyDown(KeyCode.F) ||
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            OnNext();
        }
    }

    private void LateUpdate()
    {
        // Billboard
        if (_useBillboard && _isActive && !Camera.main)
            _panelRoot.transform.forward = Camera.main.transform.forward;
        // Model
        GameObject model = transform.GetComponentInChildren<Animator>().gameObject;
        if (model && _isActive)
        {
            // 1. 플레이어를 향하는 방향 벡터 계산
            Vector3 direction = targetPos - transform.position;

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

    /// <summary> 플레이어 위치기반 회전
    public void SetTarget(Vector3 target)
    {
        targetPos = target;
    }

    /// <summary> 대화 세션 시작. NPCInteraction에서 호출. </summary>
    public void Setup(NPCDialogueSO npc, int startStep, Action<int> onComplete)
    {
        _npcData = npc;
        _currentStep = startStep;
        _onComplete = onComplete;
        _isActive = true;

        if (_txtName != null) _txtName.text = npc.NPC_Name;

        SetActive(_panelRoot, true);
        ShowStep(startStep);
    }

    /// <summary> 외부에서 다음 대화로 넘길 때 사용. </summary>
    public void OnNext()
    {
        if (!_isActive) return;

        // 타이핑 중 → 스킵
        if (_isTyping) { SkipTyping(); return; }

        // 선택지 표시 중 → 무시 (버튼 클릭으로만 진행)
        if (HasBranches()) return;

        // 다음 Step
        GoToNextStep();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Dialogue Flow
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void ShowStep(int step)
    {
        var data = _npcData.GetDialogue(step);

        if (data == null)
        {
            Debug.LogWarning($"[NPCAmbient_Interaction] Step {step} 데이터를 찾을 수 없습니다.");
            CloseDialogue(0);
            return;
        }

        _currentDialogue = data;
        _fullText = data.Current_Text;
        if (_npcName != data.Name)
            _npcName = data.Name;

        HideChoices();
        SetActive(_nextIndicator, false);
        StartTyping(_fullText);
    }

    private void GoToNextStep()
    {
        if (_currentDialogue == null) return;

        int next = _currentDialogue.Next_Step;

        if (next == 0) { CloseDialogue(0); return; }

        _currentStep = next;
        ShowStep(next);
    }

    private void CloseDialogue(int nextStep)
    {
        StopTypingCo();

        _isActive = false;
        _currentDialogue = null;

        SetActive(_panelRoot, false);
        SetActive(_nextIndicator, false);
        HideChoices();

        DialogueController.NotifyDialogueState(false);

        _onComplete?.Invoke(nextStep);
        _onComplete = null;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Typing
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void StartTyping(string text)
    {
        StopTypingCo();
        _typingCo = StartCoroutine(CoTyping(text));
    }

    private IEnumerator CoTyping(string text)
    {
        _isTyping = true;
        _txtContent.text = "";

        int idx = 0;
        while (idx < text.Length)
        {
            // 리치 텍스트 태그 처리 (<color>, <sprite> 등)
            if (text[idx] == '<')
            {
                int close = text.IndexOf('>', idx);
                if (close != -1)
                {
                    _txtContent.text += text.Substring(idx, close - idx + 1);
                    idx = close + 1;
                    yield return new WaitForSeconds(GetTypingDelay());
                    continue;
                }
            }

            _txtContent.text += text[idx];
            idx++;
            yield return new WaitForSeconds(GetTypingDelay());
        }

        _isTyping = false;
        _typingCo = null;
        OnTypingComplete();
    }

    private void SkipTyping()
    {
        StopTypingCo();

        if (_txtContent != null && _fullText != null)
            _txtContent.text = _fullText;

        OnTypingComplete();
    }

    private void StopTypingCo()
    {
        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }
        _isTyping = false;
    }

    /// <summary> 타이핑 완료 후 선택지 or 다음 표시. </summary>
    private void OnTypingComplete()
    {
        if (_currentDialogue == null) return;

        if (_npcName != _txtName.text)
            _txtName.text = _npcName;

        if (HasBranches())
            ShowChoices(_currentDialogue.Linked_Branches);
        else
            SetActive(_nextIndicator, true);
    }

    private float GetTypingDelay()
    {
        bool fast = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        return fast ? _fastTypingSpeed : _typingSpeed;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Choices
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void ShowChoices(List<BranchData> branches)
    {
        if (_choicePanel == null || _choiceButtons == null) return;

        _choicePanel.SetActive(true);

        int count = Mathf.Min(branches.Count, _choiceButtons.Length);

        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            bool active = i < count;
            _choiceButtons[i].gameObject.SetActive(active);

            if (active)
                _choiceButtons[i].Setup(branches[i], OnChoiceSelected);
        }
    }

    private void HideChoices()
    {
        SetActive(_choicePanel, false);
    }

    private void OnChoiceSelected(BranchData selected)
    {
        HideChoices();

        if (selected.Target_Step == 0) { CloseDialogue(0); return; }

        _currentStep = selected.Target_Step;
        ShowStep(_currentStep);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Helpers
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private bool HasBranches()
    {
        return _currentDialogue?.Linked_Branches != null
            && _currentDialogue.Linked_Branches.Count > 0;
    }

    private static void SetActive(GameObject go, bool value)
    {
        if (go != null) go.SetActive(value);
    }
}
