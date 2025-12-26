using Dung.Data;
using UnityEngine;

public class DungVisual : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private MeshRenderer _meshRenderer;

    private DungStats.VisualData _visualData;

    // 초기화
    public void InitializeVisual(DungStats stats)
    {
        _visualData = stats.visual;

        if(_meshRenderer != null && _visualData.skinMaterial != null)
            _meshRenderer.material = _visualData.skinMaterial;

    }

    // 질량 변화에 따라 크기 갱신
    public void UpdateSize(float currentMass, float initalMass)
    {
        if(_modelTransform == null) return;

        float scaleFactor = 1.0f + (currentMass * initalMass) * 0.1f;
        float finalScale = Mathf.Clamp(scaleFactor, 1.0f, 5.0f);

        _modelTransform.localPosition = Vector3.one * finalScale;
    }

    //똥 파괴될 때 이펙트 재생
    public void PlayCrumbleEffect()
    {
        if (_visualData.crumbleEffect != null)
            Instantiate(_visualData.crumbleEffect, transform.position, Quaternion.identity);
    }

}
