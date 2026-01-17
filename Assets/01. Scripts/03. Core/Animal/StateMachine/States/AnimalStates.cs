using UnityEngine;

namespace AnimalStates
{
    public class Eat : BaseState<AIController>
    {
        AnimalStateMachine animalStateMachine;
        public override void EnterState(AIController animal)
        {
            animalStateMachine = animal.stateMachine as AnimalStateMachine;
            animal.SetAnimBool(animal.hashEat, true);
        }

        public override void ExitState(AIController animal) { }

        public override BaseState<AIController> UpdateState(AIController animal)
        {
            if (animalStateMachine.hunger <= 80)
                return animal.stateMachine.IdleState;

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