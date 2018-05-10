using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Deterministic;
using System;

[CreateAssetMenu(menuName = "Quantum/Configurations/Deterministic")]
public class DeterministicSessionConfigAsset : ScriptableObject {
  public DeterministicSessionConfig Config;

  public static DeterministicSessionConfigAsset Instance {
    get {
      return Resources.Load<DeterministicSessionConfigAsset>("DeterministicConfig");
    }
  }
}
