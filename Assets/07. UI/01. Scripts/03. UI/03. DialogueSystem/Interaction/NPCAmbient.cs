using UnityEngine;
using System.Collections;

public class NPCAmbient : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private NPCDialogueSO _npcData;

    [Header("Options")]
    public bool IsAutoBark = true;
    [Tooltip("CSV의 Situation 태그 (예: Surprise, OnHit)")]
    public string SituationTag = "OnHit";
    public float Interval = 5.0f;

    [Header("UI Reference")]
    [SerializeField] private UI_Bark _uiBark;

    private void Start()
    {
        if (_npcData == null)
        {
            Debug.LogError($"[NPCAmbient] {_npcData} SO 파일이 연결되지 않았습니다: {gameObject.name}");
            return;
        }

        if (IsAutoBark)
        {
            StartCoroutine(BarkRoutine());
        }
    }

    private IEnumerator BarkRoutine()
    {
        // 시작하자마자 바로 말할지, 딜레이 줄지 결정
        var wait = new WaitForSeconds(Interval);

        while (IsAutoBark)
        {
            yield return wait;

            DialogueData barkData = _npcData.GetRandomBark(SituationTag);

            if (barkData != null && _uiBark != null)
            {
                _uiBark.Show(barkData.Current_Text);
            }
        }
    }
}