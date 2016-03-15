using ECS.Interfaces;

namespace ECS.Base
{
    public class BaseComponent : IComponent
    {
        private IEntity _parent;

        public IEntity Parent
        {
            get
            {
                return _parent;
            }

            private set
            {
                _parent = value;
            }
        }

        public BaseComponent(IEntity parent)
        {
            _parent = parent;
        }
    }
}
