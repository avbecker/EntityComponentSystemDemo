using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ECS.Util
{
  public class XmlAutoMapper
  {
    private string _idField;
    private bool _isElement;
    private Dictionary<Type, Dictionary<string, string>> _componentMappings;

    public XmlAutoMapper(string idField, bool isElement)
    {
      _idField = idField;
      _isElement = isElement;

      _componentMappings = new Dictionary<Type, Dictionary<string, string>>();
    }

    public void Bind<T>(Dictionary<string, string> propertyPaths) where T : IComponent
    {
      if (_componentMappings.ContainsKey(typeof(T)))
        return;

      if (propertyPaths.Keys.Any(c => !typeof(T).GetProperties().Select(d => d.Name).Contains(c)))
      {
        return;
      }

      _componentMappings.Add(typeof(T), propertyPaths);
    }

    public T Map<T>(XElement xml) where T : IEntity, new()
    {
      var entity = new T();
      if (_isElement)
      {
        entity.ID = int.Parse(xml.Element(_idField).Value);
      }
      else {
        entity.ID = int.Parse(xml.Attribute(_idField).Value);
      }

      foreach (var type in _componentMappings.Keys)
      {
        if (!typeof(IComponent).IsAssignableFrom(type))
        {
          // this really shouldn't happen
          continue;
        }

        var component = Activator.CreateInstance(type, entity);

        var properties = _componentMappings[type];

        foreach (var propertyName in properties.Keys)
        {
          var property = type.GetProperties().FirstOrDefault(c => c.Name == propertyName);
          if (property == null)
            continue;

          var path = properties[propertyName].Split('/');
          var lastNode = path[path.Length - 1].Split('>');
          string attribute = null;
          if (lastNode.Length == 2)
          {
            path[path.Length - 1] = lastNode[0];
            attribute = lastNode[1];
          }

          var elem = xml;
          for (int i = 0; i < path.Length; i++)
          {
            elem = elem.Element(path[i]);
            if (elem == null)
            {
              break;
            }
          }
          string val = null;
          if (!string.IsNullOrEmpty(attribute))
          {
            var attr = elem.Attribute(attribute);
            if (attr == null)
            {
              continue;
            }
            val = attr.Value;
          }
          else {
            if (elem == null)
            {
              continue;
            }
            val = elem.Value;
          }
          try
          {
            if (property.PropertyType == typeof(string))
            {
              property.SetValue(component, val);
            }
            else if (property.PropertyType == typeof(decimal))
            {
              property.SetValue(component, decimal.Parse(val, CultureInfo.InvariantCulture));
            }
            else if (property.PropertyType == typeof(int))
            {
              property.SetValue(component, int.Parse(val));
            }
          }
          catch
          {
            //  parse fail
          }
        }
      }
      return entity;
    }
  }
}
