using System;
using System.Collections.Generic;
using System.Threading;
using ECS.Interfaces;

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
}