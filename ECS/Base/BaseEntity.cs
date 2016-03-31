using ECS.Interfaces;
using System;
using System.Collections.Generic;

namespace ECS.Base
{
  public class BaseEntity : IEntity
  {
    public int ID { get; set; }

    protected Dictionary<Type, IComponent> _components;

    public BaseEntity()
    {
      _components = new Dictionary<Type, IComponent>();
    }

    public T Get<T>() where T : IComponent
    {
      if (_components.ContainsKey(typeof(T)))
      {
        return (T)_components[typeof(T)];
      }
      return default(T);
    }

    public void Set<T>(T component) where T : IComponent
    {
        if (_components.ContainsKey(typeof(T))) {
          _components[typeof(T)].Remove();
        }

        _components[typeof(T)] = (IComponent)component;
    }

    public Type[] List()
    {
      Type[] keys = new Type[_components.Keys.Count];
      _components.Keys.CopyTo(keys,0);
      return keys;
    }

    public override string ToString()
    {
      var components = new List<string>();
      foreach (var type in _components.Keys)
      {
        var comp = _components[type];
        var properties = new List<string>();
        foreach (var prop in type.GetProperties())
        {
          if (prop.PropertyType == typeof(IEntity))
          {
            continue; // otherwise infinite loops 
          }
          var val = prop.GetValue(comp);

          if (prop.PropertyType == typeof(string)) {
            properties.Add(string.Format("\n\t\t{0} = \"{1}\"", prop.Name, val));
          } else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(decimal)) {
            properties.Add(string.Format("\n\t\t{0} = {1}", prop.Name, val));
          } else {
            properties.Add(string.Format("\n\t\t{0} = \"{1}\"", prop.Name, val));
          }
          
        }

        components.Add(string.Format("\n\t{0} = {{ {1} \n\t}}", type.Name, string.Join(", ", properties)));
      }

      var result = string.Format("Entity:{{\n\tID = {0}, {1} \n}}", ID, string.Join(", ", components));
      return result;
    }

    public bool Has<T>() where T : IComponent
    {
      return _components.ContainsKey(typeof(T));
    }


    public IComponent[] GetComponents()
    {
      IComponent[] values = new IComponent[_components.Values.Count];
      _components.Values.CopyTo(values, 0);
      return values;
    }
  }
}
