using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;
    protected StateMachine stateMachine;
    protected string animBoolName;

    public PlayerState(PlayerController player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        if (player.Anim != null && !string.IsNullOrEmpty(animBoolName))
            player.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit()
    {
        if (player.Anim != null && !string.IsNullOrEmpty(animBoolName))
            player.Anim.SetBool(animBoolName, false);
    }

    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
}