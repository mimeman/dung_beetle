using UnityEngine;
using System.Collections.Generic;
using Dung.Data.House;

// ==========================================
// 1. 그리드 배치 시스템
// ==========================================
public class HouseGridSystem : MonoBehaviour
{
    private Dictionary<Vector2Int, FurnitureData> occupiedCells = new Dictionary<Vector2Int, FurnitureData>();
    private Vector2Int currentGridSize;
    private float cellSize;

    public void Initialize(Vector2Int size, float cell)
    {
        currentGridSize = size;
        cellSize = cell;
        Debug.Log($"[HouseGrid] Initialized with size {size}");
    }

    public bool CanPlace(FurnitureData data, Vector2Int pos)
    {
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                Vector2Int cell = pos + new Vector2Int(x, y);
                if (cell.x < 0 || cell.x >= currentGridSize.x || cell.y < 0 || cell.y >= currentGridSize.y) return false;
                if (occupiedCells.ContainsKey(cell)) return false;
            }
        }
        return true;
    }

    public void PlaceFurniture(FurnitureData data, Vector2Int pos)
    {
        if (!CanPlace(data, pos)) return;

        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                occupiedCells[pos + new Vector2Int(x, y)] = data;
            }
        }
        
        Debug.Log($"[HouseGrid] Placed {data.furnitureName} at {pos}");
        // TODO: 프리팹 인스턴스화 로직
    }
    
    public void HandleShrink(Vector2Int newSize)
    {
        // 집이 작아질 때 범위를 벗어나는 가구를 창고로 회수
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var cell in occupiedCells.Keys)
        {
            if (cell.x >= newSize.x || cell.y >= newSize.y)
            {
                toRemove.Add(cell);
            }
        }

        foreach (var cell in toRemove)
        {
            FurnitureData data = occupiedCells[cell];
            // TODO: 창고에 다시 넣는 로직
            occupiedCells.Remove(cell);
            Debug.Log($"[HouseGrid] Recovered {data.furnitureName} due to shrinking.");
        }
        currentGridSize = newSize;
    }
}

// ==========================================
// 2. 창고 및 재료 시스템
// ==========================================
public class HouseStorage : MonoBehaviour
{
    private Dictionary<string, int> materials = new Dictionary<string, int>();

    public void AddMaterial(string name, int amount)
    {
        if (materials.ContainsKey(name)) materials[name] += amount;
        else materials[name] = amount;
        Debug.Log($"[HouseStorage] Added {amount} of {name}. Total: {materials[name]}");
    }

    public bool ConsumeMaterial(string name, int amount)
    {
        if (materials.ContainsKey(name) && materials[name] >= amount)
        {
            materials[name] -= amount;
            return true;
        }
        return false;
    }

    // 경단에 붙은 오브젝트들을 재료로 변환
    public void ConvertAttachedObjects(DungBall dungBall)
    {
        IAttachable[] attached = dungBall.GetComponentsInChildren<IAttachable>();
        foreach (var item in attached)
        {
            if (item is AttachableObject obj)
            {
                AddMaterial(obj.MaterialName, obj.Quantity);
                obj.Detach(); // 쇠똥에서 떼어냄
                Destroy(obj.gameObject); // 재료로 변환되었으므로 삭제
            }
        }
    }
}
