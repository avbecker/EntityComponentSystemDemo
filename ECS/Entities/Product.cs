using ECS.Base;
using ECS.Components.Product;

namespace ECS.Entities
{
    public class Product: BaseEntity
    {
        public Product()
        {
            Set(new Description(this));
            Set(new Media(this));
        }
    }
}
