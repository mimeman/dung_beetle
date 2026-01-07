public interface Animal
{
    // Animal States 동물본능 (먹싸자)
    public abstract AnimalBaseState<AIController> EatState { get; }
    public abstract AnimalBaseState<AIController> PooState { get; }
    public abstract AnimalBaseState<AIController> SleepState { get; }
}

public interface Breedable
{
    public abstract AnimalBaseState<AIController> BreedState { get; }   // 짝짓기 상태.
    public abstract AnimalBaseState<AIController> FeedState { get; }    // 육아 상태.
}

public interface Flyable
{
    public abstract AnimalBaseState<AIController> FlyState { get; }
}

public interface Swimable
{
    public abstract AnimalBaseState<AIController> SwimState { get; }
}

public interface Housed
{
    public abstract AnimalBaseState<AIController> MakeHomeState { get; }    // 알맞는 서식지를 찾으면 집을 만드는 느낌쓰
}