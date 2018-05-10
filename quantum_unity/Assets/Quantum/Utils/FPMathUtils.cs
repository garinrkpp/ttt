using Photon.Deterministic;
using Quantum;
using System;
using UnityEngine;

public static class FPMathUtils {
  public static void LoadLookupTables(Boolean force = false) {
    if (FPLut.IsLoaded && force == false) {
      return;
    }

    FPLut.Init(file => UnityEngine.Resources.Load<TextAsset>("LUT/" + file).bytes);
  }

  public static FP ToFP(this Single v) {
    return FP.FromFloat_UNSAFE(v);
  }

  public static Quaternion ToUnityQuaternion(this FP r) {
#if QUANTUM_XY
    return Quaternion.Euler(0, 0, -(r * FP.Rad2Deg).AsFloat);
#else
    return Quaternion.Euler(0, -(r * FP.Rad2Deg).AsFloat, 0);
#endif
  }

  public static Quaternion ToUnityQuaternion(this FPQuaternion r) {
    Quaternion q;

    q.x = r.X.AsFloat;
    q.y = r.Y.AsFloat;
    q.z = r.Z.AsFloat;
    q.w = r.W.AsFloat;

    // calculate square magnitude
    var sqr = (q.x * q.x) + (q.y * q.y) + (q.z + q.z) + (q.w * q.w);
    if (Mathf.Approximately(sqr, 0)) {
      return Quaternion.identity;
    }

    // re-normalize quaternion
    var fac = 1f / sqr;

    q.x *= fac;
    q.y *= fac;
    q.z *= fac;
    q.w *= fac;

    return q;
  }

  public static FPQuaternion ToFPQuaternion(this Quaternion r) {
    FPQuaternion q;

    q.X = r.x.ToFP();
    q.Y = r.y.ToFP();
    q.Z = r.z.ToFP();
    q.W = r.w.ToFP();

    return q;
  }
  public static FP ToFPRotation2D(this Quaternion r) {
#if QUANTUM_XY
    return -FP.FromFloat_UNSAFE(r.eulerAngles.z * Mathf.Deg2Rad);
#else
    return -FP.FromFloat_UNSAFE(r.eulerAngles.y * Mathf.Deg2Rad);
#endif
  }

  public static FPVector2 ToFPVector2(this Vector2 v) {
    return new FPVector2(v.x.ToFP(), v.y.ToFP());
  }

  public static Vector2 ToUnityVector2(this FPVector2 v) {
    return new Vector2(v.X.AsFloat, v.Y.AsFloat);
  }

  public static FPVector2 ToFPVector2(this Vector3 v) {
#if QUANTUM_XY
    return new FPVector2(v.x.ToFP(), v.y.ToFP());
#else
    return new FPVector2(v.x.ToFP(), v.z.ToFP());
#endif
  }

  public static FPVector3 ToFPVector3(this Vector3 v) {
    return new FPVector3(v.x.ToFP(), v.y.ToFP(), v.z.ToFP());
  }

  public static Vector3 ToUnityVector3(this FPVector2 v) {
#if QUANTUM_XY
    return new Vector3(v.X.AsFloat, v.Y.AsFloat, 0);
#else
    return new Vector3(v.X.AsFloat, 0, v.Y.AsFloat);
#endif
  }

  public static Vector3 ToUnityVector3(this FPVector3 v) {
    return new Vector3(v.X.AsFloat, v.Y.AsFloat, v.Z.AsFloat);
  }

  public static Vector2 ToUnityVector2(this FPVector3 v) {
    return new Vector2(v.X.AsFloat, v.Y.AsFloat);
  }

  public static Vector3 RoundToInt(this Vector3 v) {
    v.x = Mathf.RoundToInt(v.x);
    v.y = Mathf.RoundToInt(v.y);
    v.z = Mathf.RoundToInt(v.z);
    return v;
  }

  public static Vector2 RoundToInt(this Vector2 v) {
    v.x = Mathf.RoundToInt(v.x);
    v.y = Mathf.RoundToInt(v.y);
    return v;
  }

  public static Color32 ToColor32(this ColorRGBA clr) {
    return new Color32(clr.R, clr.G, clr.B, clr.A);
  }

  public static Color ToColor(this ColorRGBA clr) {
    return new Color(clr.R / 255f, clr.G / 255f, clr.B / 255f, clr.A / 255f);
  }
}
