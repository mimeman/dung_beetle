using UnityEngine;

namespace AnimalStates
{
    public class Eat : BaseState<AnimalController>
    {
        AnimalStateMachine animalStateMachine;
        public override void EnterState(AnimalController animal)
        {
            animalStateMachine = animal.stateMachine as AnimalStateMachine;
            animal.SetAnimBool(animal.hashEat, true);
        }

        public override void ExitState(AnimalController animal) { }

        public override BaseState<AnimalController> UpdateState(AnimalController animal)
        {
            if (animalStateMachine.hunger <= 80)
                return animal.stateMachine.IdleState;

            animalStateMachine.hunger += Time.deltaTime;
            return this;
        }
    }

    public class Poo : BaseState<AnimalController>
    {
        public override void EnterState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }

        public override BaseState<AnimalController> UpdateState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Sleep : BaseState<AnimalController>
    {
        public override void EnterState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }

        public override BaseState<AnimalController> UpdateState(AnimalController animal)
        {
            throw new System.NotImplementedException();
        }
    }
}