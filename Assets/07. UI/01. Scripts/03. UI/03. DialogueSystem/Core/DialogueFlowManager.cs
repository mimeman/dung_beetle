using UnityEngine;
using System.Collections.Generic;

public class DialogueFlowManager : MonoBehaviour
{
    public static DialogueFlowManager Instance;

    [System.Serializable]
    public class NPCFlowData
    {
        public int NpcID;
        public int CurrentStep;
        public string Status;
    }

    [Header("Run-time Monitor")]
    [SerializeField]
    private List<NPCFlowData> _flowDatabase = new List<NPCFlowData>();

    private Dictionary<int, NPCFlowData> _flowMap = new Dictionary<int, NPCFlowData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // NPC 등록 및 현재 진도 반환
    public int RegisterNPC(int npcID, int defaultStep)
    {
        if (_flowMap.ContainsKey(npcID))
            return _flowMap[npcID].CurrentStep;

        var newData = new NPCFlowData
        {
            NpcID = npcID,
            CurrentStep = defaultStep,
            Status = "New"
        };

        _flowDatabase.Add(newData);
        _flowMap.Add(npcID, newData);

        return defaultStep;
    }

    // 진도 업데이트
    public void UpdateProgress(int npcID, int nextStep)
    {
        if (_flowMap.ContainsKey(npcID))
        {
            _flowMap[npcID].CurrentStep = nextStep;
            _flowMap[npcID].Status = (nextStep == 0) ? "Finished" : "In Progress";
        }
    }
}