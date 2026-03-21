using UnityEngine;
using System.Collections.Generic;
using System;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private string _stageName = "Main";

    public static event Action<bool> OnDialogueStateChanged;

    /// <summary>
    /// 외부에서 대화 상태 이벤트를 발행할 수 있도록 하는 헬퍼.
    /// (event는 선언 클래스 외부에서 직접 Invoke 불가)
    /// </summary>
    public static void NotifyDialogueState(bool isActive)
    {
        OnDialogueStateChanged?.Invoke(isActive);
    }

    private DialogueDatabase _db;
    private List<NPCDialogueSO> _stageNPCs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        _db = Resources.Load<DialogueDatabase>("Data/SO/DialogueDB");

        if (_db == null)
        {
            Debug.LogError("[DialogueController] DialogueDB를 찾을 수 없습니다!");
            return;
        }

        _stageNPCs = new List<NPCDialogueSO>();

        var globalData = _db.LoadStage("Global");
        if (globalData != null) _stageNPCs.AddRange(globalData);

        if (!string.IsNullOrEmpty(_stageName))
        {
            var stageData = _db.LoadStage(_stageName);
            if (stageData != null) _stageNPCs.AddRange(stageData);
        }
    }

    #region Public API

    public void StartDialogue(int npcID, int step, Action<int> onComplete)
    {
        NPCDialogueSO npc = GetNPC(npcID);
        if (npc == null)
        {
            Debug.LogError($"[DialogueController] NPC {npcID}를 찾을 수 없습니다.");
            return;
        }

        // 1. 대화 시작 방송
        OnDialogueStateChanged?.Invoke(true);

        var ui = UIManager.Instance.Show<UI_Dialogue>();

        ui.Setup(npc, step, (nextStep) =>
        {
            onComplete?.Invoke(nextStep);

            // 2. 대화 종료 방송
            OnDialogueStateChanged?.Invoke(false);
        });
    }

    public void StartDialogueGroup(int npcID, string groupID, Action<int> onComplete = null)
    {
        NPCDialogueSO npc = GetNPC(npcID);

        if (npc == null) return;

        List<DialogueData> group = npc.GetGroup(groupID);

        if (group == null || group.Count == 0) return;

        // 1. 대화 시작 방송
        OnDialogueStateChanged?.Invoke(true);

        var ui = UIManager.Instance.Show<UI_Dialogue>();

        ui.Setup(npc, group[0].Step, (nextStep) =>
        {
            onComplete?.Invoke(nextStep);

            // 2. 대화 종료 방송
            OnDialogueStateChanged?.Invoke(false);
        });
    }

    public string GetRandomBark(int npcID, string situation = null)
    {
        NPCDialogueSO npc = GetNPC(npcID);
        if (npc == null) return "";

        DialogueData bark = npc.GetRandomBark(situation);
        return bark?.Current_Text ?? "";
    }

    public NPCDialogueSO GetNPC(int npcID)
    {
        if (_stageNPCs == null) return null;
        return _stageNPCs.Find(x => x.NPC_ID == npcID);
    }

    public bool IsDialogueActive()
    {
        return UIManager.Instance.IsPopupOpen;
    }

    #endregion

    #region Debug
    [ContextMenu("현재 Stage NPC 목록 출력")]
    private void DebugPrintNPCs()
    {
        if (_stageNPCs == null) return;
        foreach (var npc in _stageNPCs)
        {
            Debug.Log($"NPC_{npc.NPC_ID}: {npc.NPC_Name}");
        }
    }
    #endregion
}