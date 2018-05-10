using UnityEngine;

public abstract class AssetBase : ScriptableObject {
  public abstract Quantum.AssetObject AssetObject {
    get;
  }

  public virtual void Loaded() {

  }
}
