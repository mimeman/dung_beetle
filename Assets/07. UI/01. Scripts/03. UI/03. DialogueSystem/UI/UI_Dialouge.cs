using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(DialogueTyper))]
public class UI_Dialogue : UIBase
{
    [Header("═══ 애니메이션 ═══")]
    [Tooltip("텍스트 박스 애니메이션")]
    [SerializeField] private UI_TweenAnimator _textBaseAnim;

    [Tooltip("이름 박스 애니메이션")]
    [SerializeField] private UI_TweenAnimator _nameBaseAnim;

    [Tooltip("선택지 리스트 컨트롤러 (Content_Choices 할당)")]
    [SerializeField] private UI_TweenListController _choiceListController;

    [Space(10)]
    [Header("═══ 타이핑 설정 ═══")]
    [Tooltip("일반 타이핑 속도 (글자당 초)")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float _normalSpeed = 0.05f;

    [Tooltip("빠른 타이핑 속도 (스킵 시)")]
    [Range(0.001f, 0.05f)]
    [SerializeField] private float _fastSpeed = 0.01f;

    private TextMeshProUGUI _txtName;
    private TextMeshProUGUI _txtContent;
    private GameObject _panelChoices;
    private DialogueTyper _typer;
    private List<ChoiceButton> _choiceButtons = new List<ChoiceButton>();

    private NPCDialogueSO _currentNPC;
    private DialogueData _currentData;
    private System.Action<int> _onEndCallback;
    private int _lastNextStep;

    public override void Init()
    {
        base.Init();
        BindUI();
        _typer = GetComponent<DialogueTyper>();
        _typer.Init(_txtContent);
    }

    private void BindUI()
    {
        _txtName = UIBinder.Bind<TextMeshProUGUI>(gameObject, "Txt_Name");
        _txtContent = UIBinder.Bind<TextMeshProUGUI>(gameObject, "Txt_Content");
        _panelChoices = UIBinder.Bind<Transform>(gameObject, "Panel_Choices").gameObject;

        _choiceButtons.Clear();
        if (_choiceListController != null)
        {
            foreach (var item in _choiceListController.Items)
            {
                var btn = item.GetComponent<ChoiceButton>();
                if (btn != null)
                    _choiceButtons.Add(btn);
            }
        }
    }

    public void Setup(NPCDialogueSO npc, int startStep, System.Action<int> onEnd)
    {
        gameObject.SetActive(true);
        UIManager.Instance.SetCursorState(true);

        _textBaseAnim?.PlayShow();
        _nameBaseAnim?.PlayShow();

        _currentNPC = npc;
        _onEndCallback = onEnd;
        _lastNextStep = startStep;

        ShowStep(startStep);
    }

    public void OnNext()
    {
        if (_panelChoices.activeSelf) return;

        if (_typer.IsTyping)
        {
            _typer.StopTyping();
            _txtContent.text = ApplyStyle(_currentData.Current_Text, _currentData.Highlight_Keyword, _currentData.Highlight_Color);
            CheckPostDialogue();
        }
        else
        {
            ProceedToNextStep();
        }
    }

    public void OnFullSkip() => CloseDialogue();

    private void ProceedToNextStep()
    {
        if (_currentData.Next_Step != 0)
            ShowStep(_currentData.Next_Step);
        else
            CloseDialogue();
    }

    private void CloseDialogue()
    {
        _typer.StopTyping();
        UIManager.Instance.SetCursorState(false);

        _textBaseAnim?.PlayHide();
        _nameBaseAnim?.PlayHide();

        Invoke(nameof(HideObject), 0.3f);
    }

    private void HideObject()
    {
        gameObject.SetActive(false);
        UIManager.Instance.Hide<UI_Dialogue>();
        _onEndCallback?.Invoke(_lastNextStep);
    }

    private void ShowStep(int step)
    {
        _currentData = _currentNPC.GetDialogue(step);

        if (_currentData == null)
        {
            CloseDialogue();
            return;
        }

        _lastNextStep = _currentData.Next_Step;

        if (_txtName.text != _currentNPC.NPC_Name)
        {
            _txtName.text = _currentNPC.NPC_Name;
            _nameBaseAnim?.PlayEmphasis(UIEmphasisType.PunchScale);
        }

        _panelChoices.SetActive(false);

        string finalContent = ApplyStyle(_currentData.Current_Text, _currentData.Highlight_Keyword, _currentData.Highlight_Color);
        _typer.StartTyping(finalContent, _normalSpeed, _fastSpeed, CheckPostDialogue);
    }

    private void CheckPostDialogue()
    {
        if (_currentData.Linked_Branches.Count > 0)
        {
            ShowChoices(_currentData.Linked_Branches);
        }
    }

    private void ShowChoices(List<BranchData> branches)
    {
        _panelChoices.SetActive(true);

        int count = Mathf.Min(branches.Count, _choiceButtons.Count);

        _choiceListController?.SetActiveCount(count);

        for (int i = 0; i < count; i++)
        {
            _choiceButtons[i].Setup(branches[i], OnChoiceSelected);
        }

        _choiceListController?.PlayShowActiveItems();
    }

    private void OnChoiceSelected(BranchData selected)
    {
        if (selected.Stage == _currentNPC.StageName)
        {
            ShowStep(selected.Target_Step);
        }
        else
        {
            _lastNextStep = selected.Target_Step;
            CloseDialogue();
        }
    }

    private string ApplyStyle(string originalText, string keyword, string color)
    {
        if (string.IsNullOrEmpty(keyword))
            return originalText;

        string styled = keyword;

        if (!string.IsNullOrEmpty(color))
            styled = $"<color={color}>{styled}</color>";

        styled = $"<b>{styled}</b>";

        return originalText.Replace(keyword, styled);
    }
}