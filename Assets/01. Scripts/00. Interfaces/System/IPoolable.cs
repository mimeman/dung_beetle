using UnityEngine;

#region 설명
/* [설명]
 * Instantiate(생성)와 Destroy(파괴)의 성능 비용을 줄이기 위한 '오브젝트 풀링' 규약.
 * 오브젝트를 실제로 파괴하지 않고 비활성화했다가, 필요할 때 다시 초기화해서 재사용함.
 
 * [작동 흐름]
 * 1. OnSpawn : 풀에서 꺼내질 때 호출 -> 체력, 위치, 속도 초기화 (Start 대용).
 * 2. OnDespawn : 풀로 돌아갈 때 호출 -> 이펙트 끄기, 물리력 초기화 (OnDestroy 대용).
 */
#endregion

public interface IPoolable
{
    // 풀에서 꺼내져서 활성화될 때 호출됩니다. (초기화 로직)
    void OnSpawn();

    // 풀로 돌아가기(비활성화) 직전에 호출됩니다. (정리 로직)
    void OnDespawn();
}