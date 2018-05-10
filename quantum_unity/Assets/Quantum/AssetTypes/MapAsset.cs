using Quantum;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/Assets/Map")]
public partial class MapAsset : AssetBase {
  public Map Settings;
  public TextAsset[] NavMeshes;

  public override AssetObject AssetObject {
    get { return Settings; }
  }

  public override void Loaded() {
    base.Loaded();

    if (NavMeshes != null) {
      InitNavMeshes(DeserializeNavMeshes(NavMeshes).ToArray());
    }

    // last resort just so that Settings.NavMeshes never is null
    if (Settings.NavMeshes == null) {
      Settings.NavMeshes = new Dictionary<String, NavMesh>();
    }
  }

  public void InitNavMeshes(NavMesh[] navmeshes) {
    if (Settings.NavMeshes == null) {
      Settings.NavMeshes = new Dictionary<String, NavMesh>();
    }

    foreach (var navmesh in navmeshes) {
      navmesh.Init(Settings);

      Log.Info("Loaded NavMesh {0}:{1}", name, navmesh.Name);

      Settings.NavMeshes.Add(navmesh.Name, navmesh);
    }
  }

  IEnumerable<NavMesh> DeserializeNavMeshes(TextAsset[] navmeshes) {
    foreach (var navmeshData in navmeshes) {
      var stream = new ByteStream(navmeshData.bytes);
      var navmesh = new NavMesh();

      navmesh.Serialize(stream, false);

      yield return navmesh;
    }
  }
}
