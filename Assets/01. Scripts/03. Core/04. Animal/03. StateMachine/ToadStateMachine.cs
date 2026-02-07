using ToadStates;
using AIStates;
using UnityEngine;

public class ToadStateMachine : AnimalStateMachine
{
    public ToadStates.Idle idleState = new ToadStates.Idle();
    public Aiming aimingState = new Aiming();
    public Snap snapState = new Snap();
    public Pull pullState = new Pull();
    public Bite biteState = new Bite();
    public Stuck stuckState = new Stuck();
    public Recover recoverState = new Recover();
    public Cooldown cooldownState = new Cooldown();
    public Hit hitState = new Hit();
    public Die dieState = new Die();

    public override BaseState<AIController> IdleState => idleState;
    public BaseState<AIController> AimingState => aimingState;
    public BaseState<AIController> SnapState => snapState;
    public BaseState<AIController> PullState => pullState;
    public BaseState<AIController> BiteState => biteState;
    public BaseState<AIController> StuckState => stuckState;
    public BaseState<AIController> RecoverState => recoverState;
    public BaseState<AIController> CooldownState => cooldownState;
    public override BaseState<AIController> HitState => hitState;
    public override BaseState<AIController> DieState => dieState;
}
