using UnityEngine;

public class PlayerState
{
    protected PlayerController player; 
    protected StateMachine stateMachine; 
    protected string animBoolName; // 애니메이션 파라미터 이름

    // 생성자
    public PlayerState(PlayerController player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    // [가상 함수] 자식들이 내용을 채워넣을 빈 껍데기들

    public virtual void Enter()
    {
        if (player.Anim != null) player.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit()
    {
        if (player.Anim != null) player.Anim.SetBool(animBoolName, false);
    }

    public virtual void LogicUpdate() { }  
    public virtual void PhysicsUpdate() { }
}