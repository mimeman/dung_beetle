using UnityEngine;

public abstract class BaseStateMachine : MonoBehaviour
{
    // Basic States 기본상태들
    public abstract BaseState<AnimalController> IdleState { get; }        // Idle 상태
    public abstract BaseState<AnimalController> PatrolState { get; }      // 필요한 요소를 찾아 탐색
    public abstract BaseState<AnimalController> TraceState { get; }       // 필요한 요소를 찾았다면 요소를 쫓아감
    public abstract BaseState<AnimalController> InteractState { get; }    // 요소와의 상호작용
    public abstract BaseState<AnimalController> AttackState { get; }      // 전투를 해야하는 상태
    public abstract BaseState<AnimalController> HitState { get; }         // 전투에서 맞았을때
    public abstract BaseState<AnimalController> DieState { get; }         // 죽었을때.
}