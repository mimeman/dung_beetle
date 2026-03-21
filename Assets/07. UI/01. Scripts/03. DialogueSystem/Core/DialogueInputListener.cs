using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 대화 상태(DialogueController 이벤트)를 감지하여 
/// 등록된 스크립트(이동, 카메라 등)를 자동으로 끄고 켜주는 클래스
/// </summary>
public class DialogueInputListener : MonoBehaviour
{
    [Header("Control Settings")]
    [Tooltip("대화 중에 멈출(Disable) 스크립트들을 이곳에 등록하세요.")]
    [SerializeField] private List<MonoBehaviour> _scriptsToDisable;

    [Header("Cursor Settings")]
    [Tooltip("대화 중 마우스 커서를 보이게 할지 여부")]
    [SerializeField] private bool _showCursorOnDialogue = true;

    private void OnEnable()
    {
        DialogueController.OnDialogueStateChanged += HandleDialogueState;
    }

    private void OnDisable()
    {
        DialogueController.OnDialogueStateChanged -= HandleDialogueState;
    }

    private void HandleDialogueState(bool isDialogueActive)
    {
        // 1. 스크립트 제어 (대화 중이면 끄고, 끝나면 킴)
        foreach (var script in _scriptsToDisable)
        {
            if (script != null)
                script.enabled = !isDialogueActive;
        }

        // 2. 커서 제어
        if (_showCursorOnDialogue)
        {
            if (isDialogueActive)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}