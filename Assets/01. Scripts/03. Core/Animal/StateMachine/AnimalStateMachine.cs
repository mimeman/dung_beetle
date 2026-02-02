using UnityEngine;
using AIStates;

public class AnimalStateMachine : BaseStateMachine, IAnimal
{
    public float hunger;
    public float poo;
    public float energy;

    private readonly Idle idleState = new Idle();
    private readonly Patrol patrolState = new Patrol();
    private readonly Trace traceState = new Trace();
    private readonly Interact interactState = new Interact();
    private readonly Attack attackState = new Attack();
    private readonly Hit hitState = new Hit();
    private readonly Die dieState = new Die();
    public override BaseState<AIController> IdleState => idleState;         // Idle 상태
    public override BaseState<AIController> PatrolState => patrolState;     // 필요한 요소를 찾아 탐색
    public override BaseState<AIController> TraceState => traceState;       // 필요한 요소를 찾았다면 요소를 쫓아감
    public override BaseState<AIController> InteractState => interactState; // 요소와의 상호작용
    public override BaseState<AIController> AttackState => attackState;     // 전투를 해야하는 상태
    public override BaseState<AIController> HitState => hitState;           // 전투에서 맞았을때
    public override BaseState<AIController> DieState => dieState;           // 죽었을때.

    public BaseState<AIController> EatState { get; }
    public BaseState<AIController> PooState { get; }
    public BaseState<AIController> SleepState { get; }
}