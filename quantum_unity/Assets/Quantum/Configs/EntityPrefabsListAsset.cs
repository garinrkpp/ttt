using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefabsListAsset : ScriptableObject {
  [Serializable]
  public class EntityPrefabItem {
    public String Path;
    public EntityPrefabRoot Prefab;
  }

  public EntityPrefabItem[] Prefabs;

  public static EntityPrefabsListAsset Instance {
    get {
      return Resources.Load<EntityPrefabsListAsset>("EntityPrefabsList");
    }
  }
}
