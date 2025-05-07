namespace Munglo.Commons
{
    public interface IFood : IConsumable
    {
        public string Name { get; }
        public bool Eat(AIObject obj);
        public bool IsCooked { get; }
        public float TimeToEat { get; }
        public float UnCookedValue { get; }
        public float CookedValue { get; }

        public float Efficiancy { get => TimeToEat / (IsCooked ? CookedValue : UnCookedValue); }
    }
}
