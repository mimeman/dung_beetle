using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class DialogueParser : EditorWindow
{
    private static string CSV_ROOT => Application.dataPath + "/Resources/Data/CSV/";
    private static string SO_ROOT => "Assets/Resources/Data/SO/";
    private static string DB_PATH => SO_ROOT + "DialogueDB.asset";

    #region 메인 실행

    [MenuItem("Tool/Parse Dialogue Data")]
    public static void Generate()
    {
        Debug.Log("=== 대화 파싱 시작 ===");

        if (!ValidatePaths()) return;

        DialogueDatabase db = LoadDatabase();
        if (db == null) return;

        var branchMap = LoadBranchData();

        var npcGroups = ParseAllCSVFiles(branchMap);

        CreateNPCAssets(npcGroups);

        UpdateDatabase(db, npcGroups);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"파싱 완료 Stage: {npcGroups.Count}개, NPC: {CountTotalNPCs(npcGroups)}개");
    }

    #endregion

    #region 1. 경로 검증

    private static bool ValidatePaths()
    {
        if (!Directory.Exists(CSV_ROOT))
        {
            Debug.LogError($"CSV 폴더가 없습니다: {CSV_ROOT}");
            return false;
        }

        if (!Directory.Exists(SO_ROOT))
        {
            Directory.CreateDirectory(SO_ROOT);
            Debug.Log($"SO 폴더 생성: {SO_ROOT}");
        }

        return true;
    }

    #endregion

    #region 2. DB 로드

    private static DialogueDatabase LoadDatabase()
    {
        DialogueDatabase db = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DB_PATH);

        if (db == null)
        {
            Debug.LogError($"DialogueDB를 찾을 수 없습니다: {DB_PATH}");
            return null;
        }

        return db;
    }

    #endregion

    #region 3. Branch 로드

    private static Dictionary<string, List<BranchData>> LoadBranchData()
    {
        string branchPath = FindCSVFile("Branch.csv");

        if (string.IsNullOrEmpty(branchPath))
        {
            Debug.LogWarning("Branch.csv를 찾을 수 없습니다. 선택지 없이 진행합니다.");
            return new Dictionary<string, List<BranchData>>();
        }

        return ParseBranchCSV(branchPath);
    }

    private static Dictionary<string, List<BranchData>> ParseBranchCSV(string fullPath)
    {
        var map = new Dictionary<string, List<BranchData>>();
        string[] lines = File.ReadAllLines(fullPath);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCSVLine(lines[i]);
            if (cols.Length < 4) continue;

            var branch = new BranchData
            {
                Branch_ID = ParseStr(cols[0]),
                Stage = ParseStr(cols[1]),
                Target_Step = ParseInt(cols[2]),
                Button_Text = ParseStr(cols[3])
            };

            if (!map.ContainsKey(branch.Branch_ID))
                map[branch.Branch_ID] = new List<BranchData>();

            map[branch.Branch_ID].Add(branch);
        }

        return map;
    }

    #endregion

    #region 4. CSV 파싱

    private static Dictionary<string, Dictionary<int, NPCDialogueSO>> ParseAllCSVFiles(
        Dictionary<string, List<BranchData>> branchMap)
    {
        var npcGroups = new Dictionary<string, Dictionary<int, NPCDialogueSO>>();
        string[] files = Directory.GetFiles(CSV_ROOT, "*.csv", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);

            if (filename.Contains("~") || filename.Contains("Branch"))
                continue;

            string stageName = ExtractStageFromPath(file);
            List<DialogueData> dialogues = ParseDialogueCSV(file, branchMap);

            GroupByNPC(npcGroups, stageName, dialogues);
            Debug.Log($"파싱: {filename} → {stageName}");
        }

        return npcGroups;
    }

    private static List<DialogueData> ParseDialogueCSV(
        string fullPath,
        Dictionary<string, List<BranchData>> branchMap)
    {
        var list = new List<DialogueData>();
        string[] lines = File.ReadAllLines(fullPath);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = SplitCSVLine(lines[i]);
            if (cols.Length < 11) continue;

            var data = new DialogueData
            {
                NPC_ID = ParseInt(cols[0]),
                Name = ParseStr(cols[1]),
                Stage = ParseStr(cols[2]),
                Group_ID = ParseStr(cols[3]),
                Situation = ParseStr(cols[4]),
                Step = ParseInt(cols[5]),
                Current_Text = ParseText(cols[6]),
                Next_Step = ParseInt(cols[7]),
                Branch_ID = ParseStr(cols[8]),
                Ani_Text = ParseStr(cols[9]),
                Ani_Tag = ParseStr(cols[10])
            };

            if (cols.Length > 11) data.Highlight_Keyword = ParseStr(cols[11]);
            if (cols.Length > 12) data.Highlight_Color = ParseStr(cols[12]);
            if (cols.Length > 13) data.Icon_Name = ParseStr(cols[13]);

            if (!string.IsNullOrEmpty(data.Branch_ID) && branchMap.ContainsKey(data.Branch_ID))
            {
                data.Linked_Branches = new List<BranchData>(branchMap[data.Branch_ID]);
            }

            list.Add(data);
        }

        return list;
    }

    #endregion

    #region 5. NPC 그룹화

    private static void GroupByNPC(
        Dictionary<string, Dictionary<int, NPCDialogueSO>> groups,
        string stageName,
        List<DialogueData> dialogues)
    {
        if (!groups.ContainsKey(stageName))
            groups[stageName] = new Dictionary<int, NPCDialogueSO>();

        foreach (var dialogue in dialogues)
        {
            int npcID = dialogue.NPC_ID;

            if (!groups[stageName].ContainsKey(npcID))
            {
                var npcSO = ScriptableObject.CreateInstance<NPCDialogueSO>();
                npcSO.NPC_ID = npcID;
                npcSO.NPC_Name = dialogue.Name;
                npcSO.StageName = stageName;

                groups[stageName][npcID] = npcSO;
            }

            bool isBark = !string.IsNullOrEmpty(dialogue.Situation) && dialogue.Situation != "[-]";

            if (isBark)
                groups[stageName][npcID].Barks.Add(dialogue);
            else
                groups[stageName][npcID].Interactions.Add(dialogue);
        }
    }

    #endregion

    #region 6. SO 파일 생성

    private static void CreateNPCAssets(Dictionary<string, Dictionary<int, NPCDialogueSO>> groups)
    {
        foreach (var stageEntry in groups)
        {
            string stageName = stageEntry.Key;
            string folderPath = SO_ROOT + stageName;

            EnsureFolder(folderPath);

            foreach (var npcEntry in stageEntry.Value)
            {
                int npcID = npcEntry.Key;
                NPCDialogueSO npcSO = npcEntry.Value;

                npcSO.SortData();

                string assetPath = $"{folderPath}/NPC_{npcID}.asset";

                var existing = AssetDatabase.LoadAssetAtPath<NPCDialogueSO>(assetPath);
                if (existing != null)
                {
                    existing.NPC_ID = npcSO.NPC_ID;
                    existing.NPC_Name = npcSO.NPC_Name;
                    existing.StageName = npcSO.StageName;
                    existing.Interactions = npcSO.Interactions;
                    existing.Barks = npcSO.Barks;

                    EditorUtility.SetDirty(existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(npcSO, assetPath);
                }
            }
        }
    }

    private static void EnsureFolder(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath)) return;

        string parentFolder = Path.GetDirectoryName(fullPath).Replace('\\', '/');
        string folderName = Path.GetFileName(fullPath);

        AssetDatabase.CreateFolder(parentFolder, folderName);
    }

    #endregion

    #region 7. DB 갱신

    private static void UpdateDatabase(
        DialogueDatabase db,
        Dictionary<string, Dictionary<int, NPCDialogueSO>> groups)
    {
        var npcIndex = new Dictionary<string, List<int>>();

        foreach (var stage in groups)
        {
            npcIndex[stage.Key] = new List<int>(stage.Value.Keys);
        }

        db.UpdateMetaData(npcIndex);
        EditorUtility.SetDirty(db);
    }

    #endregion

    #region 유틸리티

    private static string ExtractStageFromPath(string fullPath)
    {
        string relativePath = fullPath.Replace(CSV_ROOT, "").Replace('\\', '/');
        string[] parts = relativePath.Split('/');

        if (parts.Length > 0)
        {
            string folder = parts[0];

            if (folder == "Global" || folder.Contains("Default")) return "Global";
            if (folder == "Main") return "Main";
            if (folder.StartsWith("Stage_")) return folder;
        }

        string filename = Path.GetFileNameWithoutExtension(fullPath);
        if (filename.Contains("Default")) return "Global";
        if (filename.Contains("Main")) return "Main";
        if (filename.StartsWith("Stage_")) return filename;

        return "Unknown";
    }

    private static string FindCSVFile(string filename)
    {
        string[] files = Directory.GetFiles(CSV_ROOT, filename, SearchOption.AllDirectories);
        return files.Length > 0 ? files[0] : null;
    }

    private static int CountTotalNPCs(Dictionary<string, Dictionary<int, NPCDialogueSO>> groups)
    {
        int total = 0;
        foreach (var stage in groups.Values)
            total += stage.Count;
        return total;
    }

    #endregion

    #region CSV 파싱 헬퍼

    private static string[] SplitCSVLine(string line)
    {
        return System.Text.RegularExpressions.Regex.Split(
            line,
            ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"
        );
    }

    private static int ParseInt(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Trim() == "-" || s.Trim() == "[-]")
            return 0;

        return int.TryParse(s, out int result) ? result : 0;
    }

    private static string ParseStr(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Trim() == "-" || s.Trim() == "[-]")
            return "";

        return s.Trim().Trim('"');
    }

    private static string ParseText(string s)
    {
        return ParseStr(s)
            .Replace("/n", "\n")
            .Replace("\\n", "\n");
    }

    #endregion
}