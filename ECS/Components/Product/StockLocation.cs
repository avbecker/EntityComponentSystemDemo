using ECS.Base;
using ECS.Interfaces;

namespace ECS.Components.Product
{
    public class StockLocation : BaseComponent
    {
        public StockLocation(IEntity parent) : base(parent) { }
    }
}
