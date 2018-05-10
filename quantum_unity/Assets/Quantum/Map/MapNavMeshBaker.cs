using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Photon.Deterministic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Quantum {
  public static class MapNavMeshBaker {

    struct Neighbor {
      public Int32 Vertex;
      public FPVector2 Direction;
    }

    struct Border {
      public Int32 V0;
      public Int32 V1;
      public Int32 Key;

      public Border(Int32 v0, Int32 v1, Int32 key) {
        if (v0 > v1) {
          V0 = v1;
          V1 = v0;
        }
        else {
          V0 = v0;
          V1 = v1;
        }

        Key = key;
      }

      public override Int32 GetHashCode() {
        return V0 ^ V1;
      }

      public override Boolean Equals(System.Object obj) {
        if (obj is Border) {
          var other = (Border)obj;

          // 
          return (other.V0 == this.V0 && other.V1 == this.V1) || (other.V0 == this.V1 && other.V1 == this.V0);
        }

        return false;
      }
    }

    public static NavMesh BakeNavMesh(MapData data, MapNavMeshDefinition navmeshDefinition) {
      try {
        FPMathUtils.LoadLookupTables();

        var vs_array = navmeshDefinition.Vertices.ToArray();
        var nav_vertices = vs_array.Map(x => new NavMeshVertex { Point = x.Position.ToFPVector2(), Neighbors = new Int32[0], Triangles = new Int32[0], Borders = new Int32[0] });
        var nav_triangles = new NavMeshTriangle[0];

        // TRIANGLES
        for (Int32 i = 0; i < navmeshDefinition.Triangles.Length; ++i) {
          Progress("Baking NavMesh '" + navmeshDefinition.name + "': Calculating Triangles", i, navmeshDefinition.Triangles.Length);

          var t = navmeshDefinition.Triangles[i];

          var v0 = Array.FindIndex(vs_array, x => x.Id == t.VertexIds[0]);
          var v1 = Array.FindIndex(vs_array, x => x.Id == t.VertexIds[1]);
          var v2 = Array.FindIndex(vs_array, x => x.Id == t.VertexIds[2]);

          ArrayUtils.Add(ref nav_triangles, new NavMeshTriangle {
            Vertex0 = v0,
            Vertex1 = v1,
            Vertex2 = v2,
            Center = (nav_vertices[v0].Point + nav_vertices[v1].Point + nav_vertices[v2].Point) / FP._3
          });
        }


        // TRIANGLE GRID
        var nav_triangles_grid = new NavMeshTriangleNode[data.Asset.Settings.GridSize * data.Asset.Settings.GridSize];

        //for (Int32 i = 0; i < nav_triangles_grid.Length; ++i) {
        //  nav_triangles_grid[i] = i + 1;
        //}

        for (Int32 i = 0; i < nav_triangles.Length; ++i) {
          Progress("Baking NavMesh '" + navmeshDefinition.name + "': Calculating Triangle Grid", i, nav_triangles.Length);

          var v0 = nav_vertices[nav_triangles[i].Vertex0].Point;
          var v1 = nav_vertices[nav_triangles[i].Vertex1].Point;
          var v2 = nav_vertices[nav_triangles[i].Vertex2].Point;

          for (Int32 z = 0; z < data.Asset.Settings.GridSize; ++z) {
            for (Int32 x = 0; x < data.Asset.Settings.GridSize; ++x) {

              var bl = data.Asset.Settings.WorldOffset + new FPVector2(x * data.Asset.Settings.GridNodeSize, z * data.Asset.Settings.GridNodeSize);
              var br = bl + new FPVector2(data.Asset.Settings.GridNodeSize, 0);
              var ur = bl + new FPVector2(data.Asset.Settings.GridNodeSize, data.Asset.Settings.GridNodeSize);
              var ul = bl + new FPVector2(0, data.Asset.Settings.GridNodeSize);

              if (
                // if any of the corners are inside the triangle
                FPCollision.TriangleContainsPoint(bl, v0, v1, v2) ||
                FPCollision.TriangleContainsPoint(br, v0, v1, v2) ||
                FPCollision.TriangleContainsPoint(ur, v0, v1, v2) ||
                FPCollision.TriangleContainsPoint(ul, v0, v1, v2) ||

                // BL => BR
                FPCollision.TriangleContainsPoint(v0, v1, bl, br) ||
                FPCollision.TriangleContainsPoint(v1, v2, bl, br) ||
                FPCollision.TriangleContainsPoint(v2, v0, bl, br) ||

                // BR => UR
                FPCollision.TriangleContainsPoint(v0, v1, br, ur) ||
                FPCollision.TriangleContainsPoint(v1, v2, br, ur) ||
                FPCollision.TriangleContainsPoint(v2, v0, br, ur) ||

                // UR => UL
                FPCollision.TriangleContainsPoint(v0, v1, ur, ul) ||
                FPCollision.TriangleContainsPoint(v1, v2, ur, ul) ||
                FPCollision.TriangleContainsPoint(v2, v0, ur, ul) ||

                // UL => BL
                FPCollision.TriangleContainsPoint(v0, v1, ul, bl) ||
                FPCollision.TriangleContainsPoint(v1, v2, ul, bl) ||
                FPCollision.TriangleContainsPoint(v2, v0, ul, bl)
                ) {

                var idx = (z * data.Asset.Settings.GridSize) + x;

                if (nav_triangles_grid[idx].Triangles == null) {
                  nav_triangles_grid[idx].Triangles = new Int32[0];
                }

                // add triangle to this grid node
                ArrayUtils.Add(ref nav_triangles_grid[idx].Triangles, i);
              }
            }
          }
        }

        // VERTEX NEIGHBORS

        for (Int32 v = 0; v < nav_vertices.Length; ++v) {
          Progress("Baking NavMesh '" + navmeshDefinition.name + "': Calculating Vertex Neighbors", v, nav_vertices.Length);

          var triangles = new HashSet<Int32>();
          var neighbors = new HashSet<Int32>();

          for (Int32 t = 0; t < nav_triangles.Length; ++t) {
            var tr = nav_triangles[t];
            if (tr.Vertex0 == v || tr.Vertex1 == v || tr.Vertex2 == v) {
              triangles.Add(t);

              neighbors.Add(tr.Vertex0);
              neighbors.Add(tr.Vertex1);
              neighbors.Add(tr.Vertex2);
            }
          }

          // remove itself from neighbors set
          neighbors.Remove(v);

          // 
          nav_vertices[v].Triangles = triangles.OrderBy(x => x).ToArray();
          nav_vertices[v].Neighbors = neighbors.ToArray();
        }

        // BORDER EDGES

        for (Int32 t = 0; t < nav_triangles.Length; ++t) {
          Progress("Baking NavMesh '" + navmeshDefinition.name + "': Calculating Border Edges", t, nav_triangles.Length);

          var tr = nav_triangles[t];
          if (IsBorderEdge(nav_triangles, t, tr.Vertex0, tr.Vertex1)) {
            ArrayUtils.Add(ref nav_vertices[tr.Vertex0].Borders, tr.Vertex1);
            ArrayUtils.Add(ref nav_vertices[tr.Vertex1].Borders, tr.Vertex0);
          }

          if (IsBorderEdge(nav_triangles, t, tr.Vertex1, tr.Vertex2)) {
            ArrayUtils.Add(ref nav_vertices[tr.Vertex1].Borders, tr.Vertex2);
            ArrayUtils.Add(ref nav_vertices[tr.Vertex2].Borders, tr.Vertex1);
          }

          if (IsBorderEdge(nav_triangles, t, tr.Vertex2, tr.Vertex0)) {
            ArrayUtils.Add(ref nav_vertices[tr.Vertex2].Borders, tr.Vertex0);
            ArrayUtils.Add(ref nav_vertices[tr.Vertex0].Borders, tr.Vertex2);
          }
        }

        // NORMALS

        var pt2 = FP._0_10 * FP._2;
        var pt3 = FP._0_10 * FP._3;

        for (Int32 i = 0; i < nav_vertices.Length; ++i) {
          Progress("Baking NavMesh '" + navmeshDefinition.name + "': Calculating Normals", i, nav_vertices.Length);

          var v = nav_vertices[i];
          var tn = new FPVector2[3];

          if (v.Borders != null) {
            // 0. preferred middle of borders
            var borders = v.Borders.Map(x => FPVector2.Normalize(nav_vertices[x].Point - v.Point));
            if (borders.Length == 2) {
              tn[0] = FPVector2.Normalize(FPVector2.Lerp(borders[0], borders[1], FP._0_50));
            }

            // 1. second preferred neighbor edge that is furthest away from borders
            if (v.Neighbors != null) {
              var neighbors = v.Neighbors.Where(x => !v.Borders.Contains(x)).Select(x =>
                new Neighbor {
                  Direction = FPVector2.Normalize(nav_vertices[x].Point - v.Point),
                  Vertex = x
                }
              ).ToArray();

              var max_dot = FP.MinValue;
              var max_neighbor = default(Neighbor);
              max_neighbor.Vertex = -1;

              for (Int32 n = 0; n < neighbors.Length; ++n) {
                var dot = FP._0;

                for (Int32 b = 0; b < borders.Length; ++b) {
                  dot += FPVector2.Dot(borders[b], neighbors[n].Direction);
                }

                dot = FPMath.Abs(dot);

                if (dot > max_dot) {
                  max_dot = dot;
                  max_neighbor = neighbors[n];
                }
              }

              if (max_neighbor.Vertex >= 0) {
                tn[1] = max_neighbor.Direction * pt2;
              }
            }
          }

          // 2. least preferred, avarage of triangle normals
          foreach (var tc in v.Triangles.Select(x => FPVector2.Normalize(TriangleCenter(nav_triangles[x], nav_vertices) - v.Point))) {
            tn[2] += tc;
          }

          tn[2] = FPVector2.Normalize(tn[2]);

          // find normal
          var failed = true;

          for (Int32 k = 0; failed && k < tn.Length; ++k) {
            if (tn[k] != FPVector2.Zero) {
              if (failed && TriangleContains(nav_triangles, nav_vertices, (v.Point + (tn[k] * pt3)))) {
                nav_vertices[i].Normal = FPVector2.Normalize(tn[k] * pt2);

                // we're done
                failed = false;
              }

              if (failed && TriangleContains(nav_triangles, nav_vertices, (v.Point + (-tn[k] * pt3)))) {
                nav_vertices[i].Normal = FPVector2.Normalize(-tn[k] * pt2);

                // we're done
                failed = false;
              }
            }
          }
        }

        // BORDER SET

        HashSet<Border> border_set = new HashSet<Border>();

        for (Int32 v = 0; v < nav_vertices.Length; ++v) {
          Progress(navmeshDefinition.name + " Baking: Border Set", v, nav_vertices.Length);

          if (nav_vertices[v].Borders != null) {
            for (Int32 n = 0; n < nav_vertices[v].Borders.Length; ++n) {
              border_set.Add(new Border(v, nav_vertices[v].Borders[n], border_set.Count + 1));
            }
          }
        }

        // BORDER GRID

        var nav_border_grid = new NavMeshBorderNode[data.Asset.Settings.GridSize * data.Asset.Settings.GridSize];

        for (Int32 z = 0; z < data.Asset.Settings.GridSize; ++z) {
          for (Int32 x = 0; x < data.Asset.Settings.GridSize; ++x) {
            var idx = (z * data.Asset.Settings.GridSize) + x;

            Progress("Baking NavMesh '" + navmeshDefinition.name + "': Border Grid", idx, data.Asset.Settings.GridSize * data.Asset.Settings.GridSize);

            // set index key
            // nav_border_grid[idx].key = idx + 1;

            // 
            var zn = (FP)z * data.Asset.Settings.GridNodeSize;
            var xn = (FP)x * data.Asset.Settings.GridNodeSize;

            FPVector2 bl = data.Asset.Settings.WorldOffset + new FPVector2(xn, zn);
            FPVector2 br = data.Asset.Settings.WorldOffset + new FPVector2(xn + data.Asset.Settings.GridNodeSize, zn);
            FPVector2 ur = data.Asset.Settings.WorldOffset + new FPVector2(xn + data.Asset.Settings.GridNodeSize, zn + data.Asset.Settings.GridNodeSize);
            FPVector2 ul = data.Asset.Settings.WorldOffset + new FPVector2(xn, zn + data.Asset.Settings.GridNodeSize);

            foreach (var b in border_set) {

              var p0 = nav_vertices[b.V0].Point;
              var p1 = nav_vertices[b.V1].Point;

              if (
                FPCollision.LineIntersectsLine(p0, p1, bl, br) ||
                FPCollision.LineIntersectsLine(p0, p1, br, ur) ||
                FPCollision.LineIntersectsLine(p0, p1, ur, ul) ||
                FPCollision.LineIntersectsLine(p0, p1, ul, bl)
              ) {
                if (nav_border_grid[idx].Borders == null) {
                  nav_border_grid[idx].Borders = new NavMeshBorder[0];
                }

                ArrayUtils.Add(ref nav_border_grid[idx].Borders, new NavMeshBorder {
                  Key = b.Key,
                  V0 = p0,
                  V1 = p1,
                });
              }
            }
          }
        }

        // TRIANGLE CENTER GRID

        var nav_triangles_center_grid = new Int32[data.Asset.Settings.GridSize * data.Asset.Settings.GridSize];

        for (Int32 z = 0; z < data.Asset.Settings.GridSize; ++z) {
          for (Int32 x = 0; x < data.Asset.Settings.GridSize; ++x) {
            var idx = (z * data.Asset.Settings.GridSize) + x;

            Progress("Baking NavMesh '" + navmeshDefinition.name + "': Triangle Center Grid", idx, data.Asset.Settings.GridSize * data.Asset.Settings.GridSize);

            var zn = (FP)(z * data.Asset.Settings.GridNodeSize);
            var xn = (FP)(x * data.Asset.Settings.GridNodeSize);
            var g = data.Asset.Settings.WorldOffset + new FPVector2(xn, zn) + new FPVector2(FP.FromFloat_UNSAFE(data.Asset.Settings.GridNodeSize * 0.5f), FP.FromFloat_UNSAFE(data.Asset.Settings.GridNodeSize * 0.5f));

            var d = FP.MaxValue;
            var t = -1;

            for (Int32 i = 0; i < nav_triangles.Length; ++i) {
              var c = nav_triangles[i].Center;

              if (FPVector2.DistanceSquared(g, c) < d) {
                d = FPVector2.DistanceSquared(g, c);
                t = i;
              }
            }

            Assert.Check(t >= 0);

            nav_triangles_center_grid[idx] = t;
          }
        }

        NavMesh navmesh;

        navmesh = new NavMesh();
        navmesh.GridSize = data.Asset.Settings.GridSize;
        navmesh.GridNodeSize = data.Asset.Settings.GridNodeSize;
        navmesh.WorldOffset = data.Asset.Settings.WorldOffset;

        navmesh.Name = navmeshDefinition.name;
        navmesh.Vertices = nav_vertices;
        navmesh.BorderGrid = nav_border_grid;
        navmesh.Triangles = nav_triangles;
        navmesh.TrianglesGrid = nav_triangles_grid;
        navmesh.TrianglesCenterGrid = nav_triangles_center_grid;

        return navmesh;
      }
      finally {
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
      }
    }

    static FPVector2 TriangleCenter(NavMeshTriangle t, NavMeshVertex[] vs) {
      return (vs[t.Vertex0].Point + vs[t.Vertex1].Point + vs[t.Vertex2].Point) / FP._3;
    }

    static Boolean TriangleContains(NavMeshTriangle[] ts, NavMeshVertex[] vs, FPVector2 point) {
      for (Int32 i = 0; i < ts.Length; ++i) {
        var t = ts[i];

        if (FPCollision.TriangleContainsPoint(point, vs[t.Vertex0].Point, vs[t.Vertex1].Point, vs[t.Vertex2].Point)) {
          return true;
        }
      }

      return false;
    }

    static Boolean IsBorderEdge(NavMeshTriangle[] triangles, Int32 tri, Int32 v0, Int32 v1) {
      for (Int32 i = 0; i < triangles.Length; ++i) {
        if (i != tri) {
          var t = triangles[i];
          if (t.Vertex0 == v0 || t.Vertex1 == v0 || t.Vertex2 == v0) {
            if (t.Vertex0 == v1 || t.Vertex1 == v1 || t.Vertex2 == v1) {
              return false;
            }
          }
        }
      }

      return true;
    }

    static void Progress(String task, Int32 number, Int32 total) {
#if UNITY_EDITOR
      Progress(task + String.Format(" ({0} of {1})", number, total), (Single)number / (Single)total);
#endif
    }

    static void Progress(String task, Single progress) {
#if UNITY_EDITOR
      EditorUtility.DisplayProgressBar("Baking Simulation Data", task, progress);
#endif
    }

    static Boolean ProgressCancelable(String task, Single progress) {
#if UNITY_EDITOR
      return EditorUtility.DisplayCancelableProgressBar("Baking Simulation Data", task, progress);
#else
      return false;
#endif
    }

  }
}