using ECS.Base;
using ECS.Components.Product;
using ECS.Util;
using System.Globalization;
using System.Xml.Linq;

namespace ECS.Entities
{
  public class Product : BaseEntity
  {
    public Product()
    {
      ID = UniqueKeyGenerator.GetKey();
      new Description(this);
      new Media(this);
      new StockLocation(this);
      new Price(this);
    }
  }
}
