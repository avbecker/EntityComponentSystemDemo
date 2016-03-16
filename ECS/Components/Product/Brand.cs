using ECS.Base;
using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Components.Product
{
  public class Brand : BaseComponent
  {
    public Brand(IEntity parent) : base(parent) { parent.Set<Brand>(this); }

    public int ID { get; set; }
    public int ParentID { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
  }
}
