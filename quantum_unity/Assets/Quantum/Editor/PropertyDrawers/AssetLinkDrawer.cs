using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(AssetLink))]
  public class AssetLinkDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      DrawAssetObjectSelector(position, property, label);
    }

    public static void DrawAssetObjectSelector(Rect position, SerializedProperty property, GUIContent label, Type type = null) {
      type = type ?? typeof(AssetBase);

      var all = UnityEngine.Resources.LoadAll<AssetBase>("DB");
      var guid = property.FindPropertyRelative("Guid").stringValue;
      var selected = EditorGUI.ObjectField(position, label, all.FirstOrDefault(ObjectFilter(guid, type)), type, false) as AssetBase;

      if (selected) {
        property.FindPropertyRelative("Guid").stringValue = selected.AssetObject.Guid;
      }
      else {
        property.FindPropertyRelative("Guid").stringValue = "";
      }
    }

    static Func<AssetBase, Boolean> ObjectFilter(String guid, Type type) {
      return obj => obj && type.IsAssignableFrom(obj.GetType()) && obj.AssetObject != null && AssetObjectIdentifier.IsGuidValid(obj.AssetObject.Guid) && obj.AssetObject.Guid == guid;
    }
  }

  [CustomPropertyDrawer(typeof(MapLink))]
  public class MapLinkPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetLinkDrawer.DrawAssetObjectSelector(position, property, label, typeof(MapAsset));
    }
  }

  [CustomPropertyDrawer(typeof(PhysicsMaterialLink))]
  public class PhysicsMaterialPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetLinkDrawer.DrawAssetObjectSelector(position, property, label, typeof(PhysicsMaterialAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AnimatorGraphLink))]
  public class AnimatorGraphPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetLinkDrawer.DrawAssetObjectSelector(position, property, label, typeof(AnimatorGraphAsset));
    }
  }

  [CustomPropertyDrawer(typeof(PolygonColliderLink))]
  public class PolygonColliderDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetLinkDrawer.DrawAssetObjectSelector(position, property, label, typeof(PolygonColliderAsset));
    }
  }
}