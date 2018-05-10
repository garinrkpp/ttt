using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(DynamicShapeConfig))]
  public class DynamicShapeConfigDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      switch ((Quantum.Core.DynamicShapeType)property.FindPropertyRelative("ColliderType").intValue) {
        case Core.DynamicShapeType.Box: return 100;
        case Core.DynamicShapeType.Circle:
        case Core.DynamicShapeType.Polygon:
          return 60;
      }

      return 40;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var p = position.SetHeight(20);

      EditorGUI.LabelField(p, label);

      try {
        EditorGUI.indentLevel += 1;
        EditorGUI.PropertyField(p.AddY(20), property.FindPropertyRelative("ColliderType"), new GUIContent("Type"));

        switch ((Quantum.Core.DynamicShapeType)property.FindPropertyRelative("ColliderType").intValue) {
          case Core.DynamicShapeType.Box:
            EditorGUI.PropertyField(p.AddY(40), property.FindPropertyRelative("BoxExtents"), new GUIContent("Extents"));
            break;

          case Core.DynamicShapeType.Circle:
            EditorGUI.PropertyField(p.AddY(40), property.FindPropertyRelative("CircleRadius"), new GUIContent("Radius"));
            break;

          case Core.DynamicShapeType.Polygon:
            EditorGUI.PropertyField(p.AddY(40), property.FindPropertyRelative("PolygonCollider"), new GUIContent("Asset"));
            break;
        }
      }
      finally {
        EditorGUI.indentLevel -= 1;
      }
    }

  }
}