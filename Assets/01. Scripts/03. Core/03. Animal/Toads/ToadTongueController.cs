using System.Collections;
using System.Collections.Generic;
using ToadStates;
using UnityEngine;

public class ToadTongueController : MonoBehaviour
{
    private ToadController toad;

    [Header("Tongue References")]
    [SerializeField] private GameObject tonguePrefab; // 혀 끝부분 프리팹 (Collider 포함)
    [SerializeField] private Transform firePoint;     // 혀 시작점 (입 안쪽)
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Tongue Curve Settings")]
    [SerializeField] private int tongueSegments = 20;          // 라인 세그먼트 수
    [SerializeField] private float tongueWaveAmplitude = 0.15f; // 출렁임 진폭
    [SerializeField] private float tongueWaveDamping = 3f;      // 출렁임 감쇠 속도

    private GameObject _currentTongue;
    private TongueProjectile _currentProjectile;
    private bool _isFiring;
    private float _fireTime; // 발사 시점 기록 (출렁임 감쇠용)

    public void Initialize(ToadController monster)
    {
        toad = monster;
        SetupLineRenderer();
    }

    /// <summary>
    /// LineRenderer 초기 설정
    /// </summary>
    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = firePoint.gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = firePoint.gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        // Material 설정 (Unlit 색상)
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = toad.ToadConfig.tongueColor;
        lineRenderer.endColor = toad.ToadConfig.tongueColor;
        lineRenderer.startWidth = toad.ToadConfig.tongueWidth;
        lineRenderer.endWidth = toad.ToadConfig.tongueWidth * 0.6f; // 끝이 약간 가늘게
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.useWorldSpace = true;
    }

    public void FireTongue()
    {
        if (_isFiring) return;
        _isFiring = true;
        _fireTime = Time.time;

        // 혀 끝 생성 및 발사
        _currentTongue = Instantiate(tonguePrefab, firePoint.position, transform.rotation);
        _currentProjectile = _currentTongue.GetComponent<TongueProjectile>();
        _currentProjectile.Launch(toad.Target.position + new Vector3(0, 0.5f, 0), toad.ToadConfig.tongueSpeed, this);

        // LineRenderer 활성화
        lineRenderer.positionCount = tongueSegments;
        lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (!lineRenderer.enabled) return;

        // LineRenderer 갱신: 혀 끝이 살아있는 동안 업데이트
        if (_currentTongue != null)
        {
            UpdateTongueLine(_currentTongue.transform.position);
        }
    }

    /// <summary>
    /// firePoint에서 endPos까지 곡선 보간하여 LineRenderer를 업데이트
    /// </summary>
    private void UpdateTongueLine(Vector3 endPos)
    {
        Vector3 start = firePoint.position;
        Vector3 end = endPos;
        float elapsed = Time.time - _fireTime;

        // 출렁임 감쇠 계수 (시간이 지날수록 감소)
        float dampFactor = Mathf.Exp(-tongueWaveDamping * elapsed);

        for (int i = 0; i < tongueSegments; i++)
        {
            float t = i / (float)(tongueSegments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // 양 끝을 제외한 중간 포인트에 출렁임 적용
            if (i > 0 && i < tongueSegments - 1)
            {
                // 사인파 + 시간 기반 흔들림
                float wave = Mathf.Sin(t * Mathf.PI * 2f + elapsed * 8f)
                             * tongueWaveAmplitude
                             * Mathf.Sin(t * Mathf.PI) // 양 끝은 0, 중간이 최대
                             * dampFactor;

                point += transform.right * wave;
            }

            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// TongueProjectile에서 충돌 시 호출
    /// </summary>
    public void OnHit(Collider other)
    {
        _isFiring = false;

        if (!other)
        {
            // 빗나감 -> 쿨다운
            toad.ChangeState(new Cooldown());
            ResetTongue();
        }
        else if (other.CompareTag("Player"))
        {
            // Player 히트 → Pull 상태 (LineRenderer 유지)
            toad.ChangeState(new Pull());
        }
        else if (other.CompareTag("DungBall"))
        {
            // DungBall 히트 → Stuck 상태 (LineRenderer 유지)
            toad.ChangeState(new Stuck());
        }
        else
        {
            toad.ChangeState(new Idle());
            ResetTongue();
        }
    }

    /// <summary>
    /// 혀 오브젝트 제거 및 LineRenderer 비활성화
    /// </summary>
    public void ResetTongue()
    {
        if (_currentTongue != null)
            Destroy(_currentTongue);

        _currentProjectile = null;
        _isFiring = false;

        // LineRenderer 비활성화
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }
    }
}