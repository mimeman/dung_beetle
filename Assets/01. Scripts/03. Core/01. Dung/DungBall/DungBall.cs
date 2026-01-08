#region 설명
/* [DungBall]
 * 쇠똥구리 게임의 핵심 오브젝트인 '쇠똥'의 중앙 제어 장치(Hub)
 * * [구조: Facade Pattern]
 * - 본체는 직접 기능을 수행하지 않고, 하위 모듈(Module)들에게 작업을 위임함
 * - 외부(플레이어, 아이템)는 이 클래스의 인터페이스만 알면 됨
 
 * [모듈 연결]
 * - DungIdentity : ID 관리
 * - DungGrowth   : 성장/감소 계산
 * - DungPhysics  : 물리 엔진(Rigidbody) 제어
 * - DungVisual   : 외형(Scale) 및 이펙트
 * - DungAudio    : 효과음
 */
#endregion

using UnityEngine;
using Dung.Data;
using Dung.Data.Save;
using Dung.Enums;

[RequireComponent(typeof(DungIdentity))]
[RequireComponent(typeof(DungGrowth))]
[RequireComponent(typeof(DungPhysics))]
[RequireComponent(typeof(DungAudio))]
public class DungBall : MonoBehaviour, IGrabbable, ISaveable, IIDProvider, IGrowable
{
    #region Variables & Modules

    [Header("Settings")]
    [SerializeField] private DungStats _stats;

    [Header("Visual Reference")]
    [SerializeField] private DungVisual _visual;

    // 내부 모듈
    private DungIdentity _identity;
    private DungGrowth _growth;
    private DungPhysics _physics;
    private DungAudio _audio;

    private bool _isGrabbed;

    #endregion

    #region Interface Properties

    // [IIDProvider]
    public string SaveID => _identity.ID;

    // [IGrowable]
    public float CurrentMass => _growth.CurrentMass;
    public float CurrentRadius => transform.localScale.x * 0.5f;
    public bool IsMaxSize => _growth.IsMaxSize;
    public float WeatherResistance => _stats?.physics.weatherResistance ?? 0f;

    // [IGrabbable]
    public bool IsGrabbed => _isGrabbed;
    private IGrabber _currentGrabber;

    // 잡았을 때 느껴지는 무게감 (플레이어 이동 속도 감소 계산용)
    public float DragWeight => _growth.CurrentMass * 0.5f;

    #endregion

    #region Initialization
    private void Awake()
    {
        _identity = GetComponent<DungIdentity>();
        _growth = GetComponent<DungGrowth>();
        _physics = GetComponent<DungPhysics>();
        _audio = GetComponent<DungAudio>();

        if (_visual == null)
            _visual = GetComponentInChildren<DungVisual>();

        InitializeDung();
    }

    // 각 하위 모듈(물리, 성장, 오디오)을 초기화하고 이벤트를 연결합니다.
    private void InitializeDung()
    {
        if (_stats == null)
        {
            Debug.LogError($"[DungBall] {name}: DungStats가 누락되었습니다.");
            return;
        }

        // 각 모듈 초기화
        _growth.Initialize(_stats);
        _physics.Initialize(_stats, _stats.growth.initialMass);
        _audio.Initialize(_stats);

        if (_visual != null)
            _visual.InitializeVisual(_stats);

        // 이벤트 구독
        _growth.OnMassChanged += HandleMassChanged;
        _growth.OnCrumble += HandleCrumble;
    }

    // 메모리 누수 방지를 위해 이벤트 연결을 해제합니다.
    private void OnDestroy()
    {
        if (_growth != null)
        {
            _growth.OnMassChanged -= HandleMassChanged;
            _growth.OnCrumble -= HandleCrumble;
        }
    }

    #endregion

    #region Event Handlers

    // 질량이 변경되었을 때 물리 엔진과 비주얼을 동기화합니다.
    private void HandleMassChanged(float newMass)
    {
        _physics.UpdateMass(newMass);

        if (_visual != null)
            _visual.UpdateSize(newMass);
    }

    // 파괴되었을 때 이펙트와 소리를 재생하고 오브젝트를 비활성화합니다.
    private void HandleCrumble()
    {
        if (_visual != null) _visual.PlayCrumbleEffect();
        if (_audio != null) _audio.PlayBreakSound();

        gameObject.SetActive(false);
    }

    #endregion

    #region Interface Implementations

    // 외부에서 성장을 요청할 때 호출됩니다. (성장 모듈 위임)
    public bool Grow(float amount, GrowthType type)
    {
        bool success = _growth.CalculateGrowth(amount, type);

        if (success && _audio != null)
            _audio.PlayGrowSound();

        return success;
    }

    // 외부 충격이나 날씨로 인해 크기가 줄어들 때 호출됩니다.
    public bool Shrink(float amount, ShrinkType type)
    {
        return _growth.CalculateShrink(amount, type);
    }

    // 강제로 쇠똥을 파괴합니다.
    public void Crumble()
    {
        _growth.CalculateShrink(_growth.CurrentMass, ShrinkType.Physical);
    }

    // 플레이어(또는 다른 주체)가 쇠똥을 잡았을 때 호출됩니다.
    public void Grab(IGrabber grabber)
    {
        if (_isGrabbed) return;

        _isGrabbed = true;
        _currentGrabber = grabber;
        _physics.SetGrabbedState(true);
    }

    // 잡고 있던 쇠똥을 놓았을 때 호출됩니다.
    public void Release()
    {
        _isGrabbed = false;
        _currentGrabber = null;
        _physics.SetGrabbedState(false);
    }

    // 현재 상태(위치, 회전, 질량)를 저장 데이터로 변환합니다.
    public object CaptureState()
    {
        return new DungSaveData
        {
            id = _identity.ID,
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            rotX = transform.rotation.x,
            rotY = transform.rotation.y,
            rotZ = transform.rotation.z,
            rotW = transform.rotation.w,
            currentMass = _growth.CurrentMass
        };
    }

    // 저장된 데이터를 불러와 상태를 복구합니다.
    public void RestoreState(object state)
    {
        var data = (DungSaveData)state;

        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

        _growth.LoadMassState(data.currentMass);
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentRadius);
    }
#endif
}