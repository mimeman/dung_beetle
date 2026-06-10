using UnityEngine;
using System.Collections.Generic;

namespace Dung.Data.House
{
    // ==========================================
    // 1. 제작 레시피
    // ==========================================
    [System.Serializable]
    public class CraftingRecipe
    {
        [System.Serializable]
        public class Ingredient
        {
            public string materialName; // "Stone", "Twig", "Leaf" 등
            public int requiredCount;
        }
        
        public List<Ingredient> ingredients = new List<Ingredient>();
    }

    // ==========================================
    // 2. 가구 데이터
    // ==========================================
    [CreateAssetMenu(fileName = "FurnitureData", menuName = "DungBeetle/House/Furniture", order = 1)]
    public class FurnitureData : ScriptableObject
    {
        public string furnitureName;
        public Vector2Int size = new Vector2Int(1, 1); // 그리드 점유 크기
        public bool isWallDecor;                      // 벽 장식 여부
        public GameObject prefab;                      // 가구 프리팹
        public Sprite icon;                            // UI 아이콘
        public CraftingRecipe recipe;                  // 제작 비용
    }

    // ==========================================
    // 3. 집 티어 및 설정
    // ==========================================
    [CreateAssetMenu(fileName = "DungHouseConfig", menuName = "DungBeetle/House/HouseConfig", order = 2)]
    public class DungHouseConfig : ScriptableObject
    {
        [System.Serializable]
        public class HouseTier
        {
            public string tierName;
            public float minMass;             // 진입 가능한 최소 질량
            public Vector2Int gridSize;       // 내부 그리드 크기
            public float ceilingHeight = 5f;
            public GameObject interiorPrefab; // 집 내부 공간 프리팹
        }

        public List<HouseTier> tiers = new List<HouseTier>();
        public float cellSize = 1f;           // 그리드 한 칸의 크기 (유니티 단위)
        
        public HouseTier GetTierByMass(float mass)
        {
            HouseTier currentTier = tiers[0];
            foreach (var tier in tiers)
            {
                if (mass >= tier.minMass) currentTier = tier;
            }
            return currentTier;
        }
    }
}
