using UnityEngine;
using System; // Guid 사용

public class DungIdentity : MonoBehaviour
{
    [Tooltip("자동 생성되는 고유 ID. 수동 수정 금지.")]
    [SerializeField] private string _saveID;

    public string ID
    {
        get
        {
            if (string.IsNullOrEmpty(_saveID)) GenerateNewID();
            return _saveID;
        }
    }

    // 에디터에서 컴포넌트 처음 추가할 때나 리셋할 때 자동 생성
    private void Reset()
    {
        GenerateNewID();
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(_saveID)) GenerateNewID();
    }

    [ContextMenu("Generate New ID")]
    private void GenerateNewID()
    {
        _saveID = Guid.NewGuid().ToString();
    }
}