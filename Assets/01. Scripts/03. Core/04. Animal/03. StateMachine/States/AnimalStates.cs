using UnityEngine;

namespace AnimalStates
{
    public class Eat : BaseState<AIController>
    {
        AnimalStateMachine animalStateMachine;
        public override void EnterState(AIController animal)
        {
            animalStateMachine = animal.StateMachine as AnimalStateMachine;
            animal.SetAnimBool(animal.HashEat, true);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (animalStateMachine.hunger <= 80)
                return animal.StateMachine.IdleState;

            animalStateMachine.hunger += Time.deltaTime;
            return this;
        }
    }

    public class Poo : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Sleep : BaseState<AIController>
    {
        public override void EnterState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override void ExitState(AIController animal)
        {
            throw new System.NotImplementedException();
        }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            throw new System.NotImplementedException();
        }
    }
}