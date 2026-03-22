using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "DialogueDB", menuName = "Dialogue/Database")]
public class DialogueDatabase : ScriptableObject
{
    [Header("메타 정보")]
    [Tooltip("파서가 자동으로 채워주는 Stage 목록")]
    public List<string> StageList = new List<string>();

    [Tooltip("Stage별 NPC ID 인덱스")]
    [SerializeField]
    private List<StageIndexEntry> _npcIndex = new List<StageIndexEntry>();

    // Dictionary 접근용 캐시 (런타임 전용)
    private Dictionary<string, List<int>> _indexCache;

    #region 초기화

    private void OnEnable()
    {
        RebuildCache();
    }

    // Dictionary 캐시 재구성
    public void RebuildCache()
    {
        _indexCache = new Dictionary<string, List<int>>();

        foreach (var entry in _npcIndex)
        {
            _indexCache[entry.StageName] = entry.NPC_IDs;
        }
    }

    #endregion

    #region 에디터 전용 (파서가 사용)

#if UNITY_EDITOR
    // 파서가 메타 정보를 갱신
    public void UpdateMetaData(Dictionary<string, List<int>> npcGroups)
    {
        StageList.Clear();
        _npcIndex.Clear();

        foreach (var kvp in npcGroups.OrderBy(x => GetStagePriority(x.Key)))
        {
            string stageName = kvp.Key;
            List<int> npcIDs = kvp.Value;
            npcIDs.Sort();

            StageList.Add(stageName);
            _npcIndex.Add(new StageIndexEntry
            {
                StageName = stageName,
                NPC_IDs = npcIDs
            });
        }

        RebuildCache();
    }

    private int GetStagePriority(string stageName)
    {
        if (stageName == "Global") return 0;
        if (stageName == "Main") return 1;
        return 2; // Stage_XX
    }
#endif

    #endregion

    #region 런타임 로드

    // 특정 Stage의 모든 NPC SO 로드
    public List<NPCDialogueSO> LoadStage(string stageName)
    {
        List<NPCDialogueSO> result = new List<NPCDialogueSO>();

        if (_indexCache == null) RebuildCache();

        if (!_indexCache.ContainsKey(stageName))
        {
            Debug.LogWarning($"[DialogueDB] Stage '{stageName}'을 찾을 수 없습니다.");
            return result;
        }

        foreach (int npcID in _indexCache[stageName])
        {
            NPCDialogueSO npc = LoadNPC(stageName, npcID);
            if (npc != null)
                result.Add(npc);
        }

        return result;
    }

    // 특정 NPC의 SO 로드
    public NPCDialogueSO LoadNPC(string stageName, int npcID)
    {
        string path = $"Data/SO/{stageName}/NPC_{npcID}";
        NPCDialogueSO npc = Resources.Load<NPCDialogueSO>(path);

        if (npc == null)
            Debug.LogWarning($"[DialogueDB] NPC를 찾을 수 없습니다: {path}");

        return npc;
    }

    // 모든 NPC 로드 (디버그용)
    public List<NPCDialogueSO> LoadAll()
    {
        List<NPCDialogueSO> result = new List<NPCDialogueSO>();

        foreach (string stageName in StageList)
        {
            result.AddRange(LoadStage(stageName));
        }

        return result;
    }

    #endregion

    #region 헬퍼

    // Stage가 존재하는지 확인
    public bool HasStage(string stageName)
    {
        if (_indexCache == null) RebuildCache();
        return _indexCache.ContainsKey(stageName);
    }

    // Stage의 NPC 수 반환
    public int GetNPCCount(string stageName)
    {
        if (_indexCache == null) RebuildCache();

        if (_indexCache.ContainsKey(stageName))
            return _indexCache[stageName].Count;

        return 0;
    }

    #endregion
}

#region Serializable Entry (Dictionary 대신)

[System.Serializable]
public class StageIndexEntry
{
    public string StageName;
    public List<int> NPC_IDs = new List<int>();
}

#endregion