using BaseAnimalStates;
using BirdStates;
using AIBaseStates;
using UnityEngine;

public class BirdStateMachine : AnimalStateMachine, Flyable, Breedable, Housed
{
    [SerializeField] private bool isFlying;

    private readonly Idle idleState = new Idle();
    private readonly Patrol patrolState = new Patrol();
    private readonly Trace traceState = new Trace();
    private readonly Interact interactState = new Interact();
    private readonly Attack attackState = new Attack();
    private readonly Hit hitState = new Hit();
    private readonly Die dieState = new Die();
    private readonly Eat eatState = new Eat();
    private readonly Poo pooState = new Poo();
    private readonly Sleep sleepState = new Sleep();
    private readonly Fly flyState = new Fly();

    public override AnimalBaseState<AIController> IdleState => idleState;
    public override AnimalBaseState<AIController> PatrolState => patrolState;
    public override AnimalBaseState<AIController> TraceState => traceState;
    public override AnimalBaseState<AIController> InteractState => interactState;
    public override AnimalBaseState<AIController> AttackState => attackState;
    public override AnimalBaseState<AIController> HitState => hitState;
    public override AnimalBaseState<AIController> DieState => dieState;
    public override AnimalBaseState<AIController> EatState => eatState;
    public override AnimalBaseState<AIController> PooState => pooState;
    public override AnimalBaseState<AIController> SleepState => sleepState;
    public AnimalBaseState<AIController> FlyState => flyState;
    public AnimalBaseState<AIController> BreedState => throw new System.NotImplementedException();
    public AnimalBaseState<AIController> FeedState => throw new System.NotImplementedException();
    public AnimalBaseState<AIController> MakeHomeState => throw new System.NotImplementedException();
}