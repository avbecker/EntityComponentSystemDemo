using ECS.Base;
using ECS.Interfaces;

namespace ECS.Components.Product
{
    public class Description : BaseComponent
    {
        public Description(IEntity parent) : base(parent){ }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }

        public string StockLabel { get; set; }
    }
}
