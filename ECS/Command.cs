using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ECS.Interfaces;

namespace ECS
{
  class Command
  {
    public CommandType Type;
    public bool HasOutput;

    private string[] _commandArguments;
    private List<IEntity> _entities;
    private ISystem _system;

    public Command(string commandstring, List<IEntity> entities)
    {
      _entities = entities;
      var split = commandstring.Split(' ');
      _commandArguments = new string[] { };

      if (split.Length == 0)
      {
        Type = CommandType._Empty;
        return;
      }

      if (!Enum.TryParse(split[0], true, out Type))
      {
        Type = CommandType._Unknown;
      }

      if (split.Length > 1)
      {
        _commandArguments = split.Skip(1).ToArray();
      }

      if (Type == CommandType._Empty || 
          Type == CommandType._Unknown || 
          Type == CommandType.List || 
          Type == CommandType.Help ||
          Type == CommandType.Count)
      {
        HasOutput = true;
      }

      if (Type == CommandType.System)
      {
        HasOutput = true;
        if (_commandArguments.Length > 0)
        {
          Type t = GetSystems().FirstOrDefault(c => c.Name.ToLower() == _commandArguments[0].ToLower());
          if (t != null)
          {
            HasOutput = false;
            _system = (ISystem)Activator.CreateInstance(t);
          }
        }
      }
    }

    public void Do()
    {
      switch (Type)
      {
        case CommandType.System: DoSystem(); break;
        case CommandType.List: DoList(); break;
        case CommandType.Help: DoHelp(); break;
        case CommandType.Clear: DoClear(); break;
        case CommandType.Count: DoCount(); break;
        default: DoUnknown(); break;
      }
      Thread.Sleep(100);
    }

    private void DoList()
    {
      var sb = new StringBuilder();
      int count = 0;
      int skip = 0;
      int take = int.MaxValue;

      if (_commandArguments.Length > 0) 
      {
        int.TryParse(_commandArguments[0],out take);
        if (_commandArguments.Length > 1) {
          int.TryParse(_commandArguments[1], out skip);
        }
      }

      foreach (var item in _entities.Skip(skip).Take(take))
      {
        sb.Append(string.Format("{0}\n", item.ToString()));
        count++;
        if (count > 1000)
        {
          Console.WriteLine(sb);
          count = 0;
          sb.Clear();
        }
      }
      Console.WriteLine(sb);
      Console.WriteLine(string.Format("{0} entities", _entities.Count()));
    }

    private void DoUnknown()
    {
      Console.WriteLine("Invalid Command");
    }

    private void DoSystem()
    {
      if (_system == null)
      {
        Console.WriteLine("System not found, loaded systems are:");
        Console.WriteLine();
        Console.WriteLine(string.Join(", ", GetSystems().Select(c => c.Name)));
      }
      else
      {
        _system.DoWork(_entities);
      }
    }

    private void DoHelp()
    {
      if (_commandArguments == null || _commandArguments.Length == 0 || string.IsNullOrEmpty(_commandArguments[0]))
      {
        Console.WriteLine("Following commands available:");
        Console.WriteLine(string.Join(", ", Enum.GetNames(typeof(CommandType)).Where(c=>c[0] != '_')));
        Console.WriteLine("Use \"help (command)\" for more information");

        return;
      }

      switch (_commandArguments[0].ToLower())
      {
        case "help":
        {
          Console.WriteLine("derp");
        }
          break;
        case "exit":
        {
          Console.WriteLine("Exits the application");
        }
          break;
        case "system":
        {
          Console.WriteLine("Following systems available:");
          Console.WriteLine(string.Join(", ", GetSystems().Select(c => c.Name)));
          Console.WriteLine("Activate a system by the command \"system (name)\"");
        }
          break;
        case "clear":
        {
          Console.WriteLine("Clears the entity storage");
        }
          break;
        case "list":
        {
          Console.WriteLine("Prints entities in storage. Usage:");
          Console.WriteLine("list (take) (skip)");
        }
          break;
        default: Console.WriteLine("No help available"); break;
      }

    }

    private void DoClear()
    {
      _entities.Clear();
    }

    private void DoCount() {
      Console.WriteLine(string.Format("{0} entities", _entities.Count()));
    }

    private IEnumerable<Type> GetSystems()
    {
      var type = typeof(ISystem);
      return AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Where(p => type.IsAssignableFrom(p) && p != typeof(ISystem));
    }
  }
}