using AIStates;
using BirdStates;
using BreedStates;
using UnityEngine;

public class BirdStateMachine : AnimalStateMachine, IFlyable, IBreedable
{
    [SerializeField] private bool isFlying;

    private readonly Idle idleState = new Idle();
    private readonly Patrol patrolState = new Patrol();
    private readonly Trace traceState = new Trace();
    private readonly Interact interactState = new Interact();
    private readonly Attack attackState = new Attack();
    private readonly Hit hitState = new Hit();
    private readonly Die dieState = new Die();

    public override AnimalBaseState<AIController> IdleState => idleState;
    public override AnimalBaseState<AIController> PatrolState => patrolState;
    public override AnimalBaseState<AIController> TraceState => traceState;
    public override AnimalBaseState<AIController> InteractState => interactState;
    public override AnimalBaseState<AIController> AttackState => attackState;
    public override AnimalBaseState<AIController> HitState => hitState;
    public override AnimalBaseState<AIController> DieState => dieState;

    private readonly Fly flyState = new Fly();
    private readonly Breed breedState = new Breed();
    private readonly Feed feedState = new Feed();

    public AnimalBaseState<AIController> FlyState => flyState;
    public AnimalBaseState<AIController> BreedState => breedState;
    public AnimalBaseState<AIController> FeedState => feedState;
}