using ECS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ECS
{
  public class ECSDEMO
  {
    public List<IEntity> Data;

    public ECSDEMO()
    {
      Data = new List<IEntity>();
      Console.WriteLine("Welcome to Entity Component System demo application");
      Console.WriteLine("Use command \"help\" for instructions");
      Thread.Sleep(500);
    }

    public void Go()
    {
      Command cmd = ReadCommand();
      while (cmd.Type != CommandType.Exit)
      {
        Thread workThread = new Thread(new ThreadStart(cmd.Do));
        workThread.Start();
        while (!workThread.IsAlive) ;

        if (!cmd.HasOutput)
        {
          Console.Write("Working  ");
          int counter = 0;
          while (!workThread.Join(100))
          {
            counter = (counter + 1) % 4;
            switch (counter)
            {
              case 0: Console.Write("\b/"); break;
              case 1: Console.Write("\b-"); break;
              case 2: Console.Write("\b\\"); break;
              case 3: Console.Write("\b|"); break;
            }
          }
          Console.WriteLine("\rDone!                   ");
        }
        else { workThread.Join(); }

        cmd = ReadCommand();
      }
    }

    Command ReadCommand()
    {
      Console.WriteLine();
      Console.Write("\rECS > ");
      Console.Out.Flush();
      var cmd = Console.ReadLine();
      return new Command(cmd, Data);
    }

  }

  enum CommandType
  {
    Help,
    List,
    System,
    Empty,
    Exit,
    Unknown
  }

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

      if (split.Length == 0)
      {
        Type = CommandType.Empty;
        return;
      }

      if (!Enum.TryParse(split[0], true, out Type)) {
        Type = CommandType.Unknown;
      }

      if (split.Length > 1) {
        _commandArguments = split.Skip(1).ToArray();
      }

      if (Type == CommandType.Empty || Type == CommandType.Unknown || Type == CommandType.List || Type == CommandType.Help)
      {
        HasOutput = true;
      }

      if (Type == CommandType.System)
      {
        HasOutput = true;
        if (_commandArguments != null && _commandArguments.Length > 0)
        {
          Type t = GetSystems().FirstOrDefault(c => c.Name.ToLower() == _commandArguments[0].ToLower());
          if (t != null) {
            HasOutput = false;
            _system = (ISystem)Activator.CreateInstance(t);
          }
        } else {
          Type = CommandType.Unknown;
        }
      }
    }

    public void Do()
    {
      switch(Type) {
        case CommandType.System: DoSystem(); break;
        case CommandType.List: DoList(); break;
        case CommandType.Help: DoHelp(); break;
        default: DoUnknown(); break;
      }
      Thread.Sleep(100);
    }

    private void DoList() {
      var sb = new StringBuilder();
      int count = 0;
      foreach (var item in _entities)
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

    private void DoUnknown() {
      Console.WriteLine("Invalid Command");
    }

    private void DoSystem() {
      if (_system == null)
      {
        Console.WriteLine("System Not Found");
      }
      else
      {
        _system.DoWork(_entities);
      }
    }

    private void DoHelp() {
      if (_commandArguments == null || _commandArguments.Length == 0 || string.IsNullOrEmpty(_commandArguments[0])) {
        Console.WriteLine("Following commands available:");
        Console.WriteLine(string.Join(", ",Enum.GetNames(typeof(CommandType))));
        Console.WriteLine("Use \"help (command)\" for more information");

        return;
      }

      switch(_commandArguments[0].ToLower()) {
        case "system": {
            Console.WriteLine("Following systems available:");
            Console.WriteLine(string.Join(", ", GetSystems().Select(c => c.Name)));
            Console.WriteLine("Activate a system by the command \"system (name)\"");
        }break;
        default: Console.WriteLine("No help available"); break;
      }
      
    }

    private void DoClear() {
      _entities.Clear();
    }

    private IEnumerable<Type> GetSystems() {
      var type = typeof(ISystem);
      return AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
          .Where(p => type.IsAssignableFrom(p) && p != typeof(ISystem));
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      var demo = new ECSDEMO();
      demo.Go();
    }
  }
}
