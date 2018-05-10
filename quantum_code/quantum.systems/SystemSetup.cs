using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum {
  public static class SystemSetup {
    public static SystemBase[] CreateSystems(RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
            return new SystemBase[] {
        // pre-defined core systems
        new Core.PhysicsSystemPre(),
        new Core.NavMeshAgentSystem(),

        // user systems go here
                new MySystem(),
                new BulletSystem(),
        // pre-defined core systems
        new Core.AnimatorSystem(),
      };
    }
  }
}
