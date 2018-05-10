using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefabAsset : AssetBase {
  [NonSerialized]
  public Quantum.EntityPrefab Settings;

  [NonSerialized]
  public EntityPrefabRoot Prefab;

  public override Quantum.AssetObject AssetObject {
    get {
      return Settings;
    }
  }
}
