using System;

namespace Quantum {
  [Serializable]
  public partial class RuntimeConfig : Photon.Deterministic.DeterministicRuntimeConfig {
    public MapLink Map;
    public RuntimePlayer[] Players;

    public override int MaxPlayers {
      get { return Players != null ? Players.Length : 0; }
      set { throw new InvalidOperationException(); }
    }
  }
}
