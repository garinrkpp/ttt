using Photon.Deterministic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UE = UnityEngine;

#if UNITY_EDITOR
using UED = UnityEditor;
#endif

public unsafe partial class QuantumGame : IDeterministicGame {

  static Quantum.Frame _frame;
  static Quantum.Frame _framePredicted;
  static Quantum.Frame _framePredictedPrevious;
  static Quantum.Frame _frameVerified;

  static Quantum.PlayerRef[] _localPlayers;

  static public Boolean Running {
    get { return _frame != null; }
  }

  static public Quantum.Frame Frame {
    get { return _frame; }
  }

  static public Quantum.Frame FramePredicted {
    get { return _framePredicted; }
  }

  static public Quantum.Frame FramePredictedPrevious {
    get { return _framePredictedPrevious; }
  }

  static public Quantum.Frame FrameVerified {
    get { return _frameVerified; }
  }

  [Obsolete("Use LocalPlayers Array Instead")]
  static public Quantum.PlayerRef LocalPlayer {
    get {
      if (_localPlayers == null || _localPlayers.Length == 0) {
        return Quantum.PlayerRef.None;
      }

      return _localPlayers[0];
    }
  }

  static public Quantum.PlayerRef[] LocalPlayers {
    get {
      return _localPlayers;
    }
  }

  static public Single FrameInterpolationFactor {
    get;
    private set;
  }

  static public Quantum.RuntimeConfig RuntimeConfig {
    get;
    private set;
  }

  static public Quantum.SimulationConfig SimulationConfig {
    get;
    private set;
  }

  static public Func<PhotonPlayer, Quantum.PlayerRef> PhotonPlayerToQuantumPlayer {
    get;
    private set;
  }

  static public Func<Quantum.PlayerRef, PhotonPlayer> QuantumPlayerToPhotonPlayer {
    get;
    private set;
  }

  static public Boolean IsReplay {
    get;
    private set;
  }

  static public Boolean IsReplayFinished {
    get;
    private set;
  }

  static public Boolean IsLocalPlayer(Quantum.PlayerRef playerRef) {
    if (_localPlayers == null) {
      return false;
    }

    if (playerRef == Quantum.PlayerRef.None) {
      return false;
    }

    for (Int32 i = 0; i < _localPlayers.Length; ++i) {
      if (_localPlayers[i] != Quantum.PlayerRef.None && _localPlayers[i] == playerRef) {
        return true;
      }
    }

    return false;
  }

  // current sesion
  DeterministicSession _session;

  // load op
  UE.AsyncOperation _mapLoad;

  // expected size of input
  Int32 _inputSize;

  Quantum.Map _map;
  Quantum.ByteStream _inputStreamRead;
  Quantum.ByteStream _inputStreamWrite;
  Quantum.SystemBase[] _systems;
  Quantum.RuntimeConfig _runtimeConfig;
  Quantum.SimulationConfig _simulationConfig;

  public QuantumGame(Quantum.RuntimeConfig runtimeConfig) {
    // init debug
    QuantumRunner.Init();

    // initialize db
    UnityDB.Init();

    _runtimeConfig = RuntimeConfig = runtimeConfig;
    _simulationConfig = SimulationConfig = SimulationConfigAsset.Instance.Configuration;
    _systems = Quantum.SystemSetup.CreateSystems(_runtimeConfig, _simulationConfig);

    IsReplay = runtimeConfig.GameMode == DeterministicGameMode.Replay;
    IsReplayFinished = false;

    // set system runtime indices
    for (Int32 i = 0; i < _systems.Length; ++i) {
      _systems[i].RuntimeIndex = i;
    }
  }

  public void OnDestroy() {
    _frame = null;
    _framePredicted = null;
    _framePredictedPrevious = null;
    _frameVerified = null;

    _localPlayers = null;

    RuntimeConfig = null;
    SimulationConfig = null;

    PhotonPlayerToQuantumPlayer = null;
    QuantumPlayerToPhotonPlayer = null;

    IsReplay = false;
    IsReplayFinished = false;

    FrameInterpolationFactor = 0;
  }

  public DeterministicFrame CreateFrame() {
    return new Quantum.Frame(_systems, _runtimeConfig, _simulationConfig, _session.DeltaTime, _session.SimulationRate);
  }

  public void OnGameEnded() {
  }

  public void OnGameStart(DeterministicFrame f) {
    _inputStreamRead = new Quantum.ByteStream(new Byte[1024]);
    _inputStreamWrite = new Quantum.ByteStream(new Byte[1024]);
    _localPlayers = _session.LocalPlayerIndices.Select(x => (Quantum.PlayerRef)x).ToArray();

    // set public delegates for converting between photon/quantum players
    PhotonPlayerToQuantumPlayer = PlayerToIndex;
    QuantumPlayerToPhotonPlayer = IndexToPlayer;

    // sort runtime players
    SortRuntimePlayers();

    // init event invoker
    InitEventInvoker(_session.RollbackWindow);

    // init systems on latest frame
    InitSystems(f);

    _frame = (Quantum.Frame)f;
    _framePredicted = _frame;
    _framePredictedPrevious = _frame;
    _frameVerified = _frame;

    for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
      try {
        QuantumCallbacks.Instances[i].OnGameStart();
      }
      catch (Exception exn) {
        Quantum.Log.Exception(exn);
      }
    }
  }

  public Tuple<Byte[], DeterministicInputFlags> OnLocalInput(Int32 player) {
    var input = default(Tuple<Quantum.Input, DeterministicInputFlags>);

    // poll input
    if (QuantumInput.Instance) {
      try {
        input = QuantumInput.Instance.PollInput(player);
      }
      catch (Exception exn) {
        Quantum.Log.Error("## Input Code Threw Exception ##");
        Quantum.Log.Exception(exn);
      }
    }
    else {
      Quantum.Log.Warn("No QuantumInput instance found");
    }

    // clear old data
    _inputStreamWrite.Reset();

    // pack into stream
    Quantum.Input.Write(_inputStreamWrite, input.Item0);

    // send input via backend
    return Tuple.Create(_inputStreamWrite.ToArray(), input.Item1);
  }

  public void OnSimulate(DeterministicFrame state) {
    var f = (Quantum.Frame)state;

    try {
      ApplyInputs(f);

      f.PreSimulatePrepare();

      for (Int32 i = 0; i < _systems.Length; ++i) {
        if (Quantum.BitSet256.IsSet(&f.Global->Systems, i)) {
          try {
            _systems[i].Update(f);
          }
          catch (Exception exn) {
            LogSimulationException(exn);
          }
        }
      }

      f.PostSimulateCleanup();
    }
    catch (Exception exn) {
      LogSimulationException(exn);
    }
  }

  public void OnSimulateFinished(DeterministicFrame state) {
    InvokeEvents((Quantum.Frame)state);
  }

  public void OnUpdateDone() {
    if (_session.IsReplayFinished) {
      IsReplayFinished = true;
    }

    _framePredicted = _frame = (Quantum.Frame)_session.FramePredicted;
    _framePredictedPrevious = (Quantum.Frame)_session.FramePredictedPrevious;
    _frameVerified = (Quantum.Frame)_session.FrameVerified;

    FrameInterpolationFactor = UnityEngine.Mathf.Clamp01((Single)_session.AccumulatedTime / _frame.DeltaTime.AsFloat);

    Quantum.Core.DebugDraw.DrawAll();

    for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
      try {
        QuantumCallbacks.Instances[i].OnUpdateView();
      }
      catch (Exception exn) {
        Quantum.Log.Exception(exn);
      }
    }

    SyncMap();
  }

  public void AssignSession(DeterministicSession session) {
    _session = session;
  }

  public void OnChecksumError(DeterministicTickChecksumError error, DeterministicFrame[] frames) {
    var castedFrames = frames.Cast<Quantum.Frame>().ToArray();

    for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
      try {
        QuantumCallbacks.Instances[i].OnChecksumError(error, castedFrames);
      }
      catch (Exception exn) {
        Quantum.Log.Exception(exn);
      }
    }
  }

  PhotonPlayer IndexToPlayer(Quantum.PlayerRef index) {
    if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
      var id = _session.PlayerIndexToId(index);

      // find correct photon player for this player index
      return PhotonNetwork.playerList.FirstOrDefault(x => x.ID == id);
    }

    return null;
  }

  Quantum.PlayerRef PlayerToIndex(PhotonPlayer player) {
    var index = _session.PlayerIdToIndex(player.ID);
    if (index == -1) {
      return Quantum.PlayerRef.None;
    }

    return index;
  }

  void SortRuntimePlayers() {
    if (_runtimeConfig.GameMode == DeterministicGameMode.Multiplayer) {
      var players = PhotonNetwork.playerList.OrderBy(x => x.ID).ToArray();
      var runtimePlayers = new Quantum.RuntimePlayer[_runtimeConfig.MaxPlayers];

      for (Int32 currentIndex = 0; currentIndex < runtimePlayers.Length; ++currentIndex) {
        // calculate correct index
        if (currentIndex < players.Length) {
          var correctIndex = _session.PlayerIdToIndex(players[currentIndex].ID);
          if (correctIndex != -1) {
            // assign from current index to correct index
            runtimePlayers[correctIndex] = _runtimeConfig.Players[currentIndex];
          }
        }
      }

      for (Int32 i = 0; i < runtimePlayers.Length; ++i) {
        if (runtimePlayers[i] == null) {
          runtimePlayers[i] = new Quantum.RuntimePlayer();
        }
      }

      _runtimeConfig.Players = runtimePlayers;
    }
  }

  void SyncMap() {
    if (_simulationConfig.AutoLoadSceneFromMap) {
      if (_mapLoad == null) {
        var current = _frameVerified.Global->Map.Asset;
        if (current != null && current != _map) {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnMapChangeBegin();
            }
            catch (Exception exn) {
              Quantum.Log.Exception(exn);
            }
          }

          _map = current;
          _mapLoad = SceneManager.LoadSceneAsync(current.Scene);
        }
      }
      else {
        if (_mapLoad.isDone) {
          _mapLoad = null;

          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnMapChangeDone();
            }
            catch (Exception exn) {
              Quantum.Log.Exception(exn);
            }
          }
        }
      }
    }
  }

  public void OnDrawGizmos() {
#if UNITY_EDITOR
    if (_frame != null) {

      //for (Int32 x = 0; x < _frame.Map.GridSize; x++) {
      //  for (Int32 y = 0; y < _frame.Map.GridSize; y++) {
      //    var cell = _frame.Scene._broadphaseCells[x + y * _frame.Map.GridSize];
      //    var center = new FPVector2((x * _frame.Map.GridNodeSize) + (_frame.Map.GridNodeSize * FP._0_50), (y * _frame.Map.GridNodeSize) + (_frame.Map.GridNodeSize * FP._0_50)) + _frame.Map.WorldOffset;
      //    for(Int32 i = 0; i < cell.StaticCount; ++i) {
      //      var s = cell.StaticEntities[i];
      //      UnityEngine.Debug.DrawLine(center.ToUnityVector3(), s.Transform2D->Position.ToUnityVector3());
      //    }
      //  }
      //}

      var dynamics = _frame.Scene.DynamicEntities;

      for (Int32 i = dynamics.Length - 1; i >= 0; --i) {
        var b = dynamics[i].DynamicBody;
        if (b->Enabled) {
          var t = dynamics[i].Transform2D;
          var s = b->GetShape();

          switch (s.Type) {
            case Quantum.Core.DynamicShapeType.Circle:
              Quantum.GizmoUtils.DrawGizmosCircle(t->Position.ToUnityVector3(), s.Circle.Radius.AsFloat, Quantum.ColorRGBA.ColliderBlue.ToColor());
              break;

            case Quantum.Core.DynamicShapeType.Box:
              UE.Gizmos.matrix = UE.Matrix4x4.TRS(t->Position.ToUnityVector3(), t->Rotation.ToUnityQuaternion(), UE.Vector3.one);
              Quantum.GizmoUtils.DrawGizmosBox(FPVector2.Zero, s.Box.Extents * 2, Quantum.ColorRGBA.ColliderBlue.ToColor());
              UE.Gizmos.matrix = UE.Matrix4x4.identity;
              break;

            case Quantum.Core.DynamicShapeType.Polygon:
              var p = (Quantum.PolygonCollider)Quantum.DB.FastUnsafe[s.Polygon.AssetId];
              if (p != null) {
                UED.Handles.matrix = UE.Matrix4x4.TRS(t->Position.ToUnityVector3(), t->Rotation.ToUnityQuaternion(), UE.Vector3.one);
                UED.Handles.color = Quantum.ColorRGBA.ColliderBlue.ToColor();
                UED.Handles.DrawAAConvexPolygon(p.Vertices.Select(x => x.ToUnityVector3()).ToArray());
                UED.Handles.color = UE.Color.white;
                UED.Handles.matrix = UE.Matrix4x4.identity;
              }
              break;
          }
        }
      }
    }
#endif
  }

  void InitSystems(DeterministicFrame f) {
    // call init on ALL systems
    for (Int32 i = 0; i < _systems.Length; ++i) {
      try {
        _systems[i].OnInit((Quantum.Frame)f);
      }
      catch (Exception exn) {
        LogSimulationException(exn);
      }
    }

    // call OnEnabled on all systems which start enabled
    for (Int32 i = 0; i < _systems.Length; ++i) {
      if (_systems[i].StartEnabled) {
        try {
          _systems[i].OnEnabled((Quantum.Frame)f);
        }
        catch (Exception exn) {
          LogSimulationException(exn);
        }
      }
    }
  }

  void ApplyInputs(Quantum.Frame f) {
    for (Int32 i = 0; i < f.RuntimeConfig.Players.Length; i++) {
      var data = f.GetRawInput(i);
      if (data == null || data.Length == 0) {
        f.SetPlayerInput(i, default(Quantum.Input));
        continue;
      }

      // copy into stream
      _inputStreamRead.Reset();
      _inputStreamRead.CopyFromArray(data);

      Quantum.Input input;

      // try read input and assign it
      if (ReadInputFromStream(out input)) {
        f.SetPlayerInput(i, input);
      }
      else {
        Quantum.Log.Error("Received invalid input data from player {0}, could not deserialize.", i);
        f.SetPlayerInput(i, default(Quantum.Input));
      }
    }
  }

  Boolean ReadInputFromStream(out Quantum.Input input) {
    try {
      input = Quantum.Input.Read(_inputStreamRead);
      return true;
    }
    catch {
      input = default(Quantum.Input);
      return false;
    }
  }

  void LogSimulationException(Exception exn) {
    Quantum.Log.Error("## Simulation Code Threw Exception ##");
    Quantum.Log.Exception(exn);
  }

}
