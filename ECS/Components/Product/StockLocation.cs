using ECS.Base;
using ECS.Interfaces;

namespace ECS.Components.Product
{
  public enum StockLocationType
  {
    None = 0,
    Sellable = 1,
    Reserved = 2,
    Damaged = 4,
    Demo = 8,
    Repairs = 16,
    Auction = 32,
    Returns = 64,
    Transit = 128
  }

  public class StockLocation : BaseComponent
  {
    public StockLocation(IEntity parent) : base(parent) { }

    public StockLocationType Type { get; set; }
  }
}
