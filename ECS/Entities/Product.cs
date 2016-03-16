using ECS.Base;
using ECS.Components.Product;
using ECS.Util;

namespace ECS.Entities
{
  public class Product : BaseEntity
  {
    public Product()
    {
      ID = UniqueKeyGenerator.GetKey();
      Set(new Description(this));
      Set(new Media(this));
      Set(new StockLocation(this));
      Set(new Price(this));
    }
  }
}
