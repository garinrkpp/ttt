using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/Assets/Physics/Polygon Collider")]
public partial class PolygonColliderAsset : AssetBase {
  public Quantum.PolygonCollider Settings;

  public override Quantum.AssetObject AssetObject {
    get { return Settings; }
  }
}
