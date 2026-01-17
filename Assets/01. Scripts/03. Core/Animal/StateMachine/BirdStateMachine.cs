using AIStates;
using BirdStates;
using BreedStates;
using UnityEngine;

public class BirdStateMachine : AnimalStateMachine, IBird, IBreedable
{
    [SerializeField] private bool isFlying;

    private readonly Idle idleState = new Idle();
    private readonly Patrol patrolState = new Patrol();
    private readonly Trace traceState = new Trace();
    private readonly Interact interactState = new Interact();
    private readonly Attack attackState = new Attack();
    private readonly Hit hitState = new Hit();
    private readonly Die dieState = new Die();
    public override BaseState<AIController> IdleState => idleState;
    public override BaseState<AIController> PatrolState => patrolState;
    public override BaseState<AIController> TraceState => traceState;
    public override BaseState<AIController> InteractState => interactState;
    public override BaseState<AIController> AttackState => attackState;
    public override BaseState<AIController> HitState => hitState;
    public override BaseState<AIController> DieState => dieState;

    private readonly Fly flyState = new Fly();
    public BaseState<AIController> FlyState => flyState;

    private readonly Breed breedState = new Breed();
    private readonly Feed feedState = new Feed();
    public BaseState<AIController> BreedState => breedState;
    public BaseState<AIController> FeedState => feedState;
}