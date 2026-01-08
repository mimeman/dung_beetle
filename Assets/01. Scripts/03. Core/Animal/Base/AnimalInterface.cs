public interface IAnimal
{
    // Animal States 동물본능 (먹싸자)
    public abstract AnimalBaseState<AIController> EatState { get; }
    public abstract AnimalBaseState<AIController> PooState { get; }
    public abstract AnimalBaseState<AIController> SleepState { get; }
}

public interface IBreedable
{
    public abstract AnimalBaseState<AIController> BreedState { get; }   // 짝짓기 상태.
    public abstract AnimalBaseState<AIController> FeedState { get; }    // 육아 상태.
}

public interface IFlyable
{
    public abstract AnimalBaseState<AIController> FlyState { get; }
}

public interface ISwimable
{
    public abstract AnimalBaseState<AIController> SwimState { get; }
}

public interface IHoused
{
    public abstract AnimalBaseState<AIController> BuildHomeState { get; }    // 알맞는 서식지를 찾으면 집을 만드는 느낌쓰
}