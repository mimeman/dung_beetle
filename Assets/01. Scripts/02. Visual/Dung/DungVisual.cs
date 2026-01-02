using Dung.Data;
using UnityEngine;

public class DungVisual : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private MeshRenderer _meshRenderer;

    private DungStats.VisualData _visualData;
    private float _initialMass; // 초기 질량 저장


    /// <param name="stats">쇠똥 데이터</param>
    public void InitializeVisual(DungStats stats)
    {
        _visualData = stats.visual;
        _initialMass = stats.growth.initialMass;

        if (_meshRenderer != null && _visualData.skinMaterial != null)
            _meshRenderer.material = _visualData.skinMaterial;
    }

    // 질량 변화에 따라 크기를 갱신
    /// <param name="currentMass">현재 질량 (kg)</param>
    public void UpdateSize(float currentMass)
    {
        if (_modelTransform == null || _initialMass <= 0) return;

        // 부피는 질량에 비례하므로, 반지름은 질량의 세제곱근에 비례
        float massRatio = currentMass / _initialMass;
        float scaleFactor = Mathf.Pow(massRatio, 0.33f); // 세제곱근

        // 최소 0.5배 ~ 최대 5배로 제한
        float finalScale = Mathf.Clamp(scaleFactor, 0.5f, 5.0f);

        _modelTransform.localScale = Vector3.one * finalScale;
    }

    // 똥이 파괴될 때 이펙트를 재생합니다.
    public void PlayCrumbleEffect()
    {
        if (_visualData.crumbleEffect != null)
            Instantiate(_visualData.crumbleEffect, transform.position, Quaternion.identity);
    }
}