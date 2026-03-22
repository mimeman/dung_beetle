#region 설명
/*
 *  1. Branch Load (선택지 로드)
 *  - 'Branch.csv'를 먼저 읽어서 Dictionary에 캐싱 (Key: Branch_ID)
 *  
 *  2. Dialogue Load (대화 로드 & 오토 링크)
 * - Main, Default, Stage_xx.csv 파일을 읽음
 * - 읽을 때 'Branch_ID'가 있으면 1번에서 캐싱한 선택지 데이터를 'Linked_Branches'에 자동 연결
 * 
 * * 3. Sort & Save (분류 및 저장)
 * - 파일명을 기준으로 SO(ScriptableObject)의 알맞은 리스트에 저장

 * A. Main 포함    -> SO.Main_Dialogues (공통 대화)
 * B. Default 포함 -> SO.Default_Dialogues (공통 말풍선)
 * C. Stage_ 시작  -> SO.Stage_List (스테이지별 컨테이너)
 
 * ㄴ Situation == [-] (비어있음) -> Interaction_Dialogues (상호작용/스토리)
 * ㄴ Situation != [-] (값이있음) -> Bark_Dialogues (말풍선/랜덤)
 */
#endregion


using System;
using System.Collections.Generic;
using UnityEngine;

#region 1. 선택지 데이터 (Branch Data)
[Serializable]
public class BranchData
{
    [Tooltip("연결 고리 ID (예: B_Shop)")]
    public string Branch_ID;
    public string Stage;
    public int Target_Step;
    public string Button_Text;
}
#endregion

#region 2. 대화 데이터 (Dialogue Data)
[Serializable]
public class DialogueData
{
    // [1] 식별 데이터
    public int NPC_ID;
    public string Name;
    public string Stage;
    public string Group_ID;

    // [2] 상황 데이터 (중요: 파서 분류 기준)
    [Tooltip("비어있으면 상호작용, 값이 있으면(Spot) 말풍선")]
    public string Situation;

    // [3] 내용 데이터
    public int Step;
    [TextArea(3, 5)]
    public string Current_Text;

    // [4] 흐름 및 연출
    public int Next_Step;
    public string Branch_ID;
    public string Ani_Text;
    public string Ani_Tag;

    public string Highlight_Keyword;
    public string Highlight_Color;
    public string Icon_Name;

    // [5] 오토 링크 (Auto-Link)
    [Tooltip("파서가 자동으로 채워주는 선택지 리스트")]
    public List<BranchData> Linked_Branches = new List<BranchData>();
}
#endregion

#region 3. 스테이지 컨테이너 (Stage Container)
[Serializable]
public class StageContainer
{
    [Tooltip("스테이지 이름 (Key 역할, 예: Stage_01)")]
    public string StageName;

    [Header("상호작용")]
    // 용도: 스토리 진행, 선택지, 게임 정지
    public List<DialogueData> Interaction_Dialogues = new List<DialogueData>();

    [Header("말풍선")]
    // 용도: 인게임 랜덤 출력, 배경 대사
    public List<DialogueData> Bark_Dialogues = new List<DialogueData>();
}
#endregion