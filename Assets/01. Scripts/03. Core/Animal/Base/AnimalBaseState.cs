using UnityEngine;

public abstract class AnimalBaseState<T> where T : class
{
    // Init
    public abstract void EnterState(T animal);

    // Update
    public abstract AnimalBaseState<T> UpdateState(T animal);

    // Exit
    public abstract void ExitState(T animal);
}