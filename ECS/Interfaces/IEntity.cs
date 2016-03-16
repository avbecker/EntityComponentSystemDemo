namespace ECS.Interfaces
{
  public interface IEntity
  {
    int ID { get; set; }
    T Get<T>() where T : IComponent;
    void Set<T>(T component) where T : IComponent;
    bool Has<T>() where T : IComponent;
  }
}
