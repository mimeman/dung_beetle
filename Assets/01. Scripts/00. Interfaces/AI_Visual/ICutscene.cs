using System;

public interface ICutscene
{
    // 컷신을 재생합니다. (플레이어 조작 잠금, 카메라 전환)
    /// <param name="onFinished">컷신이 끝난 후 실행할 콜백 (예: 게임 시작)</param>
    void Play(Action onFinished = null);

    // 컷신을 즉시 건너뜁니다. (스킵 기능)
    void Skip();

    // 현재 컷신이 재생 중인지 확인.
    bool IsPlaying { get; }
}