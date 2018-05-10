using Photon.Deterministic;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumStaticPolygonCollider2D))]
  public class StaticPolygonCollider2DEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
      if (GUILayout.Button("Recenter", EditorStyles.miniButton)) {
        var collider = (QuantumStaticPolygonCollider2D)target;
        collider.Vertices = FPVector2.RecenterPolygon(collider.Vertices);
      }

      base.OnInspectorGUI();
    }

  }
}