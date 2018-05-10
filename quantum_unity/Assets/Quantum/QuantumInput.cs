using System;
using UnityEngine;

public abstract class QuantumInput : MonoBehaviour {
  static QuantumInput _instance;
  static public QuantumInput Instance {
    get {
      return _instance;
    }
  }

  protected virtual void Awake() {
    if (_instance) {
      Debug.LogErrorFormat("Duplicate instances of QuantumInput behaviour found, using latest attached to {0}", gameObject.name);
    }

    _instance = this;
  }

  public abstract Photon.Deterministic.Tuple<Quantum.Input, Photon.Deterministic.DeterministicInputFlags> PollInput(Int32 player);
}
