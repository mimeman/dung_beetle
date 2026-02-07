using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueProjectile : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed;
    private ToadTongueController controller;
    private bool hasHit = false;

    public void Launch(Vector3 targetPos, float speed, ToadTongueController controller)
    {
        this.targetPos = targetPos;
        this.speed = speed;
        this.controller = controller;
        Destroy(gameObject, 3.0f); // 안전장치: 3초 후 자동 소멸
    }

    private void Update()
    {
        if (hasHit) return;

        // 타겟 방향으로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            // 최대 사거리 도달 (충돌 없이) -> 빗나감 처리
            controller.OnHit(null);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // 몬스터 자신과의 충돌 무시 (Layer 설정으로도 가능)
        if (other.gameObject == controller.gameObject) return;

        hasHit = true;
        controller.OnHit(other);

        // 충돌 시 혀 오브젝트는 유지하거나 부모를 타겟으로 변경하는 등 연출 필요
        // 여기서는 간단히 컨트롤러에게 위임
    }
}