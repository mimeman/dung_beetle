using UnityEngine;

/// <summary>
/// 쇠똥구리 게임의 핵심인 '쇠똥(Dung Ball)'과의 상호작용을 정의하는 인터페이스
/// </summary>
public interface IDungInteractable
{
    // 1. 상호작용 시작/종료 신호 (밀기 등)
    void OnPushStart(GameObject user);
    void OnPushEnd(GameObject user);

    // 2. Final IK 연동을 위한 타겟 정보 제공
    // 튜플을 사용하여 왼손, 오른손 타겟을 한 번에 반환합니다.
    (Transform left, Transform right) GetIKTargets();

    // 3. 쇠똥의 현재 물리 정보 및 트랜스폼 반환
    Vector3 GetPosition();
    Transform GetTransform();

    // 4. 상호작용 가능 여부 확인 (이미 누군가 밀고 있는지 등)
    bool IsInteractable { get; }
}