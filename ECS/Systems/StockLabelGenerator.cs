using ECS.Components.Product;
using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS.Systems
{
  public class StockLabelGenerator : ISystem
  {
    public void DoWork(IList<IEntity> set)
    {
      var workset = set.Where(c => c.Has<Description>()).ToList();

      foreach (var entity in workset)
      {
        var price = entity.Get<Price>(); var description = entity.Get<Description>(); var stocklocation = entity.Get<StockLocation>();
        if (description == null)
          continue;

        var label = string.Format("{0}", description.Name);
        if (price != null)
        {
          label = string.Format("{0}, voor maar {1:C}", label, price.UnitPrice);
        }

        if (stocklocation != null)
        {
          if (stocklocation.Type.HasFlag(StockLocationType.Damaged))
          {
            label = string.Format("Licht beschadigd: {0}", label);
          }
        }
        description.StockLabel = label;
      }
    }
  }
}
