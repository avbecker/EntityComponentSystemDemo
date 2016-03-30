using ECS.Db;
using ECS.Interfaces;
using ECS.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Systems
{
  public class DBLoad : ISystem
  {
    public void DoWork(IList<IEntity> set)
    {
      var ds = new DataStore<Product>(new Product());

      set.Clear();
      var data = ds.GetAll();

      foreach (var item in data)
      {
        set.Add(item);
      }
    }
  }
}
