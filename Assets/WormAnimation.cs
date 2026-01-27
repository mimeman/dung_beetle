using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WormAnimation : MonoBehaviour
{
    [SerializeField] private Transform model;
    [Header("꼬물거림 설정 (Wiggle)")]
    public float wiggleSpeed = 5.0f; // 꼬물거리는 속도 (얼마나 빨리 흔들지)
    public float wiggleAngle = 30.0f; // 좌우 회전 각도 (얼마나 크게 흔들지)

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent)
        {
            if (agent.velocity.magnitude > 0.1f)
            {
                // 2. 좌우로 흔들기 (Sin 함수를 이용해 회전값을 계속 변화시킴)
                float wiggle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAngle;

                // 현재 각도에서 Y축(좌우)만 wiggle 값으로 설정
                // (지렁이가 바라보는 방향을 기준으로 흔듭니다)
                model.localRotation = Quaternion.Euler(0, wiggle, 0);
            }
            else
                model.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
