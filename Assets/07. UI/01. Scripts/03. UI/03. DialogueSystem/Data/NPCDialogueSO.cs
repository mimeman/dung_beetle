using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NPC_", menuName = "Dialogue/NPC Dialogue")]
public class NPCDialogueSO : ScriptableObject
{
    [Header("NPC 정보")]
    public int NPC_ID;
    public string NPC_Name;
    public string StageName;

    [Header("상호작용 대화")]
    [Tooltip("스토리, 퀘스트, 선택지 등")]
    public List<DialogueData> Interactions = new List<DialogueData>();

    [Header("말풍선 대화")]
    [Tooltip("랜덤 출력, 배경 대사")]
    public List<DialogueData> Barks = new List<DialogueData>();

    #region 헬퍼 함수

    // 첫 번째 대화 단계 반환
    public int GetDefaultStartStep()
    {
        if (Interactions == null || Interactions.Count == 0) return 0;
        return Interactions[0].Step;
    }

    // Step으로 대화 찾기
    public DialogueData GetDialogue(int step, bool isBark = false)
    {
        List<DialogueData> target = isBark ? Barks : Interactions;
        return target.Find(x => x.Step == step);
    }

    // Group_ID로 대화 묶음 가져오기
    public List<DialogueData> GetGroup(string groupID)
    {
        return Interactions.FindAll(x => x.Group_ID == groupID);
    }

    // 랜덤 말풍선 가져오기
    public DialogueData GetRandomBark(string situation = null)
    {
        // 1. 대상 리스트 결정 (상황이 있으면 필터링, 없으면 전체)
        List<DialogueData> targetList;

        if (string.IsNullOrEmpty(situation))
        {
            targetList = Barks;
        }
        else
        {
            targetList = Barks.FindAll(x => x.Situation == situation);
        }

        // 2. 랜덤 뽑기 (데이터 없으면 null)
        if (targetList == null || targetList.Count == 0) return null;

        return targetList[Random.Range(0, targetList.Count)];
    }

    // Situation으로 말풍선 필터링
    public List<DialogueData> GetBarksBySituation(string situation)
    {
        return Barks.FindAll(x => x.Situation == situation);
    }

    #endregion

    #region 에디터 전용 (정렬)

#if UNITY_EDITOR
    public void SortData()
    {
        Interactions = Interactions.OrderBy(x => x.Step).ToList();
        Barks = Barks.OrderBy(x => x.Step).ToList();
    }
#endif

    #endregion
}