using System.Collections;
using System.Collections.Generic;
using ToadStates;
using UnityEngine;

public class ToadTongueController : MonoBehaviour
{
    private ToadController toad;
    public GameObject tonguePrefab; // 혀 끝부분 프리팹 (Collider 포함)
    private GameObject _currentTongue;
    private bool _isFiring;

    public void Initialize(ToadController monster) => toad = monster;

    public void FireTongue()
    {
        if (_isFiring) return;
        _isFiring = true;

        // 혀 생성 및 발사 (실제로는 오브젝트 풀링 권장)
        _currentTongue = Instantiate(tonguePrefab, transform.position, transform.rotation);
        var tongueScript = _currentTongue.GetComponent<TongueProjectile>();
        tongueScript.Launch(toad.Target.position, toad.ToadConfig.tongueSpeed, this);
    }

    // TongueProjectile에서 충돌 시 호출
    public void OnHit(Collider other)
    {
        _isFiring = false;

        if (other.CompareTag("Player"))
        {
            toad.ChangeState(new Pull());
        }
        else if (other.CompareTag("DungBall")) // 쇠똥 공 태그 확인
        {
            toad.ChangeState(new Stuck());
        }
        else
        {
            // 빗나감 -> 쿨다운
            toad.ChangeState(new Cooldown());
            ResetTongue();
        }
    }

    public void ResetTongue()
    {
        if (_currentTongue != null) Destroy(_currentTongue);
        _isFiring = false;
    }
}