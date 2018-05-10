using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(Quantum.AssetObjectIdentifier))]
  public class AssetObjectIdentifierDrawer : PropertyDrawer {
    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      var guid = prop.FindPropertyRelative("Guid");
      EditorGUI.PropertyField(p, guid, false);
    }
  }
}