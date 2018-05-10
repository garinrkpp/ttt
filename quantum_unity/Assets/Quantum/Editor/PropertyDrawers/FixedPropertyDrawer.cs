using System;
using UnityEditor;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(FP))]
  public class FPPropertyDrawer : PropertyDrawer {
    static GUIStyle _overlay;

    static public GUIStyle OverlayStyle {
      get {
        if (_overlay == null) {
          _overlay = new GUIStyle(EditorStyles.miniLabel);
          _overlay.alignment = TextAnchor.MiddleRight;
          _overlay.contentOffset = new Vector2(-2, 0);

          Color c;
          c = EditorGUIUtility.isProSkin ? Color.yellow : Color.blue;
          c.a = 0.75f;

          _overlay.normal.textColor = c;
        }

        return _overlay;
      }
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      // go into child property (raw)
      prop.Next(true);

      // draw field
      Draw(p, prop, label);
    }

    public static void Draw(Rect p, SerializedProperty prop, GUIContent label) {
      // grab value
      var f = FP.FromRaw(prop.longValue);
      var v = (Single)Math.Round(f.AsFloat, 5);

      // edit value
      try {
        var n = label == null ? EditorGUI.FloatField(p, v) : EditorGUI.FloatField(p, label, v);
        if (n != v) {
          prop.longValue = FP.FromFloat_UNSAFE(n).RawValue;
        }

        GUI.Label(p, "(Fixed Point)", OverlayStyle);
      }
      catch (FormatException exn) {
        if (exn.Message != ".") {
          Debug.LogException(exn);
        }
      }
    }
  }

  [CustomPropertyDrawer(typeof(FPVector2))]
  public class FPVector2PropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return 60;
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      p = p.SetHeight(20);

      EditorGUI.LabelField(p, label);

      EditorGUI.indentLevel += 1;
      EditorGUI.PropertyField(p.AddY(20), prop.FindPropertyRelative("X"));
      EditorGUI.PropertyField(p.AddY(40), prop.FindPropertyRelative("Y"));
      EditorGUI.indentLevel -= 1;
    }
  }

  [CustomPropertyDrawer(typeof(FPVector3))]
  public class FPVector3PropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return 80;
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      p = p.SetHeight(20);

      EditorGUI.LabelField(p, label);

      EditorGUI.indentLevel += 1;
      EditorGUI.PropertyField(p.AddY(20), prop.FindPropertyRelative("X"));
      EditorGUI.PropertyField(p.AddY(40), prop.FindPropertyRelative("Y"));
      EditorGUI.PropertyField(p.AddY(60), prop.FindPropertyRelative("Z"));
      EditorGUI.indentLevel -= 1;
    }
  }
}