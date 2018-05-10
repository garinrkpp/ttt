using Photon.Deterministic;
using Quantum.Core;
using System;
using UnityEngine;

public sealed class ExampleQuantumRunner : MonoBehaviour {
  public static ExampleQuantumRunner Current {
    get;
    private set;
  }

  public DeterministicSession Session {
    get { return _session; }
  }

  DeterministicSession _session;

  void Update() {
    if (_session != null) {
      _session.Update();
    }
  }

  void OnDestroy() {
    if (_session != null) {
      _session.Destroy();
      _session = null;
    }
  }

  public void Shutdown() {
    Destroy(gameObject);
    Current = null;
  }

  public static void Init(Boolean force = false) {
    // load lookup table
    FPLut.Init(file => UnityEngine.Resources.Load<TextAsset>("LUT/" + file).bytes);

    // init photon logger
    Photon.Deterministic.DeterministicLog.Init(
      UnityEngine.Debug.Log,
      UnityEngine.Debug.LogWarning,
      UnityEngine.Debug.LogError,
      UnityEngine.Debug.LogException
    );
  }

  public static void StartGame(DeterministicRuntimeConfig runtimeConfig) {
    Init();

    CheckRunnerIsFree();

    if (runtimeConfig.GameMode == DeterministicGameMode.Multiplayer) {
      if (PhotonNetwork.connected == false) {
        throw new Exception("Not connected to photon");
      }

      if (PhotonNetwork.inRoom == false) {
        throw new Exception("Can't start networked game when not in a room");
      }

      if (runtimeConfig.MaxPlayers != PhotonNetwork.room.MaxPlayers) {
        throw new Exception("Less RuntimePlayer instances provided on RuntimeConfig than MaxPlayers set on room");
      }
    }

    Current = CreateInstance();
    Current._session = new DeterministicSession(DeterministicSessionConfigAsset.Instance.Config, new ExampleDeterministicGame(runtimeConfig), GetCommunicator(runtimeConfig), runtimeConfig);
  }

  static QuantumNetworkCommunicator GetCommunicator(DeterministicRuntimeConfig runtimeConfig) {
    if (runtimeConfig.GameMode != DeterministicGameMode.Multiplayer) {
      return null;
    }

    return new QuantumNetworkCommunicator(PhotonNetwork.networkingPeer, false);
  }

  static void CheckRunnerIsFree() {
    if (Current) {
      throw new Exception("QuantumRunner already exists, please call Shutdown on the existing one to start a game");
    }
  }

  static ExampleQuantumRunner CreateInstance() {
    GameObject go;

    go = new GameObject("QuantumRunner");
    go.AddComponent<ExampleQuantumRunner>();

    DontDestroyOnLoad(go);

    return go.GetComponent<ExampleQuantumRunner>();
  }

}

