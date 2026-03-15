public enum AnimalStateID
{
    Idle,
    Eat,
    Poo,
    Sleep,
    Breed,
    Fly,
    Swim
}

public interface IAnimal
{
    // Animal States 동물본능 (먹싸자)
    public abstract BaseState<AIController> EatState { get; }
    public abstract BaseState<AIController> PooState { get; }
    public abstract BaseState<AIController> SleepState { get; }
}

public interface IBreedable
{
    public abstract BaseState<AIController> BreedState { get; }   // 짝짓기 상태.
    public abstract BaseState<AIController> FeedState { get; }    // 육아 상태.
}

public interface IBird
{
    public abstract BaseState<AIController> FlyState { get; }
}

public interface ISwimable
{
    public abstract BaseState<AIController> SwimState { get; }
}

public interface IHoused
{
    public abstract BaseState<AIController> BuildHomeState { get; }    // 알맞는 서식지를 찾으면 집을 만드는 느낌쓰
}