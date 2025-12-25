#region 설명
/* [설명]
 * 플레이어가 물체를 잡거나 미는 행위를 추상화하고 물리적 연결(Joint)을 제어함.
 * 예 : 쇠똥 굴리기(잡고 밀기/끌기), 돌덩이 옮기기, 퍼즐 레버 당기기.
 * PlayerLinker(플레이어의 손/입)가 호출하며, 구현부에서는 FixedJoint나 SpringJoint를 생성하여 연결함.
 
 * [함수]
 * Grab : 대상과 그랩 주체를 물리적으로 연결합니다.
 * Release : 현재 연결된 물리적 결속을 해제합니다.
 */
#endregion
using UnityEngine;

public interface IGrabbable
{
    bool IsGrabbed { get; } //중복 잡기 방지용

    //  대상을 잡아서 그랩 주체와 연결합니다. (Joint 생성 및 물리 연결)
    /// <param name="grabber">잡는 주체 (플레이어의 손, 입, 혹은 턱)</param>
    void Grab(GameObject grabber);

    // 잡고 있는 대상을 놓아줍니다. (Joint 파괴 및 물리 연결 해제)
    void Release();
}