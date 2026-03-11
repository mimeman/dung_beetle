using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 리스트 아이템들의 순차 애니메이션을 관리
/// Inspector에서 아이템들을 할당하면 Layout 계산 후 각 위치를 기억
/// </summary>
public class UI_TweenListController : MonoBehaviour
{
    [Header("═══ 리스트 아이템 ═══")]
    [Tooltip("애니메이션할 아이템들을 드래그해서 할당\n" +
             "각 아이템에는 UI_TweenAnimator가 있어야 함")]
    [SerializeField] private List<UI_TweenAnimator> _items = new List<UI_TweenAnimator>();

    [Space(10)]
    [Header("═══ 순차 등장 설정 ═══")]
    [Tooltip("각 아이템 간의 등장 간격 (초)\n" +
             "0.1 = 따-다-다 느낌\n" +
             "0.05 = 빠르게 따다닥")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _interval = 0.1f;

    private RectTransform _rect;
    private bool _isInit = false;

    private void Awake() => Init();

    public void Init()
    {
        if (_isInit) return;

        _rect = GetComponent<RectTransform>();
        _isInit = true;
    }

    /// <summary>
    /// 레이아웃 강제 갱신 후 각 아이템의 originPos 캐싱
    /// </summary>
    private void RefreshLayoutAndCachePositions()
    {
        Canvas.ForceUpdateCanvases();

        if (_rect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

        foreach (var item in _items)
        {
            if (item != null && item.gameObject.activeSelf)
            {
                item.RefreshOriginPos();
            }
        }
    }

    #region ═══ Show ═══

    /// <summary>
    /// 모든 아이템 순차 Show
    /// </summary>
    public void PlayShowItems()
    {
        Init();
        RefreshLayoutAndCachePositions();

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null && _items[i].gameObject.activeSelf)
            {
                float delay = i * _interval;
                _items[i].PlayShowDelayed(delay);
            }
        }
    }

    /// <summary>
    /// 활성화된 아이템만 순차 Show (동적 리스트용)
    /// </summary>
    public void PlayShowActiveItems()
    {
        Init();
        RefreshLayoutAndCachePositions();

        int activeIndex = 0;
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null && _items[i].gameObject.activeSelf)
            {
                float delay = activeIndex * _interval;
                _items[i].PlayShowDelayed(delay);
                activeIndex++;
            }
        }
    }

    #endregion

    #region ═══ Hide ═══

    /// <summary>
    /// 모든 아이템 순차 Hide
    /// </summary>
    public void PlayHideItems(System.Action onAllComplete = null)
    {
        Init();

        int activeCount = 0;
        foreach (var item in _items)
        {
            if (item != null && item.gameObject.activeSelf)
                activeCount++;
        }

        if (activeCount == 0)
        {
            onAllComplete?.Invoke();
            return;
        }

        int completedCount = 0;
        int index = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null && _items[i].gameObject.activeSelf)
            {
                float delay = index * _interval;
                _items[i].PlayHideDelayed(delay, () =>
                {
                    completedCount++;
                    if (completedCount >= activeCount)
                        onAllComplete?.Invoke();
                });
                index++;
            }
        }
    }

    #endregion

    #region ═══ Utility ═══

    /// <summary>
    /// 모든 아이템 원래 위치로 리셋
    /// </summary>
    public void ResetAllToOrigin()
    {
        Init();

        foreach (var item in _items)
        {
            if (item != null)
            {
                item.ResetToOrigin();
            }
        }
    }

    /// <summary>
    /// 특정 인덱스 아이템 활성화/비활성화
    /// </summary>
    public void SetItemActive(int index, bool active)
    {
        if (index >= 0 && index < _items.Count && _items[index] != null)
        {
            _items[index].gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 특정 개수만큼 아이템 활성화 (나머지는 비활성화)
    /// </summary>
    public void SetActiveCount(int count)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
            {
                _items[i].gameObject.SetActive(i < count);
            }
        }
    }

    public List<UI_TweenAnimator> Items => _items;
    public int ItemCount => _items.Count;

    public float Interval
    {
        get => _interval;
        set => _interval = value;
    }

    #endregion

    #region ═══ Editor ═══
#if UNITY_EDITOR
    [ContextMenu("◆ 자식 자동 수집")]
    private void AutoCollectChildren()
    {
        _items.Clear();
        foreach (Transform child in transform)
        {
            var anim = child.GetComponent<UI_TweenAnimator>();
            if (anim != null)
                _items.Add(anim);
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[UI_TweenListController] {_items.Count}개 아이템 수집 완료");
    }

    [ContextMenu("▶ Test Show All")]
    private void TestShowAll()
    {
        if (!Application.isPlaying) return;
        SetActiveCount(_items.Count);
        PlayShowActiveItems();
    }

    [ContextMenu("▶ Test Hide All")]
    private void TestHideAll()
    {
        if (!Application.isPlaying) return;
        PlayHideItems();
    }
#endif
    #endregion
}