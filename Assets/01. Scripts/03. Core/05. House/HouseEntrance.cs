using UnityEngine;
using Dung.Data.House;
using System.Collections;
using UnityEngine.SceneManagement;

// ==========================================
// 1. 부착 가능 오브젝트 (재료용)
// ==========================================
public class AttachableObject : MonoBehaviour, IAttachable
{
    [SerializeField] private string materialName;
    [SerializeField] private int quantity = 1;

    public string MaterialName => materialName;
    public int Quantity => quantity;

    public bool IsAttached { get; private set; }
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Attach(Transform target)
    {
        if (IsAttached) return;

        IsAttached = true;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        transform.SetParent(target);
    }

    public void Detach(Vector3? force = null)
    {
        if (!IsAttached) return;

        IsAttached = false;
        transform.SetParent(null);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            if (force.HasValue) rb.AddForce(force.Value, ForceMode.Impulse);
        }
    }
}

// ==========================================
// 2. 하우징 진입/퇴장 매니저
// ==========================================
public class HouseTransitionManager : MonoBehaviour
{
    [SerializeField] private DungHouseConfig config;
    [SerializeField] private string houseSceneName = "HouseInteriorScene";
    
    private GameObject player;
    private DungBall currentDung;

    public void EnterHouse(GameObject playerObj, DungBall dung)
    {
        player = playerObj;
        currentDung = dung;

        Debug.Log("[HouseTransition] Entering House...");
        
        // 1. 현재 상태 저장 (외부 위치 등)
        SaveWorldState();

        // 2. Additive 씬 로드
        StartCoroutine(LoadHouseRoutine());
    }

    private IEnumerator LoadHouseRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(houseSceneName, LoadSceneMode.Additive);
        yield return op;

        // 3. 내부 공간 설정 (Tier 기반)
        DungHouseConfig.HouseTier tier = config.GetTierByMass(currentDung.CurrentMass);
        SetupInterior(tier);

        // 4. 재료 변환
        HouseStorage storage = FindFirstObjectByType<HouseStorage>();
        if (storage != null) storage.ConvertAttachedObjects(currentDung);

        // 5. 카메라 전환 (쿼터뷰)
        SetupQuarterViewCamera();
    }

    private void SaveWorldState()
    {
        // TODO: 기획에 따른 외부 세계 영속성 저장 로직
        Debug.Log("[HouseTransition] World State Saved.");
    }

    private void SetupInterior(DungHouseConfig.HouseTier tier)
    {
        // TODO: 티어에 맞는 인프라 생성 및 그리드 초기화
        HouseGridSystem grid = FindFirstObjectByType<HouseGridSystem>();
        if (grid != null) grid.Initialize(tier.gridSize, config.cellSize);
    }

    private void SetupQuarterViewCamera()
    {
        // TODO: 카메라 시점 고정 (쿼터뷰)
        Debug.Log("[HouseTransition] Camera switched to Quarter-View.");
    }
}
