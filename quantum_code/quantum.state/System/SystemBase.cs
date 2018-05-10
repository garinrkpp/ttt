using System;

namespace Quantum {
  public abstract partial class SystemBase {
    Int32? _runtimeIndex;

    public Int32 RuntimeIndex {
      get { return _runtimeIndex.GetValueOrDefault(-1); }
      set {
        if (_runtimeIndex.HasValue) {
          Log.Error("Can't change systems runtime index after game has started");
        } else {
          _runtimeIndex = value;
        }
      }
    }

    public virtual Boolean StartEnabled {
      get { return true; }
    }

    public virtual void OnInit(Frame f) {

    }

    public virtual void OnEnabled(Frame f) {

    }

    public virtual void OnDisabled(Frame f) {

    }

    public abstract void Update(Frame f);
  }
}
