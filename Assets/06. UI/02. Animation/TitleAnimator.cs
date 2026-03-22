using UnityEngine;
using DG.Tweening;
using Unity.Entities; // DOTween 네임스페이스 추가

public class TitleAnimator : MonoBehaviour
{
    [SerializeField] private bool updownAnim = false;
    [SerializeField] private bool swingAnim = false;
    [Space(10)]
    [SerializeField] private bool heartBeat = false;
    [Header("Animation Settings")]
    [SerializeField] private float animDelta = 2.5f;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (updownAnim)
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 15f, animDelta)
                .SetLoops(-1, LoopType.Yoyo) // -1은 무한반복, Yoyo는 왔다갔다 반복
                .SetEase(Ease.InOutSine);    // 부드럽게 감속/가속

        if (heartBeat)
            rectTransform.DOScale(1.05f, animDelta)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

        if (swingAnim)
            rectTransform.DORotate(new Vector3(0f, 0f, 3f), animDelta * 2)
                .SetLoops(-1, LoopType.Yoyo) // 왔다 갔다 무한반복
                .SetEase(Ease.InOutSine)    // 부드럽게
                .From(new Vector3(0f, 0f, -3f)); // 왼쪽 8도부터 시작하도록 설정
    }
}