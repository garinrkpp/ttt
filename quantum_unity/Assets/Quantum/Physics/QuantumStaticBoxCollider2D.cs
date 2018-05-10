using Photon.Deterministic;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumStaticBoxCollider2D : MonoBehaviour {
  public FPVector2 Size;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
    GizmoUtils.DrawGizmosBox(transform, Size.ToUnityVector3(), selected, ColorRGBA.ColliderGreen.ToColor());
  }
}
