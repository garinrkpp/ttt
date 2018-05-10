using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum.Core {
  public class AnimatorSystem : SystemBase {
    public override void Update(Frame f) {
      f.AnimatorUpdater.Update(f.DeltaTime);
    }
  }
}
