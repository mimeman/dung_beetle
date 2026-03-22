#region 설명
/* [설명]
 * 게임 내 모든 UI의 흐름을 관리하는 싱글톤 클래스입니다.
 * 직접 리소스를 로드하지 않고, 'UIResourceManager'에게 시킵니다.
 * '스택(Stack)' 구조를 사용하여 UI가 열린 순서를 기억하고, ESC 키 처리 시 가장 위의 창부터 닫습니다.

 * 1. 문자열("UI_Inventory") 대신 제네릭<T>만으로 UI를 엽니다. (오타 방지)
 * 2. ESC를 누르면 가장 최근에 열린 UI부터 순서대로 닫힙니다.
 */
#endregion

using UnityEngine;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private UIResourceManager _resourceManager;

    // 데이터 (캐시 & 스택)
    // 캐시: 한 번 만든 건 보관 (Key: "UI_Inventory")
    private Dictionary<string, UIBase> _uiCache = new Dictionary<string, UIBase>();

    // 스택: 열려있는 UI 목록
    private List<UIBase> _popupStack = new List<UIBase>();

    // 캔버스 위치
    private Transform _canvasRoot;

    // 외부 참조용 (팝업 열려있으면 게임 멈춤) 임시
    public bool IsPopupOpen => _popupStack.Count > 0;
    public static event System.Action<bool> OnPopupStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _resourceManager = new UIResourceManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _popupStack.Count > 0)
        {
            ClosePopupTop();
        }
    }

    private void EnsureCanvas()
    {
        if (_canvasRoot == null)
        {
            var canvas = GameObject.Find("Canvas_Main");
            if (canvas != null) _canvasRoot = canvas.transform;
            else Debug.LogError("[UIManager] Canvas_Main이 없습니다!");
        }
    }

    public T Show<T>() where T : UIBase
    {
        EnsureCanvas();
        string uiName = typeof(T).Name; // T에서 이름 자동 추출 ("UI_Inventory")

        UIBase ui = null;

        // 1. 캐시 확인
        if (_uiCache.TryGetValue(uiName, out UIBase existingUI))
        {
            ui = existingUI;
        }
        else
        {
            ui = _resourceManager.Create<T>(_canvasRoot);
            if (ui == null) return null;

            ui.Init(); // 바인딩
            _uiCache.Add(uiName, ui); // 캐시에 등록
        }

        // 3. 스택 관리 (이미 열려있으면 스택에서 빼서 맨 뒤로 보냄)
        if (ui.IsPopup)
        {
            if (_popupStack.Contains(ui))
                _popupStack.Remove(ui);

            _popupStack.Add(ui);     // 스택 최상단에 등록
            ui.transform.SetAsLastSibling(); // 화면상에서도 맨 위로
            
            if (_popupStack.Count == 1)
            {
                SetCursorState(true);
                OnPopupStateChanged?.Invoke(true);
            }
        }
        else
        {
            ui.transform.SetAsLastSibling();
        }

        // 4. 진짜 열기
        ui.Open();

        // (추후 GameStateManager 연동: Pause 상태로 변경 고려)
        if (ui.IsPopup)
            Debug.Log($"[UIManager] {uiName} 열림 (현재 스택: {_popupStack.Count})");

        return (T)ui;
    }

    // 특정 UI 닫기 (X 버튼용)
    public void Hide<T>() where T : UIBase
    {
        string uiName = typeof(T).Name;

        if (_uiCache.TryGetValue(uiName, out UIBase ui))
        {
            if (ui.IsPopup) ClosePopup(ui);
            else ui.Close();
        }
    }


    // 내부적으로 닫는 로직
    private void ClosePopup(UIBase ui)
    {
        ui.Close();
        _popupStack.Remove(ui);

        if (_popupStack.Count == 0)
        {
            SetCursorState(false);
            OnPopupStateChanged?.Invoke(false);
            Debug.Log("[UIManager] 모든 팝업 닫힘 -> 게임 재개");
        }
    }

    // 가장 위에 있는 팝업 닫기 (ESC용)
    public void ClosePopupTop()
    {
        if (_popupStack.Count == 0) return;

        // 리스트의 마지막 요소가 '가장 위에 있는 UI'
        int topIndex = _popupStack.Count - 1;
        UIBase topUI = _popupStack[topIndex];

        ClosePopup(topUI);
    }

    // 마우스 커서 상태 조절 함수
    public void SetCursorState(bool isUI)
    {
        if (isUI)
        {
            // UI 모드: 커서 보이고, 가두지 않음 (클릭 가능)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 게임 모드: 커서 숨기고, 화면 중앙에 고정
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ShowInteractPrompt(string text)
    {
        var prompt = Show<UI_InteractPrompt>();
        prompt?.SetPrompt(text);
    }

    public void HideInteractPrompt()
    {
        Hide<UI_InteractPrompt>();
    }
}