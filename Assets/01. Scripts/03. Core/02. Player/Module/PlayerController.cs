#region Class Description
/*
 * [PlayerController] (The Context / Body)
 * - FSM(Brain)이 명령을 내리는 물리적 신체입니다.
 * - 역할:
 * 1. 컴포넌트(Rigidbody, Input, Stats) 참조 제공
 * 2. 상태(State) 인스턴스 생성 및 보관
 * 3. 실제 물리 이동(Move), 점프(Jump) 등의 기능 구현
 */
#endregion

using UnityEngine;
using Dung.Data;

public class PlayerController : MonoBehaviour
{
    #region Components
    [field: SerializeField] public PlayerInputHandler Input { get; private set; }
    [field: SerializeField] public PlayerStats Stats { get; private set; }
    [field: SerializeField] public Rigidbody Rb { get; private set; }
    [field: SerializeField] public Transform CameraTransform { get; private set; }
    [field: SerializeField] public Animator Anim { get; private set; }

    public PlayerAbilityManager AbilityManager { get; private set; }
    #endregion

    #region State Machine
    public StateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public FallState FallState { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        // 카메라 자동 연결
        if (CameraTransform == null && Camera.main != null)
            CameraTransform = Camera.main.transform;

        // Rigidbody 회전 제한
        Rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        // 능력 관리자 초기화
        AbilityManager = gameObject.AddComponent<PlayerAbilityManager>();
        AbilityManager.Initialize(Stats);
    }

    private void InitializeStateMachine()
    {
        StateMachine = new StateMachine();

        IdleState = new IdleState(this, StateMachine, "Idle");
        MoveState = new MoveState(this, StateMachine, "Move");
        JumpState = new JumpState(this, StateMachine, "Jump");
        FallState = new FallState(this, StateMachine, "Fall");
    }
    #endregion

    #region Actions (Called by States)
    // 카메라 기준 이동
    public void Move(float speed)
    {
        Vector2 input = Input.MoveInput;
        Vector3 moveDir = GetCameraBasedDirection(input);

        // 목표 속도 계산
        Vector3 targetVel = moveDir * speed;
        Vector3 currentVel = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);

        // 가속/감속 적용
        float accel = (input.magnitude > 0)
            ? Stats.movement.acceleration
            : Stats.movement.deceleration;

        Vector3 newVel = Vector3.MoveTowards(
            currentVel, targetVel, accel * Time.fixedDeltaTime
        );

        Rb.velocity = new Vector3(newVel.x, Rb.velocity.y, newVel.z);

        // 회전 (입력이 있을 때만)
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Stats.movement.rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    // 점프
    public void Jump(float force)
    {
        Rb.velocity = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);
        Rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    // 지면 감지
    public bool CheckIfGrounded()
    {
        return Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            1.5f,
            Stats.detection.groundLayer
        );
    }
    #endregion

    #region Helper Methods
    private Vector3 GetCameraBasedDirection(Vector2 input)
    {
        if (CameraTransform == null) return Vector3.forward;

        Vector3 forward = CameraTransform.forward;
        Vector3 right = CameraTransform.right;

        // Y축 제거 (수평면만 사용)
        forward.y = 0;
        right.y = 0;

        return (forward.normalized * input.y + right.normalized * input.x).normalized;
    }
    #endregion
}