using UnityEngine;
using Dung.Enums;

namespace Dung.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "DungBeetle/Item/Item Data", order = 0)]
    public class ItemData : ScriptableObject
    {
        [Header("엑셀 데이터 매칭용 (Excel Mapping)")]
        public int itemID;          // 고유 ID
        public string itemName;     // 이름
        public ItemType itemType;   // 아이템 종류

        [Header("물리적 속성 (Physical)")]
        public float mass;          // 무게
        public GrowthType materialType; // 재질

        [Header("비주얼 (Visual)")]
        public GameObject prefab;   // 3D 모델
    }
}