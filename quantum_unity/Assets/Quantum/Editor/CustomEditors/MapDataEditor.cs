using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(MapData), true)]
  public class MapDataEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      base.DrawDefaultInspector();

      var data = target as MapData;
      if (data) {

        if (data.Asset) {
          EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);

          if (GUILayout.Button("Bake Data", EditorStyles.miniButton)) {
            MapDataBaker.BakeMapData(data, true);
            EditorUtility.SetDirty(data.Asset);
            AssetDatabase.Refresh();
          }

          if (GUILayout.Button("Bake Nav Meshes", EditorStyles.miniButton)) {
            MapDataBaker.BakeNavMeshes(data, true);
            EditorUtility.SetDirty(data.Asset);
            AssetDatabase.Refresh();
          }

          EditorGUI.EndDisabledGroup();
        }

        OnInspectorGUI(data);

        if (data.Asset.Settings.GridSize < 2) {
          data.Asset.Settings.GridSize = 2;
        }

        if ((data.Asset.Settings.GridSize & 1) == 1) {
          data.Asset.Settings.GridSize += 1;
        }

        if (data.Asset.Settings.GridNodeSize < 2) {
          data.Asset.Settings.GridNodeSize = 2;
        }

        if ((data.Asset.Settings.GridNodeSize & 1) == 1) {
          data.Asset.Settings.GridNodeSize += 1;
        }
      }
    }

    void OnInspectorGUI(MapData data) {
      data.transform.position = Vector3.zero;

      if (data.Asset) {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Asset Settings", EditorStyles.boldLabel);

        var asset = new SerializedObject(data.Asset);
        var property = asset.GetIterator();

        // enter first child
        property.Next(true);

        while (property.Next(false)) {
          if (property.name.StartsWith("m_")) {
            continue;
          }

          EditorGUILayout.PropertyField(property, true);
        }

        asset.ApplyModifiedProperties();
      }
    }
  }
}