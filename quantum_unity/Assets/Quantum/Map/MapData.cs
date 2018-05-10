using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using Photon.Deterministic;

[ExecuteInEditMode]
public class MapData : MonoBehaviour {
  public MapAsset Asset;

  void Update() {
    transform.position = Vector3.zero;
  }

  void OnDrawGizmos() {
    if (Asset) {
      var bottomLeft = transform.position - (-Asset.Settings.WorldOffset).ToUnityVector3();

      GizmoUtils.DrawGizmosBox(
        transform,
        new FPVector2 (Asset.Settings.WorldSize, Asset.Settings.WorldSize).ToUnityVector3()
      );

      GizmoUtils.DrawGizmoGrid(
        bottomLeft, 
        Asset.Settings.GridSize, 
        Asset.Settings.GridSize, 
        Asset.Settings.GridNodeSize,
        GizmoUtils.Blue.Alpha(0.125f)
      );
    }
  }
}
