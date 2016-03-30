using System;
using System.Collections.Generic;
namespace ECS.Interfaces
{
  public interface IEntity
  {
    int ID { get; set; }
    Type[] List();
    IComponent[] GetComponents();
    T Get<T>() where T : IComponent;
    void Set<T>(T component) where T : IComponent;
    bool Has<T>() where T : IComponent;
  }
}
