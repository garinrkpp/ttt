using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MapNavMeshDefinition))]
public class MapNavMeshDefinitionEditor : Editor {
  public static bool Editing = false;
  public static HashSet<string> SelectedVertices = new HashSet<string>();
  public static HashSet<string> SelectedTriangles = new HashSet<string>();

  bool shift;
  bool ctrl;
  bool alt;

  static string NewId() {
    return System.Guid.NewGuid().ToString();
  }

  public override void OnInspectorGUI() {
    var data = target as MapNavMeshDefinition;
    if (data) {

      if (data.Triangles == null) {
        data.Triangles = new MapNavMeshTriangle[0];
        Save();
      }

      if (data.Vertices == null) {
        data.Vertices = new MapNavMeshVertex[0];
        Save();
      }

      EditorGUILayout.HelpBox(string.Format("Vertices: {0}\r\nTriangles: {1}", data.Vertices.Length, data.Triangles.Length), MessageType.Info);

      data.Color = EditorGUILayout.ColorField("Color", data.Color);

      var s = new GUIStyle(EditorStyles.miniButton);
      s.normal.textColor = Editing ? Color.red : Color.green;
      s.fontStyle = FontStyle.Bold;

      if (GUILayout.Button("Toggle Always Draw " + (data.AlwaysDrawGizmos ? "[On]" : "[Off]"), EditorStyles.miniButton)) {
        data.AlwaysDrawGizmos = !data.AlwaysDrawGizmos;
      }

      if (GUILayout.Button("Toggle Draw Mesh" + (data.DrawMesh ? "[On]" : "[Off]"), EditorStyles.miniButton)) {
        data.DrawMesh = !data.DrawMesh;
      }

      if (GUILayout.Button("Export Mesh", EditorStyles.miniButton)) {
        Mesh m = new Mesh();
        m.name = "NavMesh";
        m.vertices = data.Vertices.Select(x => x.Position).ToArray();
        m.triangles = data.Triangles.SelectMany(x => x.VertexIds.Select<string, int>(data.GetVertexIndex)).ToArray();

        AssetDatabase.CreateAsset(m, "Assets/NavMesh.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
      }

      if (GUILayout.Button(Editing ? "Done Editing NavMesh" : "Start Edit NavMesh", s)) {
        Editing = !Editing;

        Save();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
      }

      if (Editing) {
        if (GUILayout.Button("Add Vertex", EditorStyles.miniButton)) {
          AddVertex(data);
        }

        if (GUILayout.Button("Delete Vertices", EditorStyles.miniButton)) {
          DeleteVertices(data);
        }


        if (SelectedVertices.Count == 1) {
          if (GUILayout.Button("Duplicate Vertex", EditorStyles.miniButton)) {
            DuplicateVertex(data);
          }
        }

        if (SelectedVertices.Count == 2) {
          if (GUILayout.Button("Insert Vertex + Create Triangle", EditorStyles.miniButton)) {
            InsertVertexAndCreateTriangle(data);
          }
        }

        if (SelectedVertices.Count == 3) {
          if (GUILayout.Button("Create Triangle", EditorStyles.miniButton)) {
            CreateTriangle(data);
          }
        }

        if (GUILayout.Button("Duplicate And Flip", EditorStyles.miniButton)) {
          var idMap = new Dictionary<string, string>();

          foreach (var k in data.Vertices.ToList()) {
            var v = k;

            v.Id = System.Guid.NewGuid().ToString();
            v.Position.x = -v.Position.x;
            v.Position.y = -v.Position.y;
            v.Position.z = -v.Position.z;

            idMap.Add(k.Id, v.Id);

            ArrayUtility.Add(ref data.Vertices, v);
          }

          foreach (var k in data.Triangles.ToList()) {
            var t = k;

            t.Id = System.Guid.NewGuid().ToString();
            t.VertexIds = new string[3];
            t.VertexIds[0] = idMap[k.VertexIds[0]];
            t.VertexIds[1] = idMap[k.VertexIds[1]];
            t.VertexIds[2] = idMap[k.VertexIds[2]];

            ArrayUtility.Add(ref data.Triangles, t);
          }
        }

        if (SelectedVertices.Count == 1) {
          var v = data.GetVertex(SelectedVertices.First());

          EditorGUI.BeginChangeCheck();

          v.Position = EditorGUILayout.Vector3Field("", v.Position);

          if (EditorGUI.EndChangeCheck()) {
            data.Vertices[data.GetVertexIndex(SelectedVertices.First())] = v;
          }
        }
      }

      GUI.enabled = false;

      try {
        //base.OnInspectorGUI();
      }
      finally {
        GUI.enabled = true;
      }
    }
    else {
      Editing = false;
    }
  }

  void Save() {
    EditorUtility.SetDirty(target);
  }

  void OnDisable() {
    if (Editing && target) {
      Selection.activeGameObject = (target as MapNavMeshDefinition).gameObject;
    }
  }

  void DuplicateVertex(MapNavMeshDefinition data) {
    if (SelectedVertices.Count == 1) {
      var id = NewId();

      ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
        Id = id,
        Position = data.GetVertex(SelectedVertices.First()).Position.RoundToInt()
      });

      Save();

      SelectedVertices.Clear();
      SelectedVertices.Add(id);
    }
  }

  void AddVertex(MapNavMeshDefinition data) {
    ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
      Id = NewId(),
      Position = new Vector3()
    });

    Save();
  }

  void DeleteVertices(MapNavMeshDefinition data) {
    if (SelectedVertices.Count > 0) {
      if (EditorUtility.DisplayDialog("Delete Vertices?", "Are You sure?", "Yes", "No")) {
        data.Vertices = data.Vertices.Where(x => SelectedVertices.Contains(x.Id) == false).ToArray();
        data.Triangles = data.Triangles.Where(x => x.VertexIds.Any(y => SelectedVertices.Contains(y)) == false).ToArray();

        Save();

        SelectedVertices.Clear();
      }
    }
  }

  void CreateTriangle(MapNavMeshDefinition data) {
    if (SelectedVertices.Count() == 3) {
      ArrayUtility.Add(ref data.Triangles, new MapNavMeshTriangle {
        Id = NewId(),
        VertexIds = SelectedVertices.ToArray()
      });

      Save();
    }
  }

  void InsertVertexAndCreateTriangle(MapNavMeshDefinition data) {
    if (SelectedVertices.Count() == 2) {
      var id = NewId();

      ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
        Id = id,
        Position =
          Vector3.Lerp(
            data.GetVertex(SelectedVertices.First()).Position,
            data.GetVertex(SelectedVertices.Last()).Position,
            0.5f
          ).RoundToInt()
      });

      SelectedVertices.Add(id);

      CreateTriangle(data);

      SelectedVertices.Clear();
      SelectedVertices.Add(id);

      Save();
    }
  }

  void OnSceneGUI() {
    Tools.current = Tool.None;

    var data = target as MapNavMeshDefinition;

    if (Editing && data) {
      Selection.activeGameObject = (target as MapNavMeshDefinition).gameObject;
    }
    else {
      return;
    }

    if (data) {
      if (Event.current.type == EventType.KeyDown) {
        switch (Event.current.keyCode) {
          case KeyCode.Escape:
            SelectedVertices.Clear();
            break;

          case KeyCode.T:
            switch (SelectedVertices.Count()) {
              case 2:
                InsertVertexAndCreateTriangle(data);
                break;

              case 3:
                CreateTriangle(data);
                break;

              default:
                Debug.LogError("Must select 2 or 3 vertices to use 'T' command");
                break;
            }
            break;

          case KeyCode.X:
            var select = new HashSet<string>();

            foreach (var tri in data.Triangles) {
              foreach (var v in SelectedVertices) {
                if (System.Array.IndexOf(tri.VertexIds, v) >= 0) {
                  select.Add(tri.VertexIds[0]);
                  select.Add(tri.VertexIds[1]);
                  select.Add(tri.VertexIds[2]);
                  break;
                }
              }
            }

            foreach (var v in select) {
              SelectedVertices.Add(v);
            }
            break;

          case KeyCode.Backspace:
            DeleteVertices(data);
            break;
        }
      }

      foreach (var v in data.Vertices) {
        var p = data.transform.TransformPoint(v.Position);
        var r = Quaternion.LookRotation((p - SceneView.currentDrawingSceneView.camera.transform.position).normalized);
        var s = 0f;

        if (SelectedVertices.Contains(v.Id)) {
          s = 0.2f;
          Handles.color = Color.green;
        }
        else {
          s = 0.1f;
          Handles.color = Color.white;
        }

        if (Handles.Button(p, r, s, s, Handles.DotCap)) {

          if (Event.current.shift) {
            if (SelectedVertices.Contains(v.Id)) {
              SelectedVertices.Remove(v.Id);
            }
            else {
              SelectedVertices.Add(v.Id);
            }
          }
          else {
            SelectedVertices.Clear();
            SelectedVertices.Add(v.Id);
          }

          Repaint();
        }
      }

      if (SelectedVertices.Count > 0) {
        var center = Vector3.zero;
        var positions = data.Vertices.Where(x => SelectedVertices.Contains(x.Id)).Select(x => data.transform.TransformPoint(x.Position));

        foreach (var p in positions) {
          center += p;
        }

        center /= positions.Count();

        var movedCenter = Handles.DoPositionHandle(center, Quaternion.identity);
        if (movedCenter != center) {
          var m = movedCenter - center;

#if QUANTUM_XY
          m.z = 0;
#else
          m.y = 0;
#endif

          foreach (var selected in SelectedVertices) {
            var index = data.GetVertexIndex(selected);
            if (index >= 0) {
              data.Vertices[index].Position += m;
            }
          }
        }
      }
    }
  }
}
