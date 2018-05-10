using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum {
  partial class Frame {
    public partial class FrameEvents {
      Frame _f;

      public FrameEvents(Frame f) {
        _f = f;
      }
    }
  }
}
