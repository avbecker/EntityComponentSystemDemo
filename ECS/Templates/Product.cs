using ECS.Base;
using ECS.Components.Product;
using ECS.Util;
using System.Globalization;
using System.Xml.Linq;

namespace ECS.Templates
{
  public class Product : BaseEntity
  {
    public Product()
    {
      new Description(this);
      new Media(this);
      new StockLocation(this);
      new Price(this);
      new Attributes(this);
    }
  }
}
