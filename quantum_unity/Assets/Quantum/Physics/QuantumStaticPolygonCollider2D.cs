using Photon.Deterministic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Quantum;

public class QuantumStaticPolygonCollider2D : MonoBehaviour {

  public FPVector2[] Vertices = new FPVector2[3] {
    new FPVector2(0, 2),
    new FPVector2(-1, 0),
    new FPVector2(+1, 0)
  };

  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }


  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
#if UNITY_EDITOR
    if (Vertices.Length >= 3) {
      FPMathUtils.LoadLookupTables();

      var color = FPVector2.IsPolygonConvex(Vertices) ? ColorRGBA.ColliderGreen : ColorRGBA.Red;
      var normals = FPVector2.CalculatePolygonNormals(Vertices);

      UnityEditor.Handles.matrix = transform.localToWorldMatrix;
      UnityEditor.Handles.color = color.ToColor().Alpha(0.125f);
      UnityEditor.Handles.DrawAAConvexPolygon(Vertices.Select(x => x.ToUnityVector3()).ToArray());

      var cw = FPVector2.IsClockWise(Vertices);

      Gizmos.color = color.ToColor().Alpha(selected ? 1f : 0.55f);
      Gizmos.matrix = transform.localToWorldMatrix;

      for (Int32 i = 0; i < Vertices.Length; ++i) {
        var v1 = Vertices[i].ToUnityVector3();
        var v2 = Vertices[(i + 1) % Vertices.Length].ToUnityVector3();
        var n = (cw ? -normals[i] : normals[i]).ToUnityVector3();
        var c = Vector3.Lerp(v1, v2, 0.5f);

        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(c, c + (n * 0.25f));
      }

      Gizmos.DrawWireSphere(FPVector2.CalculatePolygonCentroid(Vertices).ToUnityVector3(), 0.025f);
    }

    //Gizmos.DrawWireSphere(Vector3.zero, FPVector2.CalculatePolygonRadius(Vertices).AsFloat);

    Gizmos.color = UnityEditor.Handles.color = Color.white;
    Gizmos.matrix = UnityEditor.Handles.matrix = Matrix4x4.identity;
#endif
  }
}
