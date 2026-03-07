using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueProjectile : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed;
    private ToadTongueController controller;
    private bool hasHit = false;

    /// <summary>
    /// 히트된 대상의 Transform (LineRenderer 추적용)
    /// </summary>
    public Transform HitTarget { get; private set; }

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

        float distnace = Vector3.Distance(transform.position, targetPos);
        if (distnace < 0.01f)
        {
            // 최대 사거리 도달 (충돌 없이) -> 빗나감 처리
            Debug.Log($"{distnace} 최대 사거리 도달");
            controller.OnHit(null);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        Debug.Log($"{other.name} hit");

        // 몬스터 자신과의 충돌 무시 (Layer 설정으로도 가능)
        if (other.gameObject == controller.gameObject) return;

        hasHit = true;
        HitTarget = other.transform;

        // 혀 끝을 대상에 부착 → 당기기 중 LineRenderer가 자연스럽게 따라감
        transform.SetParent(other.transform);

        controller.OnHit(other);
    }
}