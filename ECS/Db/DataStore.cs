using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ECS.Base;

namespace ECS.Db
{
  public class DataStore<T> where T: IEntity, new()
  {
    private static string connString = "Data Source=DIRACT-036; Initial Catalog=zig; User Id=bami;Password=bami;MultipleActiveResultSets=true";

    private T _template;
    private Dictionary<Type, String> dataMapper
    {
      get
      {
        // Add the rest of your CLR Types to SQL Types mapping here
        Dictionary<Type, String> dataMapper = new Dictionary<Type, string>();
        dataMapper.Add(typeof(int), "BIGINT");
        dataMapper.Add(typeof(string), "NVARCHAR(500)");
        dataMapper.Add(typeof(bool), "BIT");
        dataMapper.Add(typeof(DateTime), "DATETIME");
        dataMapper.Add(typeof(float), "FLOAT");
        dataMapper.Add(typeof(decimal), "DECIMAL(18,6)");
        dataMapper.Add(typeof(Guid), "UNIQUEIDENTIFIER");

        return dataMapper;
      }
    }

    public DataStore(T template)
    {
      _template = template;
    }

    public void CreateTables() 
    {
      var components = _template.List();

      var fields = new List<KeyValuePair<String, Type>>();
      var foreignkeys = new List<KeyValuePair<String, Type>>();

      foreach (var component in components)
      {
        if (!typeof(IComponent).IsAssignableFrom(component))
          continue;

        foreach (var p in component.GetProperties())
        {
          if (typeof(IEntity).IsAssignableFrom(p.PropertyType))
            continue;

          if (p.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null && p.PropertyType != typeof(string))
          {
            foreignkeys.Add(new KeyValuePair<string, Type>(p.Name, p.PropertyType));
            continue;
          }

          fields.Add(new KeyValuePair<String, Type>(component.Name+"."+p.Name, p.PropertyType));
        }
      }

      System.Text.StringBuilder script = new StringBuilder();
      script.AppendLine("CREATE TABLE " + typeof(T).Name);
      script.AppendLine("(");
      script.AppendLine("\t ID INT,");
      script.AppendLine("\t CONSTRAINT PK_" + typeof(T).Name + "_ID PRIMARY KEY CLUSTERED (ID),");
      for (int i = 0; i < fields.Count; i++)
      {
        KeyValuePair<String, Type> field = fields[i];

        if (dataMapper.ContainsKey(field.Value))
        {
          script.Append("\t [" + field.Key + "] " + dataMapper[field.Value]);
        }
        else
        {
          if (field.Value.IsEnum)
          {
            script.Append("\t [" + field.Key + "] INT");
          }
          else
          {
            // Complex Type? 
            script.Append("\t [" + field.Key + "] NVARCHAR(500)");
          }
        }

        if (i != fields.Count - 1)
        {
          script.Append(",");
        }

        script.Append(Environment.NewLine);
      }
      script.AppendLine(")");
      script.AppendLine("GO");
      // components with lists are foreign key objects

      script.Append(Environment.NewLine);

      for (int i = 0; i < foreignkeys.Count; i++)
      {
        var foreign = foreignkeys[i];
        script.AppendLine("CREATE TABLE " + typeof(T).Name + foreign.Key);
        script.AppendLine("(");
        script.AppendLine("\t ID INT,");
        script.AppendLine("\t CONSTRAINT PK_" + foreign.Key + "_ID PRIMARY KEY CLUSTERED (ID),");
        script.AppendLine("\t " + typeof(T).Name + "ID INT,");

        var properties = foreign.Value.GetProperties();

        for (var j = 0; j < properties.Length;j++ )
        {
          var p = properties[j];
          var t = p.PropertyType;
          if (typeof(IEntity).IsAssignableFrom(t) ||
              typeof(IComponent).IsAssignableFrom(t))
            continue;

          if (dataMapper.ContainsKey(p.PropertyType))
          {
            script.Append("\t " + p.Name + " " + dataMapper[t]);
            if (j != properties.Length - 1)
            {
              script.Append(",");
            }
            script.Append(Environment.NewLine);
          }
        }
        script.AppendLine(")");
        script.AppendLine("GO");
        script.AppendLine("ALTER TABLE " + typeof(T).Name + foreign.Key + " WITH NOCHECK");
        script.AppendLine("ADD CONSTRAINT FK_" + typeof(T).Name + "_" + foreign.Key + " FOREIGN KEY (" + typeof(T).Name + "ID) REFERENCES " + typeof(T).Name + "(ID)");
        script.AppendLine("GO");

      }

      DoSql(script.ToString());
    }

    public IEnumerable<T> GetAll()
    {
      var components = _template.List();
      var result = new List<T>();

      using (var conn = new SqlConnection(connString))
      {
        var res = conn.Query<dynamic>("SELECT * FROM "+ typeof(T).Name);
        foreach (var row in res)
        {
          var dat = (IDictionary<string, object>)row;
          var entity = new T();
          entity.ID = row.ID;

          foreach (var type in components)
          {
            var comp = Activator.CreateInstance(type, entity);
            if (!typeof(IComponent).IsAssignableFrom(type))
              continue;

            foreach (var p in type.GetProperties())
            {
              if (!dat.ContainsKey(type.Name+"."+p.Name))
                continue;

              p.SetValue(comp, dat[p.Name]);
            }
          }

          result.Add(entity);
        }
      }
      return result;
    }

    public void Write(IEnumerable<IEntity> items)
    {
      var components = _template.List();

      var fields = new List<KeyValuePair<String, Type>>();
      var foreignkeys = new List<KeyValuePair<String, Type>>();

      foreach (var component in components)
      {
        if (!typeof(IComponent).IsAssignableFrom(component))
          continue;

        foreach (var p in component.GetProperties())
        {
          if (typeof(IEntity).IsAssignableFrom(p.PropertyType))
            continue;

          if (p.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null && p.PropertyType != typeof(string))
          {
            foreignkeys.Add(new KeyValuePair<string, Type>(p.Name, p.PropertyType));
            continue;
          }

          fields.Add(new KeyValuePair<String, Type>(component.Name + "." + p.Name, p.PropertyType));
        }
      }

      var dbTable = typeof(T).Name;
      var dbFields = "([ID]," + string.Join(",", fields.Select(c => "[" + c.Key + "]")) + ")";

      using (var conn = new SqlConnection(connString))
      {

      
      foreach (var item in items)
      {
        List<object> values = new List<object>();
        values.Add(item.ID);
        foreach (var comp in components)
        {
          var elem = item.GetComponents().FirstOrDefault(c => c.GetType() == comp);
          if (elem == null)
          {
            foreach (var prop in comp.GetProperties())
            {
              if (typeof(IEntity).IsAssignableFrom(prop.PropertyType))
                continue;

              if (prop.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) == null || prop.PropertyType == typeof(string))
              {
                values.Add(null);
              }
            }
          }
          else
          {
            foreach (var prop in elem.GetType().GetProperties())
            {
              if (typeof(IEntity).IsAssignableFrom(prop.PropertyType))
                continue;

              if (prop.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) == null || prop.PropertyType == typeof(string))
              {
                values.Add(prop.GetValue(elem));
              }
            }
          }
        }

        

        var sql = string.Format("INSERT INTO {0} {1} VALUES @vals",dbTable, dbFields);
        conn.Execute(sql, new { vals = values });
      }
      }
    }

    private void DoSql(string sql)
    {
      using (var conn = new SqlConnection(connString))
      {
        conn.Open();
        var cmds = sql.Split(new string[] {"GO"},StringSplitOptions.RemoveEmptyEntries);
        foreach (var cmd in cmds)
        {
          conn.Execute(cmd);
        }
      }
    }
  }
}
