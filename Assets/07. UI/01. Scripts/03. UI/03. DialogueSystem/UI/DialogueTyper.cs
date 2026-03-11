using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class DialogueTyper : MonoBehaviour
{
    private TextMeshProUGUI _targetText;
    private Coroutine _typingCo;

    public bool IsTyping { get; private set; } = false;

    public void Init(TextMeshProUGUI tmp)
    {
        _targetText = tmp;
    }

    public void StartTyping(string text, float speed, float fastSpeed, Action onComplete)
    {
        StopTyping();
        _typingCo = StartCoroutine(CoTypingProcess(text, speed, fastSpeed, onComplete));
    }

    public void StopTyping(string fullText = null)
    {
        if (_typingCo != null) StopCoroutine(_typingCo);
        IsTyping = false;

        if (fullText != null && _targetText != null)
        {
            _targetText.text = fullText;
        }
    }

    private IEnumerator CoTypingProcess(string originText, float speed, float fastSpeed, Action onComplete)
    {
        IsTyping = true;
        _targetText.text = "";

        int currentIdx = 0;

        while (currentIdx < originText.Length)
        {
            // 1. 현재 글자가 태그의 시작('<')인지 확인
            if (originText[currentIdx] == '<')
            {
                // 닫는 괄호 '>'의 위치를 찾음
                int closeIdx = originText.IndexOf('>', currentIdx);

                if (closeIdx != -1) // 닫는 괄호가 있다면 (정상 태그라면)
                {
                    // 태그 전체를 잘라냄 (예: <sprite name="happy">)
                    string tag = originText.Substring(currentIdx, closeIdx - currentIdx + 1);

                    // 태그를 한 번에 텍스트에 추가 (그래야 아이콘이 보임)
                    _targetText.text += tag;

                    // 인덱스를 태그 끝으로 점프
                    currentIdx = closeIdx + 1;

                    // 이렇게 해야 아이콘이 나올 때도 '타닥' 하는 박자가 생김
                    bool isFastTag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    yield return new WaitForSeconds(isFastTag ? fastSpeed : speed);

                    continue; // 다음 루프로 이동
                }
            }

            // 2. 일반 글자일 경우
            _targetText.text += originText[currentIdx];
            currentIdx++;

            // 3. 타이핑 딜레이 (애니메이션 효과)
            bool isFast = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            yield return new WaitForSeconds(isFast ? fastSpeed : speed);
        }

        IsTyping = false;
        onComplete?.Invoke();
    }
}