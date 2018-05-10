using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum
{
  public unsafe interface ISignalOnNavMeshTargetReached
  {
    Int32 RuntimeIndex { get; }
    void OnNavMeshTargetReached(Frame f, Core.NavMeshAgent* agent,  Entity* entity);
  }
}
