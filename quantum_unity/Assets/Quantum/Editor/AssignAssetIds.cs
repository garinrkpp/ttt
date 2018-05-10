using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Quantum.Editor {
  public static class AssignAssetIds {
    static String NewGuid() {
      return Guid.NewGuid().ToString().ToLowerInvariant();
    }

    [MenuItem("Quantum/Refresh Database")]
    public static void Assign() {

      // assign all missing id's
      foreach (var asset in UnityEngine.Resources.LoadAll<AssetBase>("DB")) {
        if (asset && asset.AssetObject != null) {
          if (AssetObjectIdentifier.IsGuidValid(asset.AssetObject.Guid) == false) {
            asset.AssetObject.Guid = NewGuid();
            EditorUtility.SetDirty(asset);
          }
        }
      }

      // check for duplicates
      Dictionary<String, AssetBase> guids = new Dictionary<String, AssetBase>();

      foreach (var asset in UnityEngine.Resources.LoadAll<AssetBase>("DB")) {
        if (asset && asset.AssetObject != null) {
          // check for duplicate
          if (guids.ContainsKey(asset.AssetObject.Guid)) {

            // log duplicate 
            Debug.LogFormat("Found duplicate GUID {0} on assets {1} and {2}, creating new guid for {2}", asset.AssetObject.Guid, guids[asset.AssetObject.Guid].name, asset.name);

            // TODO: Here we should check which asset is the newest by inspecting the file system mtime
            // and make sure that we change the guid of the asset which is newest.

            // create new guid
            do {
              asset.AssetObject.Guid = NewGuid();
              EditorUtility.SetDirty(asset);
            } while (guids.ContainsKey(asset.AssetObject.Guid));
          }

          guids.Add(asset.AssetObject.Guid, asset);
        }
      }

      var prefabList = EntityPrefabsListAsset.Instance;
      if (!prefabList) {
        AssetDatabase.CreateAsset(prefabList = ScriptableObject.CreateInstance<EntityPrefabsListAsset>(), "Assets/Quantum/Resources/EntityPrefabsList.asset");
      }

      prefabList.Prefabs = new EntityPrefabsListAsset.EntityPrefabItem[0];

      foreach (var prefab in UnityEngine.Resources.LoadAll<GameObject>("PREFABS")) {
        var root = prefab.GetComponent<EntityPrefabRoot>();
        if (root) {
          var path = AssetDatabase.GetAssetPath(root).Split(new[] { "PREFABS" }, StringSplitOptions.RemoveEmptyEntries);
          if (path.Length == 2) {
            ArrayUtils.Add(ref prefabList.Prefabs, new EntityPrefabsListAsset.EntityPrefabItem {
              Path = path[1].Trim('/').Replace(".prefab", ""),
              Prefab = root
            });
          }
        }
      }

      EditorUtility.SetDirty(prefabList);

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
    }
  }
}