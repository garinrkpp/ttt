using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/Assets/Physics/Physics Material")]
public partial class PhysicsMaterialAsset : AssetBase {
  public Quantum.PhysicsMaterial Settings;

  public override Quantum.AssetObject AssetObject {
    get { return Settings; }
  }
}
