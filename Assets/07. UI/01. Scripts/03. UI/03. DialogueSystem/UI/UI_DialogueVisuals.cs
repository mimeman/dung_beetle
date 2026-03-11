using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class UI_DialogueVisuals : MonoBehaviour
{
    [Header("Main Window")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private RectTransform _mainPanel;

    [Header("Text Typing")]
    [SerializeField] private TextMeshProUGUI _textContent;
    private Tween _typeTween;

    [Header("Choices")]
    [SerializeField] private CanvasGroup _choiceGroup;
    [SerializeField] private RectTransform _choicePanel;

    private void Awake()
    {
        if (_mainCanvasGroup)
        {
            _mainCanvasGroup.alpha = 0f;
            _mainCanvasGroup.blocksRaycasts = false;
        }

        if (_choiceGroup)
        {
            _choiceGroup.alpha = 0f;
            _choiceGroup.blocksRaycasts = false;
            _choicePanel.gameObject.SetActive(false);
        }
    }

    public void AnimateWindowOpen()
    {
        _mainCanvasGroup.blocksRaycasts = true;
        _mainCanvasGroup.alpha = 0f;
        _mainPanel.localScale = Vector3.one * 0.9f;

        Sequence seq = DOTween.Sequence();
        seq.Append(_mainCanvasGroup.DOFade(1f, 0.3f));
        seq.Join(_mainPanel.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
    }

    public void AnimateWindowClose(System.Action onComplete)
    {
        _mainCanvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(_mainCanvasGroup.DOFade(0f, 0.2f));
        seq.Join(_mainPanel.DOScale(0.95f, 0.2f));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void AnimateTextTyping(string text, float speed, System.Action onComplete)
    {
        KillTypeTween();
        _textContent.text = ""; // 1. 텍스트 비우기

        float duration = text.Length * speed;

        // 원리: 빈 문자열("")에서 목표 문자열(text)로 서서히 바꿈
        _typeTween = DOTween.To(
            () => _textContent.text,    // Getter: 현재 텍스트 가져오기
            x => _textContent.text = x, // Setter: 텍스트 갱신하기
            text,                       // Target: 최종 텍스트
            duration
        )
        .SetEase(Ease.Linear) // 중요: 등속도로 타이핑
        .OnComplete(() =>
        {
            _typeTween = null;
            onComplete?.Invoke();
        });
    }

    public void SkipTyping(string fullText)
    {
        if (IsTyping)
        {
            KillTypeTween();
            _textContent.text = fullText;
        }
    }

    private void KillTypeTween()
    {
        if (_typeTween != null && _typeTween.IsActive())
        {
            _typeTween.Kill();
            _typeTween = null;
        }
    }

    public bool IsTyping => _typeTween != null && _typeTween.IsActive();

    public void AnimateChoicesShow(List<CanvasGroup> activeButtons)
    {
        _choicePanel.gameObject.SetActive(true);
        _choiceGroup.blocksRaycasts = true;
        _choiceGroup.alpha = 1f;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < activeButtons.Count; i++)
        {
            var btn = activeButtons[i];
            btn.alpha = 0f;
            btn.transform.localPosition += Vector3.down * 30f;

            float startTime = i * 0.1f;
            seq.Insert(startTime, btn.DOFade(1f, 0.3f));
            seq.Insert(startTime, btn.transform.DOLocalMoveY(btn.transform.localPosition.y + 30f, 0.3f).SetEase(Ease.OutQuad));
        }
    }

    public void AnimateChoicesHide()
    {
        _choiceGroup.blocksRaycasts = false;
        _choiceGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            _choicePanel.gameObject.SetActive(false);
        });
    }
}