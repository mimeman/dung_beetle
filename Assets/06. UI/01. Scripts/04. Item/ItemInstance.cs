using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    #region [ItemInstance 설명]
    /* [설명]
     * 인벤토리 슬롯에 실제로 배치되는 아이템의 '개별 실체' 데이터입니다.
     * 원본 설계도(ItemBaseData)를 참조하며, 각 아이템마다 다른 수량과 회전 상태를 관리합니다.
     * * [주요 역할]
     * 1. 상태 유지: 현재 수량, 회전 여부 등 개별적인 상태 저장.
     * 2. 규격 반환: 회전 상태를 고려하여 현재 차지하는 가로/세로 길이 제공.
     * 3. 무게 연산: 기획서 규칙(수량 * 단위 무게)에 따른 실시간 중량 계산.
     */
    #endregion

    // [필드]
    [SerializeField] private ItemBaseData _baseData; // 원본 데이터 참조
    [SerializeField] private int _currentQuantity;   // 현재 쌓인 수량
    [SerializeField] private bool _isRotated = false; // 회전 여부 상태

    // [로직]
    public ItemInstance(ItemBaseData data, int quantity)
    {
        _baseData = data;
        // 생성 시 최대 수량을 넘지 않도록 제한 (기획서 규칙 반영)
        _currentQuantity = Mathf.Min(quantity, data.MaxQuantity);
    }

    // 현재 수량에 따른 전체 무게 계산 (단위: 근)
    public float GetTotalWeight()
    {
        return _baseData.WeightPerUnit * _currentQuantity;
    }

    // 회전 상태를 고려한 실시간 가로 크기 반환
    public int GetCurrentWidth()
    {
        return _isRotated ? _baseData.Height : _baseData.Width;
    }

    // 회전 상태를 고려한 실시간 세로 크기 반환
    public int GetCurrentHeight()
    {
        return _isRotated ? _baseData.Width : _baseData.Height;
    }

    // 프로퍼티
    public ItemBaseData BaseData => _baseData;
    public int CurrentQuantity { get => _currentQuantity; set => _currentQuantity = value; }
    public bool IsRotated { get => _isRotated; set => _isRotated = value; }
}