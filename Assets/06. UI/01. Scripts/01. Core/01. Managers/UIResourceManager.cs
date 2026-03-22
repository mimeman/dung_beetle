using UnityEngine;

#region 설명
/* [설명]
 * UI 프리팹을 경로에서 찾아서 로드하고 생성해주는 역할입니다
 * 경로(Path) 관리와 생성(Instantiate) 책임을 UIManager로부터 가져왔습니다.
 *
 * [기능]
 *  Create<T>: 타입(T)의 이름과 똑같은 프리팹을 찾아서 생성해줍니다.
 *  경로 관리: "UI/Windows/" 경로를 한곳에서 관리
 */
#endregion

public class UIResourceManager
{
    // 프리팹들이 모여있는 경로
    private const string UI_PATH_ROOT = "UI/Windows/";

    // 1. 생성 함수
    public T Create<T>(Transform parent) where T : UIBase
    {
        // 타입 이름으로 이름 추출 (예: UI_Inventory -> "UI_Inventory")
        string prefabName = typeof(T).Name;

        // 리소스 로드
        GameObject prefab = Resources.Load<GameObject>($"{UI_PATH_ROOT}{prefabName}");

        if (prefab == null)
        {
            Debug.LogError($"[UIResource] 경로에서 프리팹을 못 찾았습니다: {UI_PATH_ROOT}{prefabName}");
            return null;
        }

        // 실제 생성
        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefabName; //글자 제거

        // 컴포넌트 리턴
        return go.GetComponent<T>();
    }
}