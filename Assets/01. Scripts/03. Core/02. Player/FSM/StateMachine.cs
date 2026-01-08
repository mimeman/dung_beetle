using UnityEngine;

public class StateMachine
{
    // 현재 작동 중인 상태
    public PlayerState CurrentState { get; private set; }

    // (초기 상태 설정)
    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter(); 
    }

    //  상태 교체 
    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();     
        CurrentState = newState; 
        CurrentState.Enter();    
    }
}