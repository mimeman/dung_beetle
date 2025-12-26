using Dung.Enums;

public interface IAudioSurface
{
    // 현재 표면의 재질 타입
    // SoundManager가 이걸 보고 알맞은 소리를 찾아서 재생함
    SurfaceType SurfaceType { get; }
}