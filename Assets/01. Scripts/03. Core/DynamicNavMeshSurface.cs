using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 크기가 큰 맵일경우 NavMeshSurface의 Data가 메모리에 많이 잡히는 성능 저하가 발생합니다.
/// 이를 해결해주기 위해서 CPU연산으로 NavMeshSurface의 위치를 이동시켜주며 (주로 플레이어 중심)
/// Surface의 생성 방식을 Volume으로 하여 적은 메모리용량으로 NavMesh를 사용할수 있습니다.
/// [ 중요사항 ]
/// NavMeshSurface의 Object Collection의 Collect Objcet의 방식을 Volume으로 해주고 사이즈를 정해주세요.
/// [ 변경사항 ]
/// Grounded와 같은 게임에서도 Bake된 navmesh surface를 사용하는 방식을 채용했습니다.
/// 따라서 우리는 이 컴포넌트를 쓰지는 않을것입니다.
/// </summary>

// ExecutionOrder를 우선순위로 해줍니다. (TMP 앞으로 오게 됩니다. 플레이어보다 빠르게 처리해야하기 때문.)
[DefaultExecutionOrder(-115)]
[RequireComponent(typeof(NavMeshSurface))]
public class DynamicNavMeshSurface : MonoBehaviour
{
    [SerializeField] private GameObject trackedAgent;
    [Range(0.01f, 1f)]
    [Tooltip("NavMeshSurface의 Volume 위치 보정을 해줄 frequency")]
    [SerializeField] private float quantizationFactor = 0.1f;

    private NavMeshSurface surface = null;
    private Vector3 volumeSize = Vector3.zero;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
    }

    void OnEnable()
    {
        volumeSize = surface.size;
        surface.center = GetQuantizedCenter();
        surface.BuildNavMesh();
    }

    void Update()
    {
        var updatedCenter = GetQuantizedCenter();
        var updatedNavMesh = false;

        if (surface.center != updatedCenter)
        {   // 기존 surface의 center와 달라졌을때
            surface.center = updatedCenter;
            updatedNavMesh = true;
        }

        if (surface.size != volumeSize)
        {   // surface의 size가 변경되었을때
            volumeSize = surface.size;
            updatedNavMesh = true;
        }

        if (updatedNavMesh)
        {   // NavMesh Surface를 업데이트 시켜줍니다.
            surface.UpdateNavMesh(surface.navMeshData);
        }
    }

    private Vector3 GetQuantizedCenter()
    {
        return trackedAgent.transform.position.Quantize(surface.size * quantizationFactor);
    }

}

public static class Vector3Extensions
{
    public static Vector3 Quantize(this Vector3 position, Vector3 quantization)
    {
        return Vector3.Scale(quantization, new Vector3(
            Mathf.Floor(position.x / quantization.x),
            Mathf.Floor(position.y / quantization.y),
            Mathf.Floor(position.z / quantization.z)
        ));
    }
}