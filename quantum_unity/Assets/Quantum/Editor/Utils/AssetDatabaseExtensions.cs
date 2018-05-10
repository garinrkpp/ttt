using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  public static class AssetDatabaseExt {
    public static void DeleteNestedAsset(this Object parent, Object child) {
      // destroy child
      Object.DestroyImmediate(child, true);

      // set dirty
      EditorUtility.SetDirty(parent);

      // save
      AssetDatabase.SaveAssets();
    }

    public static Object CreateNestedScriptableObjectAsset(this Object parent, System.Type type, System.String name) {
      // create new asset in memory
      Object asset;

      asset = ScriptableObject.CreateInstance(type);
      asset.name = name;

      // add to parent asset
      AssetDatabase.AddObjectToAsset(asset, parent);

      // set dirty
      EditorUtility.SetDirty(parent);

      // save
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      return asset;
    }
  }
}