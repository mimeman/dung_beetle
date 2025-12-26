public interface IDebuggable
{
    // 현재 객체의 상태 정보를 문자열로 반환합니다.
    // 예: "State: Rolling, Mass: 50.4kg, Target: Player"
    string GetDebugInfo();

    // 씬 뷰(Scene View)에 기즈모(선, 구 등)를 그릴지 토글합니다.
    // true면 AI 인식 범위 같은 것이 초록색 선으로 보임.
    void ToggleGizmos(bool enable);
}