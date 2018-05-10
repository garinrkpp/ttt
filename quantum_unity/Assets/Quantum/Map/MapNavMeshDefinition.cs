using UnityEngine;
using System.Linq;
using Photon.Deterministic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct MapNavMeshTriangle {
  public String Id;
  public String[] VertexIds;
}

[System.Serializable]
public struct MapNavMeshVertex {
  public String Id;
  public Vector3 Position;
}

public class MapNavMeshDefinition : MonoBehaviour {
  public Boolean AlwaysDrawGizmos;
  public Boolean DrawMesh;
  public Color Color = Color.white;

  Int32 Highlight = 0;

  [EditorDisabled]
  public MapNavMeshVertex[] Vertices;

  [EditorDisabled]
  public MapNavMeshTriangle[] Triangles;

  public MapNavMeshVertex GetVertex(String id) {
    for (Int32 i = 0; i < Vertices.Length; ++i) {
      if (Vertices[i].Id == id) {
        return Vertices[i];
      }
    }

    throw new System.InvalidOperationException();
  }

  public Int32 GetVertexIndex(String id) {
    return Array.FindIndex(Vertices, x => x.Id == id);
  }

  public Boolean Contains(FPVector2 point) {
    return Contains(point.ToUnityVector3());
  }

  public Boolean Contains(Vector3 point) {
    for (Int32 i = 0; i < Triangles.Length; ++i) {
      var tri = Triangles[i].VertexIds.Select(x => GetVertex(x).Position).ToArray();

      var v0 = tri[2] - tri[0];
      var v1 = tri[1] - tri[0];
      var v2 = point - tri[0];

      var dot00 = Vector3.Dot(v0, v0);
      var dot01 = Vector3.Dot(v0, v1);
      var dot02 = Vector3.Dot(v0, v2);
      var dot11 = Vector3.Dot(v1, v1);
      var dot12 = Vector3.Dot(v1, v2);

      var invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
      var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      // check if point is in triangle
      if ((u >= 0) && (v >= 0) && (u + v < 1)) {
        return true;
      }
    }

    return false;
  }

#if UNITY_EDITOR
  void OnDrawGizmos() {
    if (AlwaysDrawGizmos) {
      OnDrawGizmosSelected();
    }
  }

  void OnDrawGizmosSelected() {
    Gizmos.color = Color;

    foreach (var t in Triangles) {
      var v0 = GetVertex(t.VertexIds[0]).Position;
      var v1 = GetVertex(t.VertexIds[1]).Position;
      var v2 = GetVertex(t.VertexIds[2]).Position;

      if (DrawMesh) {
        Handles.color = Color * 0.5f;
        Handles.lighting = true;
        Handles.DrawAAConvexPolygon(v0, v1, v2);
      }
      else {
        Gizmos.DrawLine(v0, v1);
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v0);
      }
    }

    Gizmos.color = Color.white;
  }
#endif
}
