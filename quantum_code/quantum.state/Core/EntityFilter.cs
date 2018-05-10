using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quantum {
  [StructLayout(LayoutKind.Sequential, Pack = Core.CodeGenConstants.STRUCT_PACK)]
  public unsafe struct EntityFilter {
    public Entity* Entity;
  }
}
