using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Core {
  public static class DebugDraw {

    static Queue<Draw.DebugRay> _rays = new Queue<Draw.DebugRay>();
    static Queue<Draw.DebugLine> _lines = new Queue<Draw.DebugLine>();
    static Queue<Draw.DebugCircle> _circles = new Queue<Draw.DebugCircle>();
    static Queue<Draw.DebugRectangle> _rectangles = new Queue<Draw.DebugRectangle>();

    static Dictionary<ColorRGBA, Material> _materials = new Dictionary<ColorRGBA, Material>(ColorRGBA.EqualityComparer.Instance);

    static Mesh _circleMesh;
    static Mesh CircleMesh {
      get {
        if (!_circleMesh) {
          _circleMesh = UnityEngine.Resources.Load<Mesh>("DEV/Mesh/CircleMesh");
        }

        return _circleMesh;
      }
    }

    static Mesh _quadMesh;
    static Mesh QuadMesh {
      get {
        if (!_quadMesh) {
          _quadMesh = UnityEngine.Resources.Load<Mesh>("DEV/Mesh/QuadMesh");
        }

        return _quadMesh;
      }
    }
    static public void Ray(Draw.DebugRay ray) {
      lock (_rays) {
        _rays.Enqueue(ray);
      }
    }

    static public void Line(Draw.DebugLine line) {
      lock (_lines) {
        _lines.Enqueue(line);
      }
    }

    static public void Circle(Draw.DebugCircle circle) {
      lock (_circles) {
        _circles.Enqueue(circle);
      }
    }

    static public void Rectangle(Draw.DebugRectangle rectangle) {
      lock (_rectangles) {
        _rectangles.Enqueue(rectangle);
      }
    }

    static public Material GetMaterial(ColorRGBA color) {
      Material material;

      if (_materials.TryGetValue(color, out material) == false) {
        material = new Material(UnityEngine.Resources.Load<Material>("DEV/DebugDraw"));
        material.SetColor("_Color", color.ToColor());

        _materials.Add(color, material);
      }

      return material;
    }

    static Draw.DebugRay[] _raysArray = new Draw.DebugRay[64];
    static Draw.DebugLine[] _linesArray = new Draw.DebugLine[64];
    static Draw.DebugCircle[] _circlesArray = new Draw.DebugCircle[64];
    static Draw.DebugRectangle[] _rectanglesArray = new Draw.DebugRectangle[64];

    static public void DrawAll() {
      var raysCount = TakeAllFromQueueAndClearLocked(_rays, ref _raysArray);
      var linesCount = TakeAllFromQueueAndClearLocked(_lines, ref _linesArray);
      var circlesCount = TakeAllFromQueueAndClearLocked(_circles, ref _circlesArray);
      var rectanglesCount = TakeAllFromQueueAndClearLocked(_rectangles, ref _rectanglesArray);

      for (Int32 i = 0; i < raysCount; ++i) {
        DrawRay(_raysArray[i]);
      }

      for (Int32 i = 0; i < linesCount; ++i) {
        DrawLine(_linesArray[i]);
      }

      for (Int32 i = 0; i < circlesCount; ++i) {
        DrawCircle(_circlesArray[i]);
      }

      for (Int32 i = 0; i < rectanglesCount; ++i) {
        DrawRectangle(_rectanglesArray[i]);
      }
    }

    static void DrawRay(Draw.DebugRay ray) {
      Debug.DrawRay(ray.Origin.ToUnityVector3(), ray.Direction.ToUnityVector3(), ray.Color.ToColor());
    }

    static void DrawLine(Draw.DebugLine line) {
      Debug.DrawLine(line.Start.ToUnityVector3(), line.End.ToUnityVector3(), line.Color.ToColor());
    }

    static void DrawCircle(Draw.DebugCircle circle) {
      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(180, 0, 0);
#else
      rot = Quaternion.Euler(-90, 0, 0);
#endif

      // matrix for mesh
      var m = Matrix4x4.TRS(circle.Center.ToUnityVector3(), rot, Vector3.one * circle.Radius.AsFloat);

      // draw
      Graphics.DrawMesh(CircleMesh, m, GetMaterial(circle.Color), 0, null);
    }

    static void DrawRectangle(Draw.DebugRectangle rectangle) {
      var m = Matrix4x4.TRS(rectangle.Center.ToUnityVector3(), rectangle.Rotation.ToUnityQuaternion(), rectangle.Size.ToUnityVector3());

      Graphics.DrawMesh(QuadMesh, m, GetMaterial(rectangle.Color), 0, null);
    }

    static Int32 TakeAllFromQueueAndClearLocked<T>(Queue<T> queue, ref T[] result) {
      lock (queue) {
        var count = 0;

        if (queue.Count > 0) {
          // if result array size is less than queue count
          if (result.Length < queue.Count) {

            // find the next new size that is a multiple of the current result size
            var newSize = result.Length;

            while (newSize < queue.Count) {
              newSize = newSize * 2;
            }

            // and re-size array
            Array.Resize(ref result, newSize);
          }

          // grab all
          while (queue.Count > 0) {
            result[count++] = queue.Dequeue();
          }

          // clear queue
          queue.Clear();
        }

        return count;
      }
    }

  }
}