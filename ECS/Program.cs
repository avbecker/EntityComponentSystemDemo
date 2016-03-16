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
        }

        public void Go()
        {
            Command cmd = ReadCommand();
            while (cmd.Type != CommandType.Exit)
            {
                Thread workThread = new Thread(new ThreadStart(cmd.Do));
                workThread.Start();
                while (!workThread.IsAlive);

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
                } else { workThread.Join(); }

                cmd = ReadCommand();
            }
        }

        Command ReadCommand()
        {
            Console.Write("\rECS > ");
            Console.Out.Flush();
            var cmd = Console.ReadLine();
            return new Command(cmd, Data);
        }

    }

    enum CommandType
    {
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

        private string _cmd;
        private List<IEntity> _entities;
        private ISystem _system;

        public Command(string commandstring, List<IEntity> entities)
        {
            _cmd = commandstring;
            _entities = entities;
            var split = _cmd.Split(' ');

            if (split.Length == 0)
            {
                Type = CommandType.Empty;
                return;
            }

            switch(split[0].ToLower())
            {
                case "exit": Type = CommandType.Exit; break;
                case "system": Type = CommandType.System; break;
                case "list": Type = CommandType.List; break;
                default: Type = CommandType.Unknown; break;
            }

            if (Type == CommandType.Empty || Type == CommandType.Unknown || Type == CommandType.List)
            {
                HasOutput = true;
            }

            if (Type == CommandType.System)
            {
              if (split.Length > 1)
              {
                try
                {
                  Type t = System.Type.GetType("ECS.Systems." + split[1]);
                  _system = (ISystem)Activator.CreateInstance(t);
                }
                catch
                {
                  HasOutput = true;
                }
              }
              else
              {
                HasOutput = true;
              }
            }
        }

        public void Do()
        {
            if (Type == CommandType.Empty || Type == CommandType.Unknown)
            {
                Console.WriteLine("Invalid Command");
            }

            if (Type == CommandType.System)
            {
                if (_system == null)
                {
                    Console.WriteLine("System Not Found");
                }
                else
                {
                    _system.DoWork(_entities);
                }
            }

            if (Type == CommandType.List)
            {
              foreach (var item in _entities)
              {
                Console.WriteLine(item.ToString());
              }
            }

            Thread.Sleep(100);
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
