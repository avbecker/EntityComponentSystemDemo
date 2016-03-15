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
    }
}
