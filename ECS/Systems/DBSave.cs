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
  class DBSave : ISystem
  {
    public void DoWork(IList<IEntity> set)
    {
      var ds = new DataStore<Product>(new Product());

      ds.Write(set);
    }
  }
}
