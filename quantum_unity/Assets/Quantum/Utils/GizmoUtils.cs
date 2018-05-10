using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Deterministic;
using System;

namespace Quantum {
  public static class GizmoUtils {
    public static readonly Color Blue = new Color(0.0f, 0.7f, 1f);

    static public Color Alpha(this Color color, Single a) {
      color.a = a;
      return color;
    }

    static public void DrawGizmosBox(Transform transform, Vector3 size) {
      DrawGizmosBox(transform, size, false);
    }

    static public void DrawGizmosBox(Transform transform, Vector3 size, bool selected) {
      DrawGizmosBox(transform, size, selected, Blue);
    }

    static public void DrawGizmosBox(Transform transform, Vector3 size, Color color) {
      DrawGizmosBox(transform, size, false, color);
    }

    static public void DrawGizmosBox(Transform transform, Vector3 size, bool selected, Color color) {
      DrawGizmosBox(transform, size, selected, color, Vector3.zero);
    }

    static public void DrawGizmosBox(Transform transform, Vector3 size, bool selected, Color color, Vector3 offset) {
      color.a = selected ? 0.3f : 0.1f;

      Gizmos.color = color;
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawCube(offset, size);

      color.a = selected ? 0.5f : 0.3f;

      Gizmos.color = color;
      Gizmos.DrawWireCube(offset, size);
      Gizmos.matrix = Matrix4x4.identity;
    }

    static public void DrawGizmosBox(FPVector2 center, FPVector2 size, Color color) {
      color.a = 0.5f;

      Gizmos.color = color;
      Gizmos.DrawCube(center.ToUnityVector3(), size.ToUnityVector3());

      color.a = 0.75f;

      Gizmos.color = color;
      Gizmos.DrawWireCube(center.ToUnityVector3(), size.ToUnityVector3());
    }

    static public void DrawGizmosBox(Vector3 center, Vector3 size, Color color) {
      color.a = 0.5f;

      Gizmos.color = color;
      Gizmos.DrawCube(center, size);

      color.a = 0.75f;

      Gizmos.color = color;
      Gizmos.DrawWireCube(center, size);
    }

    static public void DrawGizmosBox_NoLines(Vector3 center, Vector3 size, Color color) {
      Gizmos.color = color;
      Gizmos.DrawCube(center, size);
      Gizmos.color = Color.white;
    }

    static public void DrawGizmosCircle(Vector3 position, Single radius) {
      DrawGizmosCircle(position, radius, false);
    }

    static public void DrawGizmosCircle(Vector3 position, Single radius, Boolean selected) {
      DrawGizmosCircle(position, radius, selected, new Color(0.0f, 0.7f, 1f));
    }

    static public void DrawGizmosCircle(Vector3 position, Single radius, Color color) {
      DrawGizmosCircle(position, radius, false, color);
    }

    static public void DrawGizmosCircle(Vector3 position, Single radius, Boolean selected, Color color) {
      var s = new Vector3(radius, radius, radius);

      color.a = selected ? 0.3f : 0.1f;

      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(0, 0, 0);
#else
      rot = Quaternion.Euler(-90, 0, 0);
#endif

      Gizmos.color = color;
      Gizmos.DrawMesh(UnityEngine.Resources.Load<Mesh>("DEV/Mesh/CircleMesh"), 0, position, rot, s);

      color.a = selected ? 0.5f : 0.3f;

      Gizmos.color = color;
      Gizmos.DrawWireMesh(UnityEngine.Resources.Load<Mesh>("DEV/Mesh/CircleMesh"), 0, position, rot, s);
      Gizmos.matrix = Matrix4x4.identity;
    }

    static public void DrawGizmoGrid(FPVector2 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
      DrawGizmoGrid(bottomLeft.ToUnityVector3(), width, height, nodeSize, color);
    }

    static public void DrawGizmoGrid(Vector3 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
      Gizmos.color = color;

      for (Int32 z = 0; z < height; ++z) {
        for (Int32 x = 0; x < width; ++x) {
          var zn = FP.FromFloat_UNSAFE(z * nodeSize + (nodeSize / 2f));
          var xn = FP.FromFloat_UNSAFE(x * nodeSize + (nodeSize / 2f));

          Gizmos.DrawWireCube(bottomLeft + new FPVector2(zn, xn).ToUnityVector3(), new FPVector2(nodeSize, nodeSize).ToUnityVector3());
        }
      }

      Gizmos.color = Color.white;
    }
  }
}