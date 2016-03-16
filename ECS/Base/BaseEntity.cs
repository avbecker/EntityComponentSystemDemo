using ECS.Interfaces;
using System;
using System.Collections.Generic;

namespace ECS.Base
{
    public class BaseEntity : IEntity
    {
        public int ID {get; protected set;}

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

        protected void Set<T>(T component)
        {
            if (!_components.ContainsKey(typeof(T)))
            {
                _components.Add(typeof(T), (IComponent)component);
            }
            else
            {
                _components[typeof(T)] = (IComponent)component;
            }
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
              if (prop.PropertyType == typeof(ECS.Interfaces.IEntity))
              {
                continue;
              }
              properties.Add(string.Format("\n\t\t{0} = \"{1}\"",prop.Name,prop.GetValue(comp)));
            }

            components.Add(string.Format("\n\t{0} = {{ {1} }}", type.Name, string.Join(", ", properties)));
          }

          var result = string.Format("Product:{{\n\tID = \"{0}\", {1} }}", ID, string.Join(", ", components));
          return result;
        }
    }
}
