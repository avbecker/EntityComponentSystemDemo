using ECS.Components.Product;
using ECS.Entities;
using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Systems
{
  class ProductsGenerator : ISystem
  {
    public void DoWork(IList<IEntity> set)
    {
      var random = new Random();
      for(var i=0;i<1000000;i++) {
        var product = new Product();

        product.Get<Description>().Name = string.Format("derp {0}", random.Next(1000));
        product.Get<Price>().UnitPrice = (decimal)(random.NextDouble() * 1000f);
        product.Get<StockLocation>().Type = (StockLocationType)random.Next(255);
        set.Add(product);
      }
    }
  }
}
