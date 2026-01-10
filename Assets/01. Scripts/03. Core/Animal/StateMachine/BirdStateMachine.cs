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
    public override BaseState<AnimalController> IdleState => idleState;
    public override BaseState<AnimalController> PatrolState => patrolState;
    public override BaseState<AnimalController> TraceState => traceState;
    public override BaseState<AnimalController> InteractState => interactState;
    public override BaseState<AnimalController> AttackState => attackState;
    public override BaseState<AnimalController> HitState => hitState;
    public override BaseState<AnimalController> DieState => dieState;

    private readonly Fly flyState = new Fly();
    public BaseState<AnimalController> FlyState => flyState;

    private readonly Breed breedState = new Breed();
    private readonly Feed feedState = new Feed();
    public BaseState<AnimalController> BreedState => breedState;
    public BaseState<AnimalController> FeedState => feedState;
}