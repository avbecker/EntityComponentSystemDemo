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
  public class DataStore<T> where T : IEntity, new()
  {
    private static string connString = "Data Source=DIRACT-036; Initial Catalog=zig; User Id=bami;Password=bami;MultipleActiveResultSets=true";

    private T _template;
    private Dictionary<Type, String> dataMapper
    {
      get
      {
        // Add the rest of your CLR Types to SQL Types mapping here
        Dictionary<Type, String> dataMapper = new Dictionary<Type, string>();
        dataMapper.Add(typeof(int), "INT");
        dataMapper.Add(typeof(string), "NVARCHAR(500)");
        dataMapper.Add(typeof(bool), "BIT");
        dataMapper.Add(typeof(DateTime), "DATETIME");
        dataMapper.Add(typeof(float), "FLOAT");
        dataMapper.Add(typeof(decimal), "DECIMAL(18,6)");
        dataMapper.Add(typeof(Guid), "UNIQUEIDENTIFIER");

        return dataMapper;
      }
    }
    private Dictionary<Type, List<PropertyInfo>> mappableProperties;

    public DataStore(T template)
    {
      _template = template;

      mappableProperties = new Dictionary<Type, List<PropertyInfo>>();
      var components = _template.List();
      foreach (var type in components)
      {
        if (!typeof(IComponent).IsAssignableFrom(type))
          continue;
        mappableProperties.Add(type, new List<PropertyInfo>());

        foreach (var p in type.GetProperties())
        {
          if (typeof(IEntity).IsAssignableFrom(p.PropertyType))
            continue;

          if (p.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) == null || p.PropertyType == typeof(string))
          {
            mappableProperties[type].Add(p);
            continue;
          }
        }
      }
    }

    public void CreateTables()
    {
      var components = _template.List();

      var fields = new List<KeyValuePair<String, Type>>();
      var foreignkeys = new List<KeyValuePair<String, Type>>();

      foreach (var type in mappableProperties.Keys)
      {
        foreach (var p in mappableProperties[type])
        {
          if (p.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null && p.PropertyType != typeof(string))
          {
            foreignkeys.Add(new KeyValuePair<string, Type>(p.Name, p.PropertyType));
            continue;
          }
          fields.Add(new KeyValuePair<String, Type>(string.Format("{0}.{1}", type.Name, p.Name), p.PropertyType));
        }
      }

      System.Text.StringBuilder script = new StringBuilder();
      script.AppendLine(string.Format("CREATE TABLE {0}", typeof(T).Name));
      script.AppendLine("(");
      script.AppendLine("\t ID INT IDENTITY(1,1),");
      script.AppendLine(string.Format("\t CONSTRAINT PK_{0}_ID PRIMARY KEY CLUSTERED (ID),", typeof(T).Name));
      for (int i = 0; i < fields.Count; i++)
      {
        KeyValuePair<String, Type> field = fields[i];

        if (dataMapper.ContainsKey(field.Value))
        {
          script.Append(string.Format("\t [{0}] {1}", field.Key, dataMapper[field.Value]));
        }
        else
        {
          if (field.Value.IsEnum)
          {
            script.Append(string.Format("\t [{0}] INT", field.Key));
          }
          else
          {
            // Complex Type? 
            script.Append(string.Format("\t [{0}] NVARCHAR(500)", field.Key));
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
        script.AppendLine(string.Format("CREATE TABLE {0}{1}", typeof(T).Name, foreign.Key));
        script.AppendLine("(");
        script.AppendLine("\t ID INT IDENTITY(1,1),");
        script.AppendLine(string.Format("\t CONSTRAINT PK_{0}_ID PRIMARY KEY CLUSTERED (ID),", foreign.Key));
        script.AppendLine(string.Format("\t {0}ID INT,", typeof(T).Name));

        var properties = foreign.Value.GetProperties();

        for (var j = 0; j < properties.Length; j++)
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
      var result = new List<T>();

      using (var conn = new SqlConnection(connString))
      {
        conn.Open();
        var res = conn.Query<dynamic>("SELECT * FROM " + typeof(T).Name);
        foreach (var row in res)
        {
          var dat = (IDictionary<string, object>)row;
          var entity = new T();
          entity.ID = row.ID;

          foreach (var type in mappableProperties.Keys)
          {
            var comp = Activator.CreateInstance(type, entity);
            foreach (var p in mappableProperties[type])
            {
              p.SetValue(comp, dat[string.Format("{0}.{1}", type.Name, p.Name)]);
            }
          }

          result.Add(entity);
        }
        conn.Close();
      }
      return result;
    }

    public void Write(IEnumerable<IEntity> items)
    {
      //var fields = new List<KeyValuePair<String, Type>>();
      //var foreignkeys = new List<KeyValuePair<String, Type>>();

      var dbTable = typeof(T).Name;
      var dbFields = "";

      using (var conn = new SqlConnection(connString))
      {
        conn.Open();
        using (var tran = conn.BeginTransaction())
        {
          foreach (var item in items)
          {
            Dictionary<string, object> values = new Dictionary<string, object>();
            if (item.ID > 0)
            {
              values.Add("ID", item.ID);
            }

            foreach (var type in mappableProperties.Keys)
            {
              var elem = item.GetComponents().FirstOrDefault(c => c.GetType() == type);
              if (elem == null)
              {
                foreach (var prop in mappableProperties[type])
                {
                  values.Add(string.Format("{0}_{1}", type.Name, prop.Name), null);
                }
              }
              else
              {
                foreach (var prop in mappableProperties[type])
                {
                  if (prop.PropertyType.IsEnum)
                  {
                    values.Add(string.Format("{0}_{1}", type.Name, prop.Name), (int)prop.GetValue(elem));
                  }
                  else
                  {
                    values.Add(string.Format("{0}_{1}", type.Name, prop.Name), prop.GetValue(elem));
                  }
                }
              }
            }
            var sql = "";

            if (item.ID > 0)
            {
              dbFields = string.Join(",", values.Skip(1).Select(c => "[" + c.Key.Replace('_', '.') + "] = @" + c.Key + ""));
              sql = string.Format("UPDATE {0} SET {1} WHERE ID = @ID", dbTable, dbFields);
              conn.Execute(sql, values, tran);
            }
            else
            {
              dbFields = "(" + string.Join(",", values.Select(c => "[" + c.Key.Replace('_', '.') + "]")) + ")";
              var dbValues = "(" + string.Join(",", values.Select(c => "@" + c.Key + "")) + ")";
              sql = string.Format("INSERT INTO {0} {1} VALUES {2}; SELECT CAST(SCOPE_IDENTITY() as int)", dbTable, dbFields, dbValues);
              item.ID = conn.Query<int>(sql, values, tran).Single();
            }
          }
          tran.Commit();
        }
        conn.Close();
      }
    }

    public void Sync(IEnumerable<IEntity> items)
    {
      using (var conn = new SqlConnection(connString))
      {
        conn.Open();
        using (var trans = conn.BeginTransaction())
        {
          var query = "DELETE FROM " + typeof(T).Name + " WHERE ID NOT IN @ids";

          for (var i = 0; i <= items.Count(); i += 1000)
          {
            conn.Execute(query, new { ids = items.Skip(i).Take(1000).Select(c => c.ID).Distinct().ToList() }, trans);
          }
          trans.Commit();
        }
        conn.Close();
      }
      Write(items);
    }

    private void DoSql(string sql)
    {
      using (var conn = new SqlConnection(connString))
      {
        conn.Open();
        var cmds = sql.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var cmd in cmds)
        {
          conn.Execute(cmd);
        }
        conn.Close();
      }
    }
  }
}
