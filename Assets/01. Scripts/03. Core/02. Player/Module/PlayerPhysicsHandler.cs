using UnityEngine;

public class PlayerPhysicsHandler : MonoBehaviour
{
    /// <summary>
    /// 플레이어의 입력을 바탕으로 쇠똥 공에 물리적인 힘을 가합니다.
    /// </summary>
    /// <param name="dung">상호작용 중인 쇠똥 인터페이스</param>
    /// <param name="direction">밀고자 하는 방향 (Input 기반)</param>
    /// <param name="power">플레이어의 기본 밀기 위력</param>
    public void AddPushForceToDung(IDungInteractable dung, Vector3 direction, float power)
    {
        if (dung == null) return;

        // 1. 인터페이스를 통해 쇠똥의 Transform을 가져와 물리 핸들러를 찾습니다.
        Transform dungTransform = dung.GetTransform();
        var dungPhysics = dungTransform.GetComponent<DungPhysicsHandler>();

        if (dungPhysics != null)
        {
            // 2. 플레이어의 이동 방향을 공이 굴러갈 수 있도록 수평 벡터로 가공합니다.
            Vector3 pushDir = new Vector3(direction.x, 0, direction.z).normalized;

            // 3. 공의 물리 핸들러에 실제 힘을 전달합니다.
            // 공 내부에서 질량(Mass)에 따른 가속도 보정이 이루어집니다.
            dungPhysics.AddPushForce(pushDir, power);
        }
    }
}