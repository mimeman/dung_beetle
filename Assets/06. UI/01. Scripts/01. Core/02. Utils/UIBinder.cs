#region 설명
/* [설명]
 * 코드상의 변수와 Unity Hierarchy의 컴포넌트를 이름(Name) 기반으로 자동 연결해주는 정적(Static) 도구입니다.
 * [작동 원리]
 * 1. UIBase.Init() 시점에 호출됩니다.
 * 2. GetComponentsInChildren<T>(true)를 사용해 비활성화된 자식까지 모두 스캔합니다.
 * 3. 요청한 '이름'과 정확히 일치하는 오브젝트를 찾아 반환합니다.
 
 * [함수]
 * Bind<T> : (Target, Name)을 받아 해당 컴포넌트를 찾아 반환합니다. 실패 시 에러 로그를 출력합니다.
 * 
 * [주의사항]
 * 탐색 비용(Cost)이 발생하므로 반드시 '초기화(Init)' 단계에서만 사용해야 합니다. (Update 사용 금지)
 */
#endregion

using UnityEngine;

public static class UIBinder
{
    public static T Bind<T>(GameObject target, string name) where T : Component
    {
        T[] components = target.GetComponentsInChildren<T>(true);

        foreach (T component in components)
        {
            if (component.gameObject.name == name)
            {
                return component;
            }
        }
        Debug.LogError($"[UIBinder] 실패! '{target.name}' 안에서 '{name}'라는 {typeof(T).Name}를 못 찾음.");
        return null;
    }
}