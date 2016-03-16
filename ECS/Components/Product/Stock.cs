using ECS.Base;
using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Components.Product
{
  public class Stock : BaseComponent
  {
    public Stock(IEntity parent) : base(parent) { parent.Set<Stock>(this); }

    public int InStock { get; set; }
  }
}
