using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quantum {
  public unsafe interface ISignalOnEntityCreated {
    Int32 RuntimeIndex { get; }
    void OnEntityCreated(Frame f, Entity* entity);
  }

  public unsafe interface ISignalOnEntityDestroy {
    Int32 RuntimeIndex { get; }
    void OnEntityDestroy(Frame f, Entity* entity);
  }

  [StructLayout(LayoutKind.Sequential, Pack = Core.CodeGenConstants.STRUCT_PACK)]
  public partial struct Entity {
    internal EntityRef _ref;
    internal Boolean _active;

    public EntityTypes Type {
      get {
        return _ref._type;
      }
    }

    public EntityRef EntityRef {
      get { return _ref; }
    }
  }
}
