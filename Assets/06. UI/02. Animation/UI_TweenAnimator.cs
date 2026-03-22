using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI 애니메이션 타입
/// </summary>
public enum UIAnimType
{
    None,       // 애니메이션 없음
    Fade,       // 투명도만 변화
    ScalePop,   // 크기 0 → 1 (팝업 느낌)
    SlideUp,    // 아래에서 위로 등장
    SlideDown,  // 위에서 아래로 등장
    SlideLeft,  // 오른쪽에서 왼쪽으로 등장
    SlideRight  // 왼쪽에서 오른쪽으로 등장
}

/// <summary>
/// UI 강조 효과 타입
/// </summary>
public enum UIEmphasisType
{
    None,           // 효과 없음
    PunchScale,     // 크기 펀치 (띠용)
    ShakePosition   // 위치 흔들림
}

/// <summary>
/// 자주 쓰는 Ease 타입만 모음 (DOTween 원본은 37개라 불편)
/// </summary>
public enum UIEaseType
{
    None,           // Ease 없음 (Linear와 동일)
    Linear,         // 일정한 속도
    InQuad,         // 천천히 시작
    OutQuad,        // 천천히 끝남
    InOutQuad,      // 양쪽 천천히
    OutBack,        // 끝에서 살짝 튕김 (추천: Show)
    InBack,         // 시작에서 살짝 당김 (추천: Hide)
    OutElastic,     // 탄성 효과 (띠용띠용)
    OutBounce       // 바운스 효과 (통통)
}

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class UI_TweenAnimator : MonoBehaviour
{
    [Header("═══ 등장 애니메이션 ═══")]
    [Tooltip("UI가 나타날 때 어떻게 등장할지 선택\n" +
             "• Fade: 투명 → 불투명\n" +
             "• ScalePop: 작게 → 크게 (팝업)\n" +
             "• SlideUp/Down/Left/Right: 방향에서 슬라이드")]
    [SerializeField] private UIAnimType _showType = UIAnimType.ScalePop;

    [Tooltip("등장 애니메이션의 움직임 느낌\n" +
             "• OutBack: 끝에서 살짝 튕김 (추천)\n" +
             "• OutElastic: 탄성 효과\n" +
             "• Linear: 일정한 속도")]
    [SerializeField] private UIEaseType _easeShow = UIEaseType.OutBack;

    [Space(10)]
    [Header("═══ 퇴장 애니메이션 ═══")]
    [Tooltip("UI가 사라질 때 어떻게 퇴장할지 선택")]
    [SerializeField] private UIAnimType _hideType = UIAnimType.Fade;

    [Tooltip("퇴장 애니메이션의 움직임 느낌\n" +
             "• InBack: 시작에서 살짝 당김 (추천)\n" +
             "• Linear: 일정한 속도")]
    [SerializeField] private UIEaseType _easeHide = UIEaseType.InBack;

    [Space(10)]
    [Header("═══ 세부 설정 ═══")]
    [Tooltip("애니메이션 재생 시간 (초)\n보통 0.2 ~ 0.5 사이가 적당")]
    [Range(0.1f, 2f)]
    [SerializeField] private float _duration = 0.3f;

    [Tooltip("슬라이드 이동 거리 (픽셀)\nSlide 타입에서만 사용됨")]
    [Range(10f, 500f)]
    [SerializeField] private float _moveDistance = 100f;

    // 내부 변수
    private CanvasGroup _cg;
    private RectTransform _rect;
    private Vector2 _originPos;
    private Vector3 _originScale;
    private bool _isInit = false;

    private void Awake() => Init();

    public void Init()
    {
        if (_isInit) return;

        _cg = GetComponent<CanvasGroup>();
        _rect = GetComponent<RectTransform>();
        _originScale = transform.localScale;
        _isInit = true;
    }

    /// <summary>
    /// 현재 위치를 originPos로 갱신 (Layout 계산 후 호출)
    /// </summary>
    public void RefreshOriginPos()
    {
        Init();
        _originPos = _rect.anchoredPosition;
    }

    #region ═══ Show ═══

    public void PlayShow()
    {
        Init();
        gameObject.SetActive(true);
        KillAllTweens();

        _cg.alpha = (_showType == UIAnimType.None) ? 1f : 0f;
        _rect.anchoredPosition = _originPos;
        transform.localScale = _originScale;

        Ease ease = ConvertEase(_easeShow);
        Sequence seq = DOTween.Sequence();

        switch (_showType)
        {
            case UIAnimType.None:
                _cg.alpha = 1f;
                break;

            case UIAnimType.Fade:
                seq.Append(_cg.DOFade(1f, _duration).SetEase(ease));
                break;

            case UIAnimType.ScalePop:
                transform.localScale = Vector3.zero;
                _cg.alpha = 1f;
                seq.Append(transform.DOScale(_originScale, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideUp:
                _rect.anchoredPosition = _originPos + Vector2.down * _moveDistance;
                seq.Append(_cg.DOFade(1f, _duration * 0.5f));
                seq.Join(_rect.DOAnchorPos(_originPos, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideDown:
                _rect.anchoredPosition = _originPos + Vector2.up * _moveDistance;
                seq.Append(_cg.DOFade(1f, _duration * 0.5f));
                seq.Join(_rect.DOAnchorPos(_originPos, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideLeft:
                _rect.anchoredPosition = _originPos + Vector2.right * _moveDistance;
                seq.Append(_cg.DOFade(1f, _duration * 0.5f));
                seq.Join(_rect.DOAnchorPos(_originPos, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideRight:
                _rect.anchoredPosition = _originPos + Vector2.left * _moveDistance;
                seq.Append(_cg.DOFade(1f, _duration * 0.5f));
                seq.Join(_rect.DOAnchorPos(_originPos, _duration).SetEase(ease));
                break;
        }
    }

    public void PlayShowDelayed(float delay)
    {
        Init();
        gameObject.SetActive(true);

        if (delay <= 0f)
        {
            PlayShow();
            return;
        }

        SetInitialHiddenState();
        DOVirtual.DelayedCall(delay, PlayShow);
    }

    private void SetInitialHiddenState()
    {
        KillAllTweens();
        _cg.alpha = 0f;

        switch (_showType)
        {
            case UIAnimType.ScalePop:
                _rect.anchoredPosition = _originPos;
                transform.localScale = Vector3.zero;
                break;
            case UIAnimType.SlideUp:
                _rect.anchoredPosition = _originPos + Vector2.down * _moveDistance;
                transform.localScale = _originScale;
                break;
            case UIAnimType.SlideDown:
                _rect.anchoredPosition = _originPos + Vector2.up * _moveDistance;
                transform.localScale = _originScale;
                break;
            case UIAnimType.SlideLeft:
                _rect.anchoredPosition = _originPos + Vector2.right * _moveDistance;
                transform.localScale = _originScale;
                break;
            case UIAnimType.SlideRight:
                _rect.anchoredPosition = _originPos + Vector2.left * _moveDistance;
                transform.localScale = _originScale;
                break;
            default:
                _rect.anchoredPosition = _originPos;
                transform.localScale = _originScale;
                break;
        }
    }

    #endregion

    #region ═══ Hide ═══

    public void PlayHide(System.Action onComplete = null)
    {
        Init();
        KillAllTweens();

        Ease ease = ConvertEase(_easeHide);
        Sequence seq = DOTween.Sequence();

        switch (_hideType)
        {
            case UIAnimType.None:
                seq.AppendCallback(() => _cg.alpha = 0f);
                break;

            case UIAnimType.Fade:
                seq.Append(_cg.DOFade(0f, _duration).SetEase(ease));
                break;

            case UIAnimType.ScalePop:
                seq.Append(transform.DOScale(Vector3.zero, _duration).SetEase(ease));
                seq.Join(_cg.DOFade(0f, _duration));
                break;

            case UIAnimType.SlideUp:
                seq.Append(_cg.DOFade(0f, _duration * 0.8f));
                seq.Join(_rect.DOAnchorPos(_originPos + Vector2.up * _moveDistance, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideDown:
                seq.Append(_cg.DOFade(0f, _duration * 0.8f));
                seq.Join(_rect.DOAnchorPos(_originPos + Vector2.down * _moveDistance, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideLeft:
                seq.Append(_cg.DOFade(0f, _duration * 0.8f));
                seq.Join(_rect.DOAnchorPos(_originPos + Vector2.left * _moveDistance, _duration).SetEase(ease));
                break;

            case UIAnimType.SlideRight:
                seq.Append(_cg.DOFade(0f, _duration * 0.8f));
                seq.Join(_rect.DOAnchorPos(_originPos + Vector2.right * _moveDistance, _duration).SetEase(ease));
                break;

            default:
                seq.Append(_cg.DOFade(0f, _duration).SetEase(ease));
                break;
        }

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void PlayHideDelayed(float delay, System.Action onComplete = null)
    {
        Init();

        if (delay <= 0f)
        {
            PlayHide(onComplete);
            return;
        }

        DOVirtual.DelayedCall(delay, () => PlayHide(onComplete));
    }

    #endregion

    #region ═══ Emphasis ═══

    public void PlayEmphasis(UIEmphasisType type)
    {
        Init();
        transform.DOKill(true);
        transform.localScale = _originScale;

        switch (type)
        {
            case UIEmphasisType.PunchScale:
                transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1f);
                break;

            case UIEmphasisType.ShakePosition:
                _rect.DOShakeAnchorPos(0.4f, 10f, 20, 90f, false, true);
                break;
        }
    }

    #endregion

    #region ═══ Utility ═══

    private void KillAllTweens()
    {
        transform.DOKill();
        _cg?.DOKill();
        _rect?.DOKill();
    }

    public void Stop() => KillAllTweens();

    public void ResetToOrigin()
    {
        Init();
        Stop();

        _rect.anchoredPosition = _originPos;
        transform.localScale = _originScale;
        _cg.alpha = 1f;
    }

    /// <summary>
    /// UIEaseType → DOTween Ease 변환
    /// </summary>
    private Ease ConvertEase(UIEaseType type)
    {
        return type switch
        {
            UIEaseType.None => Ease.Linear,
            UIEaseType.Linear => Ease.Linear,
            UIEaseType.InQuad => Ease.InQuad,
            UIEaseType.OutQuad => Ease.OutQuad,
            UIEaseType.InOutQuad => Ease.InOutQuad,
            UIEaseType.OutBack => Ease.OutBack,
            UIEaseType.InBack => Ease.InBack,
            UIEaseType.OutElastic => Ease.OutElastic,
            UIEaseType.OutBounce => Ease.OutBounce,
            _ => Ease.Linear
        };
    }

    public float Duration => _duration;
    public Vector2 OriginPos => _originPos;

    #endregion

    #region ═══ Editor Test ═══
#if UNITY_EDITOR
    [ContextMenu("▶ Test Show")]
    private void TestShow()
    {
        if (!Application.isPlaying) return;
        RefreshOriginPos();
        PlayShow();
    }

    [ContextMenu("▶ Test Hide")]
    private void TestHide()
    {
        if (!Application.isPlaying) return;
        PlayHide();
    }

    [ContextMenu("▶ Test Punch")]
    private void TestPunch()
    {
        if (!Application.isPlaying) return;
        PlayEmphasis(UIEmphasisType.PunchScale);
    }
#endif
    #endregion
}