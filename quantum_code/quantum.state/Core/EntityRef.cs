using System;
using System.Runtime.InteropServices;

namespace Quantum {
  [StructLayout(LayoutKind.Sequential, Pack = Core.CodeGenConstants.STRUCT_PACK)]
  public unsafe struct EntityRef : IEquatable<EntityRef> {
    internal Int32 _index;
    internal Int32 _version;
    internal EntityTypes _type;

    public static readonly EntityRef None = default(EntityRef);

    public EntityTypes Type {
      get {
        return _type;
      }
    }

    public Boolean Equals(EntityRef other) {
      return other._index == this._index && other._version == this._version && other._type == this._type;
    }

    public override Int32 GetHashCode() {
      unchecked {
        var hash = 17;
        hash = hash * 31 + _index;
        hash = hash * 31 + _version;
        hash = hash * 31 + (Int32)_type;
        return hash;
      }
    }

    public override Boolean Equals(Object obj) {
      if (obj is EntityRef) {
        return this.Equals((EntityRef)obj);
      }

      return false;
    }

    public override String ToString() {
      return String.Format("[EntityRef Type:{0} Index:{1}]", _type, _index);
    }

    public static Boolean operator ==(EntityRef a, EntityRef b) {
      return a._type == b._type && a._index == b._index &&  a._version == b._version;
    }

    public static Boolean operator !=(EntityRef a, EntityRef b) {
      return a._type != b._type || a._index != b._index || a._version != b._version;
    }
  }
}
