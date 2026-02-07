using Dung.Data;
using RootMotion.FinalIK;
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
    [field: SerializeField] public GrounderFBBIK Grounder { get; private set; }

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
        UpdateAnimator();

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

        if (Rb != null)
        {
            Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
            Rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Grounder 자동 찾기 (연결 안 되어 있으면)
        if (Grounder == null)
        {
            Grounder = GetComponentInChildren<GrounderFBBIK>();
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
        PushState = new PlayerPushState(this, StateMachine, "IsPushing");
    }
    #endregion

    #region Interaction Logic
    private void HandleInteractionInput()
    {
        if (StateMachine.CurrentState == PushState)
        {
            StateMachine.ChangeState(IdleState);
        }
        else if (Detector.CurrentInteractable != null)
        {
            StateMachine.ChangeState(PushState);
        }
    }
    #endregion

    #region Animation
    private void UpdateAnimator()
    {
        if (Anim == null) return;

        // 이동 입력 (카메라 기준으로 변환)
        Vector2 input = Input.MoveInput;
        Vector3 moveDir = GetCameraBasedDirection(input);

        // 로컬 좌표로 변환 (Animator용)
        Vector3 localMove = transform.InverseTransformDirection(moveDir);

        Anim.SetFloat("Horizontal", localMove.x);
        Anim.SetFloat("Vertical", localMove.z);

        // 땅 체크
        Anim.SetBool("IsGrounded", CheckIfGrounded());

        /*float currentSpeed = Rb.velocity.magnitude;
        float normalizedSpeed = currentSpeed / Stats.movement.runSpeed;
        Anim.SetFloat("Speed", normalizedSpeed);*/
    }
    #endregion

    #region Actions (Called by States)
    public void Move(float speed)
    {
        Vector2 input = Input.MoveInput;
        Vector3 moveDir = GetCameraBasedDirection(input);

        Vector3 targetVel = moveDir * speed;
        Vector3 currentVel = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);

        float accel = (input.magnitude > 0)
            ? Stats.movement.acceleration
            : Stats.movement.deceleration;

        Vector3 newVel = Vector3.MoveTowards(
            currentVel, targetVel, accel * Time.fixedDeltaTime
        );

        Rb.velocity = new Vector3(newVel.x, Rb.velocity.y, newVel.z);

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

    public void Jump(float force)
    {
        Rb.velocity = new Vector3(Rb.velocity.x, 0, Rb.velocity.z);
        Rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    public bool CheckIfGrounded()
    {
        return Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            1.5f,
            Stats.detection.groundLayer
        );
    }

    public void SetGrounderWeight(float weight)
    {
        if (Grounder != null)
        {
            Grounder.weight = weight;
        }
    }
    #endregion

    #region Helper Methods
    public Vector3 GetCameraBasedDirection(Vector2 input)
    {
        if (CameraTransform == null)
        {
            return (transform.forward * input.y + transform.right * input.x).normalized;
        }

        Vector3 forward = CameraTransform.forward;
        Vector3 right = CameraTransform.right;

        forward.y = 0;
        right.y = 0;

        return (forward.normalized * input.y + right.normalized * input.x).normalized;
    }

    public void SetIKWeight(float weight)
    {
        if (IKSolver != null)
        {
            IKSolver.solver.IKPositionWeight = weight;
        }
    }
    #endregion
}