using UnityEngine;

public class AnimalStateMachine : BaseStateMachine, IAnimal
{
    public float hunger;
    public float poo;
    public float energy;

    // Basic States 기본상태들
    public override BaseState<AnimalController> IdleState { get; }        // Idle 상태
    public override BaseState<AnimalController> PatrolState { get; }      // 필요한 요소를 찾아 탐색
    public override BaseState<AnimalController> TraceState { get; }       // 필요한 요소를 찾았다면 요소를 쫓아감
    public override BaseState<AnimalController> InteractState { get; }    // 요소와의 상호작용
    public override BaseState<AnimalController> AttackState { get; }      // 전투를 해야하는 상태
    public override BaseState<AnimalController> HitState { get; }         // 전투에서 맞았을때
    public override BaseState<AnimalController> DieState { get; }         // 죽었을때.

    public BaseState<AnimalController> EatState { get; }
    public BaseState<AnimalController> PooState { get; }
    public BaseState<AnimalController> SleepState { get; }
}