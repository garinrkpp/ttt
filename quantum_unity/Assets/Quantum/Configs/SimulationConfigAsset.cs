using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/Configurations/Simulation")]
public class SimulationConfigAsset : ScriptableObject {
  public Quantum.SimulationConfig Configuration;

  public static SimulationConfigAsset Instance {
    get {
      return Resources.Load<SimulationConfigAsset>("SimulationConfig");
    }
  }
}
