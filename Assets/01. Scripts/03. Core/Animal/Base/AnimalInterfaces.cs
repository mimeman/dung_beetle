public interface IAnimal
{
    // Animal States 동물본능 (먹싸자)
    public abstract BaseState<AnimalController> EatState { get; }
    public abstract BaseState<AnimalController> PooState { get; }
    public abstract BaseState<AnimalController> SleepState { get; }
}

public interface IBreedable
{
    public abstract BaseState<AnimalController> BreedState { get; }   // 짝짓기 상태.
    public abstract BaseState<AnimalController> FeedState { get; }    // 육아 상태.
}

public interface IBird
{
    public abstract BaseState<AnimalController> FlyState { get; }
}

public interface ISwimable
{
    public abstract BaseState<AnimalController> SwimState { get; }
}

public interface IHoused
{
    public abstract BaseState<AnimalController> BuildHomeState { get; }    // 알맞는 서식지를 찾으면 집을 만드는 느낌쓰
}