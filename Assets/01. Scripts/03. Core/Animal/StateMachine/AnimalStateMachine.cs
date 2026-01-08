using UnityEngine;

public class AnimalStateMachine : BaseStateMachine, IAnimal
{
    // Basic States 기본상태들
    public override AnimalBaseState<AIController> IdleState { get; }        // Idle 상태
    public override AnimalBaseState<AIController> PatrolState { get; }      // 필요한 요소를 찾아 탐색
    public override AnimalBaseState<AIController> TraceState { get; }       // 필요한 요소를 찾았다면 요소를 쫓아감
    public override AnimalBaseState<AIController> InteractState { get; }    // 요소와의 상호작용
    public override AnimalBaseState<AIController> AttackState { get; }      // 전투를 해야하는 상태
    public override AnimalBaseState<AIController> HitState { get; }         // 전투에서 맞았을때
    public override AnimalBaseState<AIController> DieState { get; }         // 죽었을때.

    public AnimalBaseState<AIController> EatState { get; }
    public AnimalBaseState<AIController> PooState { get; }
    public AnimalBaseState<AIController> SleepState { get; }
}