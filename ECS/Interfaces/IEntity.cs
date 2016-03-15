namespace ECS.Interfaces
{
    public interface IEntity
    {
        int ID { get; }
        T Get<T>() where T : IComponent;
    }
}
