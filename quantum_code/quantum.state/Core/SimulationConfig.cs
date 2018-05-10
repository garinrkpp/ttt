using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantum {
  [Serializable]
  public partial class SimulationConfig {

    [Serializable]
    public class SimulationConfigPhysics {
      public FPVector2 Gravity;
      public Int32 SolverIterations;
      public Int32 Substeps;
      public Boolean UseAngularVelocity;
      public Boolean RaiseCollisionEventsForStatics;
      public PhysicsMaterial DefaultPhysicsMaterial;
    }

    [Serializable]
    public class SimulationConfigNavMeshAgent
    {
      public Int32 ProximityFactor;
      public Int32 UpdateInterval;
      public NavMeshAgentConfig DefaultNavMeshAgent;
    }

    public SimulationConfigPhysics Physics;
    public SimulationConfigNavMeshAgent NavMeshAgent;
    public Boolean AutoLoadSceneFromMap;
  }
}
