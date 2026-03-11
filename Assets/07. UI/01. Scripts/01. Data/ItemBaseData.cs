using UnityEngine;

#region [ItemBaseData 설명]
/* [설명]
 * 인벤토리 시스템에서 사용하는 '아이템의 원본 규격 데이터'를 정의하는 ScriptableObject입니다.
 * 기획서의 '근' 단위 무게 체계와 '최대 보유 수량' 규칙을 지원합니다.
 * 일부 무게가 없는 아이템(약초 등)은 WeightPerUnit을 0으로 설정하여 처리합니다.
 * * [데이터 구성]
 * 1. 아이템 식별: 고유 ID, 이름, 아이콘 스프라이트, 타입 및 분류 색상.
 * 2. 배치 규격: 화물 인벤토리 내 차지하는 가로(Width)와 세로(Height) 칸 수.
 * 3. 수량 및 무게: 최대 보유 수량(초과 시 별도 수납) 및 단위 수량당 무게(근).
 * 4. 귀속 정보: 여정이 끝나도 인벤토리에 남는지 여부(기본 true).
 */
#endregion

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemBaseData : ScriptableObject
{
    // [필드]
    [Header("기본 정보")]
    [SerializeField] private string _itemID;
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _itemIcon;
    [SerializeField] private ItemType _itemType;
    [SerializeField] private Color _typeColor;

    [Header("배치 규격 (Grid Size)")]
    [SerializeField] private int _width = 1;
    [SerializeField] private int _height = 1;

    [Header("수량 및 무게 (Geun)")]
    [SerializeField] private int _maxQuantity = 1;      // 최대 보유 수량 (예: 곡물 200근)
    [SerializeField] private float _weightPerUnit = 0f;  // 수량 1개당 무게 (단위: 근)

    [Header("기타 설정")]
    [SerializeField] private bool _isPlayerBound = true; // 플레이어 화물 귀속 여부

    // [로직 - 프로퍼티]
    public string ItemID => _itemID;
    public string ItemName => _itemName;
    public Sprite ItemIcon => _itemIcon;
    public ItemType ItemType => _itemType;
    public Color TypeColor => _typeColor;
    public int Width => _width;
    public int Height => _height;
    public int MaxQuantity => _maxQuantity;
    public float WeightPerUnit => _weightPerUnit;
    public bool IsPlayerBound => _isPlayerBound;
}