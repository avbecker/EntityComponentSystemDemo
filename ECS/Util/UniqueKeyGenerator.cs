using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.Util
{
  static class UniqueKeyGenerator
  {
    private static int key = 0;
    private static object locker = new object();

    public static int GetKey()
    {
      lock (locker)
      {
        return UniqueKeyGenerator.key++;
      }
    }
  }
}
