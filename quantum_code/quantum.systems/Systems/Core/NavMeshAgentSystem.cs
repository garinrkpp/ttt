using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum.Core
{
  public unsafe class NavMeshAgentSystem : SystemBase, INavMeshCallbacks
  {
    public void OnTargetReached(NavMeshUpdater.NavMeshEntity e)
    {
      _f.Signals.OnNavMeshTargetReached(e.Agent, (Entity*)e.Entity);
    }

    private Frame _f;
    public override void Update(Frame f)
    {
      _f = f;
      Profiler.Start("NavMeshAgent System");
      f.NavMeshUpdater.Update(f.DeltaTime, f.Number, this);
      Profiler.End();
      _f = null;
    }
  }
}
