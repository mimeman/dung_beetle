using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TongueProjectile : MonoBehaviour
{
    [SerializeField] private float maxDistance = 8f;

    private Vector3 moveDirection;
    private Vector3 startPos;
    private float speed;
    private ToadTongueController controller;
    private bool hasHit = false;

    /// <summary>
    /// 히트된 대상의 Transform (LineRenderer 추적용)
    /// </summary>
    public Transform HitTarget { get; private set; }

    public void Launch(Vector3 targetPos, float speed, ToadTongueController controller)
    {
        this.speed = speed;
        this.controller = controller;

        startPos = transform.position;
        moveDirection = (targetPos + Vector3.up * 0.1f - startPos).normalized;

        Destroy(gameObject, 3.0f); // 안전장치: 3초 후 자동 소멸
    }

    private void Update()
    {
        if (hasHit) return;

        // 타겟 방향으로 이동
        transform.position += moveDirection * Time.deltaTime * speed;

        float distnace = Vector3.Distance(transform.position, startPos);
        if (distnace >= maxDistance)
        {
            // 최대 사거리 도달 (충돌 없이) -> 빗나감 처리
            Debug.Log($"{distnace} 최대 사거리 도달");
            controller.OnHit(null);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || other.gameObject == controller.gameObject || other.tag != "Player")
        {
            controller.OnHit(null);
            Destroy(gameObject);
            return;
        }

        if (other.tag == "Player")
        {
            Debug.Log($"{other.name} hit");

            hasHit = true;
            HitTarget = other.transform;

            // 혀 끝을 대상에 부착 → 당기기 중 LineRenderer가 자연스럽게 따라감
            transform.SetParent(other.transform);

            controller.OnHit(other);
        }
    }
}