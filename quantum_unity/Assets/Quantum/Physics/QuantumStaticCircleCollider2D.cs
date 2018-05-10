using Photon.Deterministic;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumStaticCircleCollider2D : MonoBehaviour {
  public FP Radius;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
    GizmoUtils.DrawGizmosCircle(transform.position, Radius.AsFloat, selected, ColorRGBA.ColliderGreen.ToColor());
  }
}
