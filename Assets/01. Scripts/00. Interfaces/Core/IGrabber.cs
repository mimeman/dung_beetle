using UnityEngine;

// 잡는 행위를 하는 주체 (플레이어, 함정)
public interface IGrabber
{
    // 물체를 잡을 위치 (손의 위치 등)
    Transform GrabPoint { get; }

    // 물리적 연결 처리 (Joint 연결 등)
    void Connect(IGrabbable target);

    // 연결 해제 처리
    void Disconnect();
}