using ECS.Base;
using ECS.Components.Product;
using ECS.Interfaces;
using ECS.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ECS.Systems
{
  class ConcentratorXmlLoader : ISystem
  {
    public void DoWork(IList<IEntity> set)
    {
      var data = XElement.Load(@"Data/247Assortment.xml");

      var mapper = new XmlAutoMapper("ProductID", false);

      mapper.Bind<Description>(
        new Dictionary<string, string>
        {
            {"ShortDescription",  "Content>ShortDescription"},
            {"LongDescription",   "Content>LongDescription"},
            {"Name",              "Content>ProductName" }
        });

      mapper.Bind<Price>(
        new Dictionary<string, string>
        {
            {"UnitPrice", "Price/UnitPrice"},
            {"CostPrice", "Price/CostPrice"}
        });

      mapper.Bind<Stock>(
        new Dictionary<string, string>
        {
            {"InStock", "Stock>InStock"}
        });

      mapper.Bind<Brand>(
        new Dictionary<string, string>
        {
            {"ID", "Brands/Brand>BrandID"},
            {"ParentID", "Brands/Brand>ParentBrandID"},
            {"Name", "Brands/Brand/Name"},
            {"Code", "Brands/Brand/Code"},
        });

      foreach (var productElem in data.Elements("Product"))
      {
        set.Add(mapper.Map<BaseEntity>(productElem));
      }
    }
  }
}