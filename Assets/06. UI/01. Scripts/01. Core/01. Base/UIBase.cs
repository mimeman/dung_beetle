#region 설명
/* [설명]
 * 모든 UI 팝업(Window, Popup, Panel)들이 상속받아야 하는 '최상위 부모 클래스'입니다.
 * UI의 공통적인 생명주기(Init -> Open -> Close)를 표준화하여 관리합니다.
 * 복잡한 바인딩 도구(UIBinder)를 내부적으로 감싸서(Wrapping), 자식 클래스들이 쉽게 부품을 찾을 수 있게 돕습니다.
 
 *  UIInputLogic(입력 제어) 컴포넌트가 옆에 붙어있으면 자동으로 감지합니다.
 *  Open() 시 InputLogic을 켜고, Close() 시 끕니다. (자동 스위치)
 
 * [함수]
 * Init   : 초기화 함수. 바인딩 로직(GetUI)은 반드시 여기서 수행해야 합니다. (중복 실행 방지됨)
 * Open   : UI를 켭니다. 만약 초기화가 안 되어 있다면 자동으로 Init을 먼저 수행합니다.
 * Close  : UI를 끕니다. (SetActive false)
 * GetUI  : (protected) 자식들이 이름만으로 컴포넌트를 찾을 수 있게 해주는 도우미 함수.
 */
#endregion

using UnityEngine;

public class UIBase : MonoBehaviour
{
    // 초기화를 한 번만 수행하기 위한 체크 변수
    protected bool _isInit = false;

    // 팝업 여부 판단 (기본은 팝업)
    public virtual bool IsPopup => true;

    protected UIInputLogic<UIBase> _inputLogic;

    // 1. 초기화
    public virtual void Init()
    {
        if (_isInit) return;
        _isInit = true;

        _inputLogic = GetComponent<UIInputLogic<UIBase>>();

        if (_inputLogic != null)
        {
            _inputLogic.Init(this);
            _inputLogic.enabled = false;
        }
    }

    // 2. 켜기
    public virtual void Open()
    {
        if (!_isInit) Init();
        gameObject.SetActive(true);

        if (_inputLogic != null)
            _inputLogic.enabled = true;
    }

    // 3. 끄기
    public virtual void Close()
    {
        gameObject.SetActive(false);

        if (_inputLogic != null)
            _inputLogic.enabled = false;
    }

    // 도우미
    // 자식 클래스는 UIBinder를 몰라도 됨. GetUI<T>("이름") 이것만 쓰면 됨
    protected T GetUI<T>(string name) where T : Component
    {
        return UIBinder.Bind<T>(this.gameObject, name);
    }
}