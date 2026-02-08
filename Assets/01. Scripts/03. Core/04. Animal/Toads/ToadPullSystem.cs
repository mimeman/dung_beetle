using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToadPullSystem : MonoBehaviour
{
    private ToadController toad;
    public Transform mouthPoint; // 입 위치

    public void Initialize(ToadController animal) => toad = animal;

    public bool PullTarget()
    {
        if (toad.Target == null) return true; // 예외 처리

        // 타겟을 입 쪽으로 이동
        toad.Target.position = Vector3.MoveTowards(toad.Target.position, mouthPoint.position, toad.ToadConfig.pullSpeed * Time.deltaTime);

        // 지속 데미지 (옵션)
        // _monster.Target.GetComponent<IDamageable>()?.TakeDamage(_monster.Data.pullDamagePerTick * Time.deltaTime);

        // 도착 확인
        if (Vector3.Distance(toad.Target.position, mouthPoint.position) < 1.0f)
        {
            return true; // 당기기 완료
        }
        return false;
    }
}