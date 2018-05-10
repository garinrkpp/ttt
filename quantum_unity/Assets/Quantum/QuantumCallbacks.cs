using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

public abstract class QuantumCallbacks : MonoBehaviour {
  public static readonly List<QuantumCallbacks> Instances = new List<QuantumCallbacks>();

  protected void OnEnable() {
    Instances.Add(this);
  }

  protected void OnDisable() {
    Instances.Remove(this);
  }

  public virtual void OnUpdateView() {

  }

  public virtual void OnGameStart() {

  }

  public virtual void OnMapChangeBegin() {

  }

  public virtual void OnMapChangeDone() {

  }

  public virtual void OnChecksumError(DeterministicTickChecksumError error, Frame[] frames) {

  }
}
