using ECS.Base;
using ECS.Interfaces;

namespace ECS.Components.Product
{
  public class Vendor : BaseComponent
  {
    public Vendor(IEntity parent) : base(parent) { parent.Set<Vendor>(this); }

    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
  }
}
