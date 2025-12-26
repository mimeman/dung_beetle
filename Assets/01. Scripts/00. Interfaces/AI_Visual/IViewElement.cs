public interface IViewElement
{
    // UI 패널을 화면에 표시합니다.
    void Show();

    // UI 패널을 숨깁니다.
    void Hide();

    // UI 데이터를 최신 상태로 갱신합니다.
    void Refresh();

    // 현재 열려있는지 확인.
    bool IsVisible { get; }
}