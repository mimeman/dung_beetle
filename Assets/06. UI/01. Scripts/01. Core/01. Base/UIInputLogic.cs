#region 설명
/* [설명]
 * 개별 UI 팝업들의 입력 로직을 담당하는 부모 클래스입니다.

 * 역할: 이제 순수하게 '자기 자신만의 단축키(I, K 등)'만 처리하면 됩니다.
 */
#endregion

using UnityEngine;

// T는 UIBase를 상속받은 클래스여야 함을 명시 (Constraint)
public class UIInputLogic<T> : MonoBehaviour where T : UIBase
{
    protected T _view;

    public virtual void Init(T view)
    {
        _view = view;
        enabled = false; // 평소엔 꺼둠
    }

    // 자식들이 override해서 사용
    protected virtual void Update()
    {

    }
}