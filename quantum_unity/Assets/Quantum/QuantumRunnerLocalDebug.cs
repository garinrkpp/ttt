using System;
using UnityEngine;

public class QuantumRunnerLocalDebug : MonoBehaviour {
  public Quantum.RuntimeConfig Config;

  void Start() {
    if (PhotonNetwork.connected) {
      return;
    }

    if (QuantumRunner.Current) {
      return;
    }

    Debug.Log("### Starting quantum in local debug mode ###");

    var mapdata = FindObjectOfType<MapData>();
    if (mapdata) {
      // set singleplayer
      Config.GameMode = Photon.Deterministic.DeterministicGameMode.Local;

      // set map to this maps asset
      Config.Map.Guid = mapdata.Asset.AssetObject.Guid;

      // start with debug config
      QuantumRunner.StartGame(Config);
    }
    else {
      throw new Exception("No MapData object found, can't debug start scene");
    }
  }
}
