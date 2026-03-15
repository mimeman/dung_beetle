using UnityEngine;

public abstract class BaseState<T> where T : class
{
    // Init
    public abstract void EnterState(T animal);

    // Update
    public abstract BaseState<T> UpdateState(T animal);

    // Exit
    public abstract void ExitState(T animal);
}