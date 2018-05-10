using UnityEngine;

[CreateAssetMenu(menuName = "Quantum/Assets/Physics/NavMesh Agent Config")]
public partial class NavMeshAgentConfigAsset : AssetBase {
  public Quantum.NavMeshAgentConfig Settings;

  public override Quantum.AssetObject AssetObject {
    get { return Settings; }
  }
}
