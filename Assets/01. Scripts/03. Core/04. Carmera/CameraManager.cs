using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("vcam 연결")]
    public CinemachineVirtualCamera vcamQuarter;
    public CinemachineVirtualCamera vcamFPS;

    [Header("쿼터뷰 - 우클릭 궤도 회전")]
    public Transform orbitTarget;
    public float orbitSpeed = 120f;

    [Header("1인칭 - 마우스 감도")]
    public float fpsSensitivityX = 2f;
    public float fpsSensitivityY = 2f;
    public float fpsMinPitch = -60f;
    public float fpsMaxPitch = 60f;

    [Header("1인칭 - 걷기 Bob")]
    public float fpsBobAmount = 0.05f;
    public float fpsBobSpeed = 10f;

    [Header("1인칭 - 플레이어 메시 숨김")]
    public Renderer[] playerRenderers;

    [Header("시점 전환 블렌딩")]
    public float blendDuration = 0.5f;

    private const int ACTIVE = 11;
    private const int INACTIVE = 0;

    public enum CameraMode { Quarter, FPS }
    private CameraMode _currentMode = CameraMode.Quarter;
    private bool _isInside = false;
    private bool _isBlending = false;

    private PlayerInputHandler _input;
    private PlayerController _player;

    // 쿼터뷰 궤도 회전
    private float _currentYaw;
    private float _yawVelocity;

    // 1인칭 회전
    private float _fpsYaw;
    private float _fpsPitch;

    // 1인칭 Bob
    private float _bobTimer;
    private Vector3 _fpsInitLocalPos;

    // F키 뼈대 (추후 확장)
    // public event System.Action OnFKeyPressed;

    private void Awake()
    {
        Instance = this;
        _input = FindObjectOfType<PlayerInputHandler>();
        _player = FindObjectOfType<PlayerController>();

        if (orbitTarget != null)
            _currentYaw = orbitTarget.eulerAngles.y;

        if (vcamFPS != null)
            _fpsInitLocalPos = vcamFPS.transform.localPosition;

        if (_player != null)
            _fpsYaw = _player.transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleViewToggle();

        // 쿼터뷰 - 우클릭 회전
        if (_currentMode == CameraMode.Quarter)
            HandleOrbitRotation();
    }

    // Cinemachine이 Update에서 카메라 위치 계산하므로
    // FPS 회전/Bob은 LateUpdate에서 덮어써야 정확하게 적용됨
    private void LateUpdate()
    {
        if (_currentMode != CameraMode.FPS) return;

        HandleFPSRotation();
        HandleFPSBob();
    }


    // V키 - 쿼터뷰 ↔ 1인칭
    private void HandleViewToggle()
    {
        // ToggleViewTriggered 대신 직접 키 체크로 임시 교체
        if (!Input.GetKeyDown(KeyCode.V)) return;
        if (_isInside) return;
        if (_isBlending) return;

        SwitchTo(_currentMode == CameraMode.Quarter
            ? CameraMode.FPS
            : CameraMode.Quarter);
    }

    // 쿼터뷰 - 우클릭 Y축 궤도 회전
    private void HandleOrbitRotation()
    {
        if (!_input.IsAiming || orbitTarget == null) return;

        float mouseX = _input.LookInput.x;
        float targetYaw = _currentYaw + mouseX * orbitSpeed * Time.deltaTime;
        _currentYaw = Mathf.SmoothDampAngle(
            _currentYaw, targetYaw, ref _yawVelocity, 0.05f
        );
        orbitTarget.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
    }


    // 1인칭 - 마우스 회전
    private void HandleFPSRotation()
    {
        if (_player == null || vcamFPS == null) return;

        float mouseX = _input.LookInput.x * fpsSensitivityX;
        float mouseY = _input.LookInput.y * fpsSensitivityY;

        _fpsYaw += mouseX;
        _fpsPitch -= mouseY;
        _fpsPitch = Mathf.Clamp(_fpsPitch, fpsMinPitch, fpsMaxPitch);

        // 플레이어 몸체 Y축 회전 (좌우)
        _player.transform.rotation = Quaternion.Euler(0f, _fpsYaw, 0f);

        // vcamFPS Transform 직접 회전 (상하 + 좌우)
        vcamFPS.transform.rotation = Quaternion.Euler(_fpsPitch, _fpsYaw, 0f);
    }

    // 1인칭 - 걷기 Bob
    private void HandleFPSBob()
    {
        if (vcamFPS == null) return;

        bool isMoving = _input.MoveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            _bobTimer += Time.deltaTime * fpsBobSpeed;
            float bobY = Mathf.Sin(_bobTimer) * fpsBobAmount;
            float bobX = Mathf.Sin(_bobTimer * 0.5f) * fpsBobAmount * 0.5f;
            vcamFPS.transform.localPosition = _fpsInitLocalPos
                + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            _bobTimer = 0f;
            vcamFPS.transform.localPosition = Vector3.Lerp(
                vcamFPS.transform.localPosition,
                _fpsInitLocalPos,
                Time.deltaTime * 10f
            );
        }
    }

    // 집 진입/퇴장
    public void EnterHouse()
    {
        _isInside = true;
        SwitchTo(CameraMode.FPS);
    }

    public void ExitHouse()
    {
        _isInside = false;
        SwitchTo(CameraMode.Quarter);
    }

    // 똥 굴리기
    public void OnPushStart()
    {
        if (_isInside) return;
        if (_currentMode == CameraMode.FPS)
            SwitchTo(CameraMode.Quarter);
    }

    public void OnPushEnd() { }

    // 시점 전환 코어
    private void SwitchTo(CameraMode mode)
    {
        _currentMode = mode;
        vcamQuarter.Priority = INACTIVE;
        vcamFPS.Priority = INACTIVE;

        switch (mode)
        {
            case CameraMode.Quarter:
                vcamQuarter.Priority = ACTIVE;
                // 쿼터뷰 복귀 시 궤도 yaw 동기화
                if (orbitTarget != null)
                    _currentYaw = orbitTarget.eulerAngles.y;
                // 커서 해제
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SetPlayerVisible(true);
                break;

            case CameraMode.FPS:
                vcamFPS.Priority = ACTIVE;
                // FPS 진입 시 현재 플레이어 방향으로 yaw 동기화
                if (_player != null)
                    _fpsYaw = _player.transform.eulerAngles.y;
                _fpsPitch = 0f;
                // 커서 잠금
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                SetPlayerVisible(false);
                break;


        }

        StartCoroutine(BlendCooldown());
    }
    private void SetPlayerVisible(bool visible)
    {
        foreach (var r in playerRenderers)
            if (r != null) r.enabled = visible;
    }

    // 전환 직후 재입력 방지
    private IEnumerator BlendCooldown()
    {
        _isBlending = true;
        yield return new WaitForSeconds(blendDuration);
        _isBlending = false;
    }

    // 외부 접근용
    public CameraMode CurrentMode => _currentMode;
    public bool IsInside => _isInside;
}