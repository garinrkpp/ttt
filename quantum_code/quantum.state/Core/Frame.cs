using Photon.Deterministic;
using Quantum.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantum {
  public unsafe partial class Frame : Core.FrameBase, Core.IFrameInternal {
    _globals_* _globals;
    _entities_* _entities;

    Int32 _simulationRate;

    // meta classes
    FrameEvents _frameEvents;
    FrameSignals _frameSignals;

    // events
    EventBase _events;

    // dynamic scene
    DynamicScene _scene;

    // animator
    AnimatorUpdater _animator;

    // navmesh
    NavMeshUpdater _navMeshUpdater;

    // configs
    RuntimeConfig _runtimeConfig;
    SimulationConfig _simulationConfig;

    // systems
    SystemBase[] _systems;

    // 
    Queue<EntityRef> _destroy;

    IEntityManifoldFilter _entityManifoldFilter;

    ISignalOnEntityCreated[] _ISignalOnEntityCreateSystems;
    ISignalOnEntityDestroy[] _ISignalOnEntityDestroySystems;

    ISignalOnCollisionStatic[] _ISignalOnCollisionStaticSystems;
    ISignalOnCollisionDynamic[] _ISignalOnCollisionDynamicSystems;

    ISignalOnTriggerStatic[] _ISignalOnTriggerStaticSystems;
    ISignalOnTriggerDynamic[] _ISignalOnTriggerDynamicSystems;

    ISignalOnNavMeshTargetReached[] _ISignalOnNavMeshTargetReachedSystems;

    public _globals_* Global {
      get {
        return _globals;
      }
    }

    public RNGSession* RNG {
      get {
        return &_globals->RngSession;
      }
    }

    public FP DeltaTime {
      get {
        return _globals->DeltaTime;
      }
      set {
        _globals->DeltaTime = value;
      }
    }

    public Int32 SimulationRate {
      get {
        return _simulationRate;
      }
    }

    public Map Map {
      get {
        return _globals->Map.Asset;
      }
      set {
        _globals->Map.Asset = value;
      }
    }

    public DynamicScene Scene {
      get {
        return _scene;
      }
    }

    public AnimatorUpdater AnimatorUpdater {
      get {
        return _animator;
      }
    }

    public NavMeshUpdater NavMeshUpdater
    {
      get
      {
        return _navMeshUpdater;
      }
    }

    public FrameSignals Signals {
      get {
        return _frameSignals;
      }
    }

    public FrameEvents Events {
      get { return _frameEvents; }
    }

    public RuntimeConfig RuntimeConfig {
      get {
        return _runtimeConfig;
      }
    }

    public SimulationConfig SimulationConfig {
      get {
        return _simulationConfig;
      }
    }

    public SystemBase[] Systems {
      get { return _systems; }
    }

    public override IEntityManifoldFilter EntityManifoldFilter {
      get {
        return _entityManifoldFilter;
      }
    }

    EventBase IFrameInternal.EventHead {
      get {
        return _events;
      }
    }

    public Frame(SystemBase[] systems, RuntimeConfig runtimeConfig, SimulationConfig simulationConfig, FP deltaTime, Int32 simulationRate) {
      _systems = systems;
      _runtimeConfig = runtimeConfig;
      _simulationConfig = simulationConfig;
      _simulationRate = simulationRate;

      _frameEvents = new FrameEvents(this);
      _frameSignals = new FrameSignals(this);
      _destroy = new Queue<EntityRef>(1024);

      _entityManifoldFilter = _systems.FirstOrDefault(x => x is IEntityManifoldFilter) as IEntityManifoldFilter;

      // use dummy in case no system implements the filter
      if (_entityManifoldFilter == null) {
        _entityManifoldFilter = new DummyManifoldFilter();
      }

      AllocGen();
      InitGen();

      _ISignalOnEntityCreateSystems = BuildSignalsArray<ISignalOnEntityCreated>();
      _ISignalOnEntityDestroySystems = BuildSignalsArray<ISignalOnEntityDestroy>();

      _ISignalOnCollisionStaticSystems = BuildSignalsArray<ISignalOnCollisionStatic>();
      _ISignalOnCollisionDynamicSystems = BuildSignalsArray<ISignalOnCollisionDynamic>();

      _ISignalOnTriggerStaticSystems = BuildSignalsArray<ISignalOnTriggerStatic>();
      _ISignalOnTriggerDynamicSystems = BuildSignalsArray<ISignalOnTriggerDynamic>();

      _ISignalOnNavMeshTargetReachedSystems = BuildSignalsArray<ISignalOnNavMeshTargetReached>();

      // assign map, rng session, etc.
      _globals->Map.Asset = runtimeConfig.Map.Instance;
      _globals->RngSession = new RNGSession(runtimeConfig.Seed);
      _globals->DeltaTime = deltaTime;

      // set default enabled systems
      for (Int32 i = 0; i < _systems.Length; ++i) {
        if (_systems[i].StartEnabled) {
          BitSet256.Set(&_globals->Systems, i);
        }
      }

      var allEntities = GetAllEntitiesUnsafe();

      // init physics
      var physicsEntities = new List<DynamicScene.DynamicSceneEntity>();

      for (Int32 i = 0; i < allEntities.Length; ++i) {
        var e = allEntities[i];
        var d = Entity.GetDynamicBody(e.Entity);
        var t = Entity.GetTransform2D(e.Entity);
        var tv = Entity.GetTransform2DVertical(e.Entity);
        if (tv == null)
        {
          tv = &_globals->PhysicsSettings.DefaultVerticalTransform;
        }

        if (d != null)
        {
          physicsEntities.Add(new DynamicScene.DynamicSceneEntity
          {
            Entity = (void*)e.Entity,
            DynamicBody = d,
            Transform2D = t,
            Transform2DVertical = tv,
          });
        }
      }

      // init physics
      _globals->PhysicsSettings.Gravity = simulationConfig.Physics.Gravity;
      _globals->PhysicsSettings.SolverIterations = simulationConfig.Physics.SolverIterations;
      _globals->PhysicsSettings.UseAngularVelocity = simulationConfig.Physics.UseAngularVelocity;
      _globals->PhysicsSettings.Substeps = simulationConfig.Physics.Substeps;
      _globals->PhysicsSettings.RaiseCollisionEventsForStatics = simulationConfig.Physics.RaiseCollisionEventsForStatics;

      // create scene
      _scene = new DynamicScene(&_globals->PhysicsSettings, physicsEntities.ToArray());

      // init animator
      var animatorEntities = new List<AnimatorUpdater.AnimatorEntity>();

      for (Int32 i = 0; i < allEntities.Length; ++i) {
        var e = allEntities[i];
        var a = Entity.GetAnimator(e.Entity);
        if (a != null) {
          animatorEntities.Add(new AnimatorUpdater.AnimatorEntity {
            Entity = (void*)e.Entity,
            Animator = a,
            Transform = Entity.GetTransform2D(e.Entity),
            DynamicBody = Entity.GetDynamicBody(e.Entity)
          });
        }
      }

      _animator = new AnimatorUpdater(animatorEntities.ToArray());

      // init animator
      var navMeshEntites = new List<NavMeshUpdater.NavMeshEntity>();

      for (Int32 i = 0; i < allEntities.Length; ++i)
      {
        var e = allEntities[i];
        var a = Entity.GetNavMeshAgent(e.Entity);
        if (a != null)
        {
          navMeshEntites.Add(new NavMeshUpdater.NavMeshEntity
          {
            Entity = (void*)e.Entity,
            Agent = a,
            Transform2D = Entity.GetTransform2D(e.Entity),
            DynamicBody = Entity.GetDynamicBody(e.Entity),
            Index = i
          });
        }
      }

      NavMeshUpdater.NavMeshUpdaterConfig navMeshConfig = new NavMeshUpdater.NavMeshUpdaterConfig()
      {
        ProximityFactor = simulationConfig.NavMeshAgent.ProximityFactor,
        UpdateInterval = simulationConfig.NavMeshAgent.UpdateInterval
      };

      _navMeshUpdater = new NavMeshUpdater(navMeshEntites.ToArray(), Scene, Map, navMeshConfig);

      AllocUser();
      InitUser();
    }

    public sealed override void Reset() {
      // release all events
      try {
        var head = _events;

        while (head != null) {
          // copy to tmp
          var tmp = head;

          // grab tail
          head = tmp._tail;

          // release event
          tmp.Release();
        }
      } catch (Exception exn) {
        Log.Exception(exn);

      } finally {
        // clear event head
        _events = null;
      }
    }

    public String DumpFrame() {
      return FrameDiffer.Dump("Globals", *_globals) + FrameDiffer.Dump("Entities", *_entities);
    }

    public sealed override UInt64 CalculateChecksum() {
      var globals = CRC64.Calculate(0, (Byte*)_globals, sizeof(_globals_));
      var entities = CRC64.Calculate(0, (Byte*)_entities, sizeof(_entities_));

      unchecked {
        UInt64 hash = 17UL;
        hash = hash * 31UL + globals;
        hash = hash * 31UL + entities;
        return hash;
      }
    }

    public sealed override void CopyFrom(DeterministicFrame frame) {
      CopyFromGen((Frame)frame);
      CopyFromUser((Frame)frame);
    }

    public sealed override void Free() {
      FreeGen();
      FreeUser();
    }

    public Boolean SystemIsEnabled<T>() where T : SystemBase {
      var system = FindSystem<T>();
      if (system.Item0 == null) {
        return false;
      }

      return BitSet256.IsSet(&_globals->Systems, system.Item1);
    }

    public void SystemEnable<T>() where T : SystemBase {
      var system = FindSystem<T>();
      if (system.Item0 == null) {
        return;
      }

      if (BitSet256.IsSet(&_globals->Systems, system.Item1) == false) {
        // set flag
        BitSet256.Set(&_globals->Systems, system.Item1);

        try {
          system.Item0.OnEnabled(this);
        } catch (Exception exn) {
          Log.Exception(exn);
        }
      }
    }

    public void SystemDisable<T>() where T : SystemBase {
      var system = FindSystem<T>();
      if (system.Item0 == null) {
        return;
      }

      if (BitSet256.IsSet(&_globals->Systems, system.Item1)) {
        // clear flag
        BitSet256.Clear(&_globals->Systems, system.Item1);

        try {
          system.Item0.OnDisabled(this);
        } catch (Exception exn) {
          Log.Exception(exn);
        }
      }
    }

    public void PreSimulatePrepare() {
      _destroy.Clear();
    }

    public void PostSimulateCleanup() {
      while (_destroy.Count > 0) {
        DestroyEntityInternal(GetEntity(_destroy.Dequeue()));
      }
    }

    public Boolean EntityExists(EntityRef entityRef) {
      return GetEntity(entityRef) != null;
    }

    Tuple<SystemBase, Int32> FindSystem<T>() {
      for (Int32 i = 0; i < _systems.Length; ++i) {
        if (_systems[i].GetType() == typeof(T)) {
          return Tuple.Create(_systems[i], i);
        }
      }

      Log.Error("System '{0}' not found, did you forget to add it to SystemSetup.CreateSystems ?", typeof(T).Name);
      return new Tuple<SystemBase, Int32>(null, -1);
    }

    T[] BuildSignalsArray<T>() {
      return _systems.Where(x => x is T).Cast<T>().ToArray();
    }

    void AddEvent(EventBase evnt) {
      // cons tail
      evnt._tail = _events;

      // overwrite head pointer
      _events = evnt;
    }

    // partial declarations populated from code generator
    partial void InitGen();
    partial void FreeGen();
    partial void AllocGen();
    partial void CopyFromGen(Frame frame);

    partial void InitUser();
    partial void FreeUser();
    partial void AllocUser();
    partial void CopyFromUser(Frame frame);

    // entity create and destroy methods
    void EntityCreate(Entity* entity) {
      Assert.Check(entity->_active == false);
      entity->_ref._version += 1;
      entity->_active = true;
    }

    void EntityDestroy(Entity* entity) {
      Assert.Check(entity->_active);
      entity->_ref._version += 1;
      entity->_active = false;
    }

    class DummyManifoldFilter : IEntityManifoldFilter {
      public Boolean Filter(FrameBase frame, DynamicScene.Manifold manifold) {
        return true;
      }
    }
  }
}
