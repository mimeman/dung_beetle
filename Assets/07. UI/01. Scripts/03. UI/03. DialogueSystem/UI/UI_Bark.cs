using UnityEngine;
using TMPro;
using System.Collections;

public class UI_Bark : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject _panelRoot;    // 말풍선 배경 (Image)
    [SerializeField] private TextMeshProUGUI _txtContent; // 내용 (TMP)

    [Header("Settings")]
    [SerializeField] private float _duration = 3.0f;   // 떠있는 시간

    private Coroutine _co;

    void Awake()
    {
        // 시작하면 꺼두기
        if (_panelRoot) _panelRoot.SetActive(false);
    }

    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 켜기
        _panelRoot.SetActive(true);
        _txtContent.text = text;

        // 기존 타이머 끄고 새로 시작
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CloseDelay());
    }

    IEnumerator CloseDelay()
    {
        yield return new WaitForSeconds(_duration);
        _panelRoot.SetActive(false);
    }
}