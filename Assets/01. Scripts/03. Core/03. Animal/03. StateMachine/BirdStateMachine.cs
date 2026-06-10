using AIStates;
using BirdStates;
using UnityEngine;

public class BirdStateMachine : AnimalStateMachine
{
    [SerializeField] private bool isFlying;

    private readonly FlyIdle idleState = new FlyIdle();
    private readonly FlyPatrol flyPatrol = new FlyPatrol();
    private readonly FlyStalking flyStalking = new FlyStalking();
    private readonly FlyDive flyDive = new FlyDive();
    private readonly FlyAscent flyAscent = new FlyAscent();
    private readonly FlyPerch flyPerch = new FlyPerch();
    private readonly FlyScatter flyScatter = new FlyScatter();
    private readonly Hit hitState = new Hit();
    private readonly Die dieState = new Die();

    public override BaseState<AIController> IdleState => idleState;
    public override BaseState<AIController> PatrolState => flyPatrol;
    public BaseState<AIController> StalkingState => flyStalking;
    public BaseState<AIController> DiveState => flyDive;
    public BaseState<AIController> AscentState => flyAscent;
    public BaseState<AIController> PerchState => flyPerch;
    public BaseState<AIController> ScatterState => flyScatter;
    public override BaseState<AIController> HitState => hitState;
    public override BaseState<AIController> DieState => dieState;
}