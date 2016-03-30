using ECS.Base;
using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Components.Product
{
  public enum AttributeType
  {

  }

  public class Attribute
  {
    public AttributeType Type;
    public string Value;
  }

  public class Attributes : BaseComponent
  {
    public Attributes(IEntity parent) : base(parent) { parent.Set<Attributes>(this); }

    public List<Attribute> AttributeValues { get; set; }
  }
}
