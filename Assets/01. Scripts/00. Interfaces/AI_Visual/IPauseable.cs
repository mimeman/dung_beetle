public interface IPauseable
{
    // 게임이 일시정지되었을 때 호출됩니다.
    // (AI 생각 멈춤, 애니메이션 속도 0, 소리 음소거 등)
    void OnPause();

    // 일시정지가 해제되었을 때 호출됩니다.
    // (멈췄던 동작 재개)
    void OnResume();
}