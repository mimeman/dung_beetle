using UnityEngine;

public class PlayerPhysicsHandler : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 공에 힘을 전달합니다 (수평 방향만)
    /// </summary>
    public void AddPushForceToDung(IDungInteractable dung, Vector3 direction, float power)
    {
        if (dung == null) return;

        Transform dungTransform = dung.GetTransform();
        var dungPhysics = dungTransform.GetComponent<DungPhysicsHandler>();

        if (dungPhysics != null)
        {
            // 수평 방향으로만 (Y축 제거)
            Vector3 pushDir = new Vector3(direction.x, 0, direction.z).normalized;

            dungPhysics.AddPushForce(pushDir, power);
        }
    }
}