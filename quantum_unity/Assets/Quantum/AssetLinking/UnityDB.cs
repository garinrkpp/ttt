using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public static class UnityDB {
  static AssetBase[] _byIndex;
  static Dictionary<String, AssetBase> _byGuid;

  static String[] GetUnityLayerNameArray() {
    var layers = new String[32];

    for (Int32 i = 0; i < layers.Length; ++i) {
      try {
        layers[i] = UnityEngine.LayerMask.LayerToName(i);
      }
      catch {
        // just eat exceptions
      }
    }

    return layers;
  }

  static Int32[] GetUnityLayerMatrix() {
    var matrix = new Int32[32];

    for (Int32 a = 0; a < 32; ++a) {
      for (Int32 b = 0; b < 32; ++b) {
        if (Physics.GetIgnoreLayerCollision(a, b) == false) {
          matrix[a] |= (1 << b);
          matrix[b] |= (1 << a);
        }
      }
    }

    return matrix;
  }

  public static event Action<List<AssetBase>> OnAssetLoad;

  public static IEnumerable<AssetBase> All {
    get { return _byIndex; }
  }

  public static IEnumerable<T> AllOf<T>() where T : class {
    return _byIndex.Select(x => x as T).Where(x => x != null);
  }

  public static void Init(Boolean force = false) {
    if (_byIndex != null && force == false) {
      return;
    }

    // init runtime
    QuantumRunner.Init();

    // init layers
    Quantum.Layers.Init(GetUnityLayerNameArray(), GetUnityLayerMatrix());

    _byIndex = LoadAll();
    _byGuid = new Dictionary<String, AssetBase>();

    foreach (var asset in _byIndex) {
      if (asset) {
        _byGuid.Add(asset.AssetObject.Guid, asset);
      }
    }

    List<AssetObject> navMeshes = new List<AssetObject>();
        int nextId = _byIndex.Length;
    foreach (var asset in _byIndex) {
      if (asset) {
        try {
          asset.Loaded();
          var map = asset as MapAsset;
          if (map != null)
          {
            foreach(var navMesh in map.Settings.NavMeshes.Values)
            {
              navMesh.Id = nextId++;
              navMesh.Guid = "NavMesh" + navMesh.Id;
              navMeshes.Add(navMesh);
            }
          }

        }
        catch (Exception exn) {
          Log.Exception(exn);
        }
      }
    }

    var data = _byIndex.MapRef(x => x.AssetObject);
    data = data.Concat<AssetObject>(navMeshes).ToArray<AssetObject>();
    DB.Init(data);
    Debug.Log("Quantum Asset Database Loaded");
  }

  public static T FindAsset<T>(AssetObject asset) where T : AssetBase {
    if (asset == null) {
      return default(T);
    }

    return FindAsset<T>(asset.Id);
  }

  public static T FindAsset<T>(String guid) where T : AssetBase {
    Assert.Check(_byGuid != null);

    AssetBase value;

    if (guid != null && _byGuid.TryGetValue(guid, out value)) {
      return value as T;
    }

    return null;
  }

  public static T FindAsset<T>(Int32 index) where T : AssetBase {
    Assert.Check(_byIndex != null);

    if (index >= 0 && index < _byIndex.Length) {
      return _byIndex[index] as T;
    }

    return default(T);
  }

  public static AssetBase FindAsset(String guid) {
    Assert.Check(_byGuid != null);

    AssetBase value;

    if (guid != null && _byGuid.TryGetValue(guid, out value)) {
      return value;
    }

    return null;
  }

  public static AssetBase FindAsset(Int32 index) {
    Assert.Check(_byIndex != null);

    if (index >= 0 && index < _byIndex.Length) {
      return _byIndex[index];
    }

    return null;
  }

  static AssetBase[] LoadAll() {
    var all = UnityEngine.Resources.LoadAll<AssetBase>("DB").ToList();

    // load entity prefabs
    var prefabList = EntityPrefabsListAsset.Instance;
    if (prefabList && prefabList.Prefabs != null) {
      foreach (var prefabItem in prefabList.Prefabs) {
        if (prefabItem.Prefab) {
          EntityPrefabAsset prefab;
          prefab = ScriptableObject.CreateInstance<EntityPrefabAsset>();
          prefab.Settings = new EntityPrefab();
          prefab.Settings.Guid = prefabItem.Path;
          prefab.Prefab = prefabItem.Prefab;

          all.Add(prefab);
        }
      }
    }

    // add default physics material
    {
      PhysicsMaterialAsset material;

      material = ScriptableObject.CreateInstance<PhysicsMaterialAsset>();
      material.Settings = SimulationConfigAsset.Instance.Configuration.Physics.DefaultPhysicsMaterial;
      material.Settings.Guid = Quantum.PhysicsMaterial.DEFAULT_ID;

      all.Add(material);
    }

    // add default physics material
    {
      NavMeshAgentConfigAsset agent;

      agent = ScriptableObject.CreateInstance<NavMeshAgentConfigAsset>();
      agent.Settings = SimulationConfigAsset.Instance.Configuration.NavMeshAgent.DefaultNavMeshAgent;
      agent.Settings.Guid = Quantum.NavMeshAgentConfig.DEFAULT_ID;

      all.Add(agent);
    }

        // call OnAssetLoad if callback exists
        // to allow user code to modify assets
        // before they are loaded into quantum
        if (OnAssetLoad != null) {
      OnAssetLoad(all);
    }

    // make sure we only have valid guids
    for (Int32 i = all.Count - 1; i >= 0; --i) {
      if (AssetObjectIdentifier.IsGuidValid(all[i].AssetObject.Guid) == false) {
        // log error
        Debug.LogErrorFormat("Asset '{0}' does not have a valid guid '{1}'. Asset guids have to be a non-zero length string which only contains ASCII characters. Asset '{0}' will not be loaded.", all[i].name, all[i].AssetObject.Guid);

        // remove this asset from the load table
        all.RemoveAt(i);
      }
    }

    // check for duplicate guids
    foreach (var group in all.GroupBy(x => x.AssetObject.Guid)) {
      if (group.Count() > 1) {
        // log error
        Debug.LogErrorFormat("Assets '{0}' share the same guid '{1}', this is not allowed. Assets '{0}' will not be loaded.", String.Join("', '", group.Select(x => x.name).ToArray()), group.Key);

        // remove from list
        foreach (var asset in group) {
          all.Remove(asset);
        }
      }
    }

    // sort based on guid
    all.Sort((a, b) => a.AssetObject.Guid.CompareTo(b.AssetObject.Guid));

    // create result
    var result = new AssetBase[all.Count + 1];

    // set ids (index + 1)
    for (Int32 i = 0; i < all.Count; ++i) {
      result[i + 1] = all[i];
      result[i + 1].AssetObject.Id = i + 1;
    }


    return result;
  }
}
