using Photon.Deterministic;
using Quantum;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class MapDataBaker {

  public static void BakeMapData(MapData data, Boolean inEditor) {
    FPMathUtils.LoadLookupTables();

    if (inEditor == false && !data.Asset) {
      data.Asset = ScriptableObject.CreateInstance<MapAsset>();
      data.Asset.Settings = new Quantum.Map();
    }

    BakeData(data, inEditor);
  }

  public static void BakeNavMeshes(MapData data, Boolean inEditor) {
    FPMathUtils.LoadLookupTables();

    data.Asset.NavMeshes = new TextAsset[0];

    var navmeshes = BakeNavMeshesLoop(data).ToArray();

    if (inEditor) {
#if UNITY_EDITOR
      var pathOnDisk = PathUtils.Combine('/', Path.GetDirectoryName(Application.dataPath), Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset)));
      var assetDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset));
      var assetPath = PathUtils.Combine('/', assetDir, Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(data.Asset)));

      foreach (var navmesh in navmeshes) {
        var navmeshFileOnDisk = data.Asset.name + "_" + navmesh.Name + ".bytes";
        var navmeshAssetPath = assetPath + "_" + navmesh.Name + ".bytes";

        // serialize (max 20 megabytes for now)
        var bytestream = new ByteStream(new Byte[1024 * 1024 * 20]);
        navmesh.Serialize(bytestream, true);

        // write
        File.WriteAllBytes(PathUtils.Combine('/', pathOnDisk, navmeshFileOnDisk), bytestream.ToArray());

        // import asset
        AssetDatabase.ImportAsset(navmeshAssetPath, ImportAssetOptions.ForceUpdate);

        // add assets to navmehs
        ArrayUtils.Add(ref data.Asset.NavMeshes, AssetDatabase.LoadAssetAtPath<TextAsset>(navmeshAssetPath));
      }
#endif
    }
    else {
      data.Asset.InitNavMeshes(navmeshes);
    }
  }

  static StaticColliderData GetStaticData(GameObject gameObject, QuantumStaticColliderSettings settings) {
    return new StaticColliderData {
      Asset = settings.Asset,
      Name = gameObject.name,
      Tag = gameObject.tag
    };
  }

  static void BakeData(MapData data, Boolean inEditor) {
#if UNITY_EDITOR
    if (inEditor) {
      if (EditorSceneManager.loadedSceneCount != 1) {
        Debug.LogErrorFormat("Can't bake map data when more than one scene is open.");
        return;
      }

      // set scene name
      data.Asset.Settings.Scene = EditorSceneManager.GetActiveScene().name;
    }
#endif

    // clear existing colliders
    data.Asset.Settings.StaticColliders = new MapStaticCollider[0];

    // circle colliders
    foreach (var collider in UnityEngine.Object.FindObjectsOfType<QuantumStaticCircleCollider2D>()) {
      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders, new MapStaticCollider {
        Position = collider.transform.position.ToFPVector2(),
        Rotation = collider.transform.rotation.ToFPRotation2D(),
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        Trigger = collider.Settings.Trigger,
        StaticData = GetStaticData(collider.gameObject, collider.Settings),
        Layer = collider.gameObject.layer,

        // circle
        ShapeType = Quantum.Core.DynamicShapeType.Circle,
        CircleRadius = FP.FromFloat_UNSAFE(collider.Radius.AsFloat * collider.transform.localScale.x)
      });
    }

    // polygon colliders
    foreach (var collider in UnityEngine.Object.FindObjectsOfType<QuantumStaticPolygonCollider2D>()) {
      var s = collider.transform.localScale;
      var vertices = collider.Vertices.Select(x => { var v = x.ToUnityVector3(); return new Vector3(v.x * s.x, v.y * s.y, v.z * s.z); }).Select(x => x.ToFPVector2()).ToArray();

      if (FPVector2.IsClockWise(vertices)) {
        FPVector2.MakeCounterClockWise(vertices);
      }

      var normals = FPVector2.CalculatePolygonNormals(vertices);

      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders, new MapStaticCollider {
        Position = collider.transform.position.ToFPVector2(),
        Rotation = collider.transform.rotation.ToFPRotation2D(),
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        Trigger = collider.Settings.Trigger,
        StaticData = GetStaticData(collider.gameObject, collider.Settings),
        Layer = collider.gameObject.layer,

        // polygon
        ShapeType = Quantum.Core.DynamicShapeType.Polygon,
        PolygonCollider = new PolygonCollider {
          Vertices = vertices,
          Normals = normals
        }
      });
    }

    // polygon colliders
    foreach (var collider in UnityEngine.Object.FindObjectsOfType<QuantumStaticBoxCollider2D>()) {
      var e = collider.Size.ToUnityVector3();
      var s = collider.transform.localScale;
      e.x *= s.x;
      e.y *= s.y;
      e.z *= s.z;

      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders, new MapStaticCollider {
        Position = collider.transform.position.ToFPVector2(),
        Rotation = collider.transform.rotation.ToFPRotation2D(),
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        Trigger = collider.Settings.Trigger,
        StaticData = GetStaticData(collider.gameObject, collider.Settings),
        Layer = collider.gameObject.layer,

        // polygon
        ShapeType = Quantum.Core.DynamicShapeType.Box,
        BoxExtents = e.ToFPVector2() * FP._0_50
      });
    }

    // invoke callbacks
    foreach (var callback in Quantum.TypeUtils.GetSubClasses(typeof(MapDataBakerCallback), "Assembly-CSharp", "Assembly-CSharp-firstpass", "Assembly-CSharp-Editor", "Assembly-CSharp-Editor-firstpass")) {
      if (callback.IsAbstract == false) {
        try {
          (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBake(data);
        }
        catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }

    if (inEditor) {
      Debug.LogFormat("Baked {0} static colliders", data.Asset.Settings.StaticColliders.Length);
    }
  }


  static IEnumerable<NavMesh> BakeNavMeshesLoop(MapData data) {
    foreach (var navmeshDefinition in data.GetComponentsInChildren<MapNavMeshDefinition>()) {
      var navmesh = default(NavMesh);

      try {
        navmesh = MapNavMeshBaker.BakeNavMesh(data, navmeshDefinition);
      }
      catch (Exception exn) {
        Log.Exception(exn);
      }

      if (navmesh != null) {
        yield return navmesh;
      }
      else {
        Log.Error("Nav Mesh '{0}' baking failed", navmeshDefinition.name);
      }
    }
  }

}
