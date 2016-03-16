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
    public static int GetKey()
    {
      return UniqueKeyGenerator.key++;
    }
  }
}
