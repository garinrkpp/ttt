using Photon.Deterministic;
using Quantum;
using Quantum.Core;
using System;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine;

public sealed class QuantumRunner : MonoBehaviour {
  public static QuantumRunner Current;

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

  void OnDrawGizmos() {
#if UNITY_EDITOR
    if (_session != null) {
      var game = _session.Game as QuantumGame;
      if (game != null) {
        game.OnDrawGizmos();
      }
    }
#endif
  }

  public void Shutdown() {
    Destroy(gameObject);
    Current = null;
  }

#if UNITY_SWITCH && !UNITY_EDITOR
  static unsafe class __Internal {
    [SuppressUnmanagedCodeSecurity]
    [DllImport("__Internal", EntryPoint = "egmemcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static unsafe extern void* memcpy(void* dest, void* src, UInt64 count);

    public static void* MemCpyCaller(void* dest, void* src, UInt64 count) {
      return __Internal.memcpy(dest, src, count);
    }
  }
#endif

  public static void Init(Boolean force = false) {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    Quantum.Native.Impl = Quantum.Native.NativeImpl.MSCVCRT;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
    Quantum.Native.Impl = Quantum.Native.NativeImpl.LIBC;
#elif UNITY_ANDROID
    Quantum.Native.Impl = Quantum.Native.NativeImpl.C;
#elif UNITY_SWITCH && !UNITY_EDITOR
    unsafe {
      Quantum.Native.Impl = Quantum.Native.NativeImpl.DELEGATE;
      Quantum.Native.MemCpyDelegate = __Internal.MemCpyCaller;
    }
#endif

    // load lookup table
    FPMathUtils.LoadLookupTables(force);

    // init profiler
    Quantum.Profiler.Init(x => UnityEngine.Profiling.Profiler.BeginSample(x), () => UnityEngine.Profiling.Profiler.EndSample());

    // init debug draw functions
    Quantum.Draw.Init(
      Quantum.Core.DebugDraw.Ray,
      Quantum.Core.DebugDraw.Line,
      Quantum.Core.DebugDraw.Circle,
      Quantum.Core.DebugDraw.Rectangle
    );

    // init quantum logger
    Quantum.Log.Init(
      UnityEngine.Debug.Log,
      UnityEngine.Debug.LogWarning,
      UnityEngine.Debug.LogError,
      UnityEngine.Debug.LogException
    );

    // init photon logger
    Photon.Deterministic.DeterministicLog.Init(
      UnityEngine.Debug.Log,
      UnityEngine.Debug.LogWarning,
      UnityEngine.Debug.LogError,
      UnityEngine.Debug.LogException
    );
  }

  public static void StartGame(RuntimeConfig runtimeConfig) {

    CheckRunnerIsFree();

    Quantum.Log.Info("Starting Game");

    if (runtimeConfig.GameMode == DeterministicGameMode.Multiplayer) {
      if (PhotonNetwork.connected == false) {
        throw new Exception("Not connected to photon");
      }

      if (PhotonNetwork.inRoom == false) {
        throw new Exception("Can't start networked game when not in a room");
      }

      if (runtimeConfig.Players.Length != PhotonNetwork.room.MaxPlayers) {
        throw new Exception("Less RuntimePlayer instances provided on RuntimeConfig than MaxPlayers set on room");
      }
    }

    Current = CreateInstance();
    Current._session = new DeterministicSession(DeterministicSessionConfigAsset.Instance.Config, new QuantumGame(runtimeConfig), GetCommunicator(runtimeConfig), runtimeConfig);
  }

  static QuantumNetworkCommunicator GetCommunicator(RuntimeConfig runtimeConfig) {
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

  static QuantumRunner CreateInstance() {
    GameObject go;

    go = new GameObject("QuantumRunner");
    go.AddComponent<QuantumRunner>();

    DontDestroyOnLoad(go);

    return go.GetComponent<QuantumRunner>();
  }

}

