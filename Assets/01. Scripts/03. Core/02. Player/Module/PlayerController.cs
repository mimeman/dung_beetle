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

using Dung.Data;
using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Components
    [field: SerializeField] public PlayerInputHandler Input { get; private set; }
    [field: SerializeField] public PlayerStats Stats { get; private set; }
    [field: SerializeField] public Rigidbody Rb { get; private set; }
    [field: SerializeField] public Transform CameraTransform { get; private set; }
    [field: SerializeField] public Animator Anim { get; private set; }
    [field: SerializeField] public FullBodyBipedIK IKSolver { get; private set; }

    public PlayerPhysicsHandler PhysicsHandler { get; private set; }
    public PlayerDetector Detector { get; private set; }
    public PlayerAbilityManager AbilityManager { get; private set; }
    #endregion

    #region State Machine
    public StateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public FallState FallState { get; private set; }

    public PlayerPushState PushState { get; private set; }
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

        if (Input.InteractTriggered)
        {
            HandleInteractionInput();
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        if (CameraTransform == null && Camera.main != null)
            CameraTransform = Camera.main.transform;


        if (Anim != null)
        {
            Anim.transform.localPosition = Vector3.zero;
            Anim.transform.localRotation = Quaternion.identity;
        }

        if (Rb != null)
        {
            Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        AbilityManager = gameObject.AddComponent<PlayerAbilityManager>();
        AbilityManager.Initialize(Stats);
        PhysicsHandler = GetComponent<PlayerPhysicsHandler>();
        Detector = GetComponentInChildren<PlayerDetector>();

        if (Detector != null) Detector.Initialize(this);
    }

    private void InitializeStateMachine()
    {
        StateMachine = new StateMachine();

        IdleState = new IdleState(this, StateMachine, "Idle");
        MoveState = new MoveState(this, StateMachine, "Move");
        JumpState = new JumpState(this, StateMachine, "Jump");
        FallState = new FallState(this, StateMachine, "Fall");

        // PushState 인스턴스 생성 및 애니메이션 파라미터 등록
        PushState = new PlayerPushState(this, StateMachine, "IsPushing");
    }
    #endregion

    #region Interaction Logic
    private void HandleInteractionInput()
    {
        Debug.Log("<color=yellow>[Controller]</color> E 키 입력 감지됨");

        if (StateMachine.CurrentState == PushState)
        {
            Debug.Log("<color=yellow>[Controller]</color> 밀기 종료 -> Idle 상태로 변경");
            StateMachine.ChangeState(IdleState);
        }
        else if (Detector.CurrentInteractable != null)
        {
            Debug.Log("<color=yellow>[Controller]</color> 밀기 시작 -> Push 상태로 변경");
            StateMachine.ChangeState(PushState);
        }
        else
        {
            Debug.Log("<color=red>[Controller]</color> 앞에 감지된 공이 없어 전환 실패");
        }
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

    public void RotateTowards(Vector3 targetPosition, float rotationSpeed)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public IEnumerator SetIKWeight(float targetWeight, float duration)
    {
        if (IKSolver == null) yield break;

        float startWeight = IKSolver.solver.IKPositionWeight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, elapsed / duration);

            // 전체 솔버 가중치
            IKSolver.solver.IKPositionWeight = currentWeight;

            // 양손의 개별 가중치도 함께 올려줘야 손이 움직입니다!
            IKSolver.solver.leftHandEffector.positionWeight = currentWeight;
            IKSolver.solver.rightHandEffector.positionWeight = currentWeight;

            yield return null;
        }

        IKSolver.solver.IKPositionWeight = targetWeight;
        IKSolver.solver.leftHandEffector.positionWeight = targetWeight;
        IKSolver.solver.rightHandEffector.positionWeight = targetWeight;
    }
    #endregion

    #region Helper Methods
    private Vector3 GetCameraBasedDirection(Vector2 input)
    {
        if (CameraTransform == null) return Vector3.forward;

        Vector3 forward = CameraTransform.forward;
        Vector3 right = CameraTransform.right;

        forward.y = 0;
        right.y = 0;

        return (forward.normalized * input.y + right.normalized * input.x).normalized;
    }
    #endregion
}