#region 설명
/* [설명]
 * 플레이어가 물체를 잡거나 미는 행위를 추상화함.
 * GameObject에 의존하지 않고 IGrabber 인터페이스를 통해 상호작용함 (DIP 준수)
 */
#endregion

using UnityEngine;

public interface IGrabbable
{
    bool IsGrabbed { get; }
    float DragWeight { get; }

    // 대상을 잡음
    /// <param name="grabber">잡는 주체 (IGrabber를 구현한 객체)</param>
    void Grab(IGrabber grabber);

    // 잡고 있는 대상을 놓는다
    void Release();
}