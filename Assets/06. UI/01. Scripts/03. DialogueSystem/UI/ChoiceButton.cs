using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("UI Components")]
    [SerializeField] private Button _btn;
    [SerializeField] private Image _btnImage;
    [SerializeField] private TextMeshProUGUI _txt;

    [Header("Sprite Settings (이미지 연결)")]
    [SerializeField] private Sprite _normalSprite; // 평소 (검은색: Choice_Not selected)
    [SerializeField] private Sprite _hoverSprite;  // 마우스 올림 (흰색: Choice_Select)
    [SerializeField] private Sprite _clickSprite;  // 클릭 (회색: Button_Click)

    [Header("Color Settings (글자 색상)")]
    [SerializeField] private Color _normalTextColor = Color.white; // 평소 글자 (흰색)
    [SerializeField] private Color _hoverTextColor = Color.black;  // 올렸을 때/클릭 글자 (검은색)

    private System.Action<BranchData> _onClickCallback;
    private BranchData _currentBranch;
    private bool _isClicked = false;

    public void Setup(BranchData data, System.Action<BranchData> onClick)
    {
        _currentBranch = data;
        _onClickCallback = onClick;
        _txt.text = data.Button_Text;

        // 초기화
        _isClicked = false;
        _btn.interactable = true;

        // 시작은 '평상시' 상태로
        SetVisual(ButtonState.Normal);

        _btn.onClick.RemoveAllListeners();
    }

    // --- 이벤트 감지 ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isClicked) return;
        SetVisual(ButtonState.Hover); // 마우스 올리면 흰배경/검은글씨
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isClicked) return;
        SetVisual(ButtonState.Normal); // 나가면 검은배경/흰글씨
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isClicked) return;
        StartCoroutine(CoClickProcess());
    }

    // --- 내부 로직 ---

    private enum ButtonState { Normal, Hover, Click }

    private void SetVisual(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Normal:
                if (_normalSprite) _btnImage.sprite = _normalSprite;
                _txt.color = _normalTextColor;
                break;

            case ButtonState.Hover:
                if (_hoverSprite) _btnImage.sprite = _hoverSprite;
                _txt.color = _hoverTextColor;
                break;

            case ButtonState.Click:
                if (_clickSprite) _btnImage.sprite = _clickSprite;
                _txt.color = _hoverTextColor; // 클릭 시에도 검은 글씨 유지
                break;
        }
    }

    private IEnumerator CoClickProcess()
    {
        _isClicked = true;
        _btn.interactable = false;

        // 클릭 연출 (회색 배경 + 검은 글씨)
        SetVisual(ButtonState.Click);

        yield return new WaitForSeconds(0.15f); // 0.15초 대기

        _onClickCallback?.Invoke(_currentBranch);
    }
}