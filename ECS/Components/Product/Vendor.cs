using ECS.Base;
using ECS.Interfaces;

namespace ECS.Components.Product
{
  public class Vendor : BaseComponent
  {
    public Vendor(IEntity parent) : base(parent) { parent.Set<Vendor>(this); }

    public string VendorName { get; set; }
    public string Vendordingdong { get; set; }
  }
}
