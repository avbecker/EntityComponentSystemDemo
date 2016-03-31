namespace ECS.Interfaces
{
  public interface IComponent
  {
    IEntity Parent { get; }
    void Remove();
  }
}
