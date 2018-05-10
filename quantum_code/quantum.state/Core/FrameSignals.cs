using System;

namespace Quantum {
  partial class Frame {
    public unsafe partial class FrameSignals {
      Frame _f;

      public FrameSignals(Frame f) {
        _f = f;
      }

      public void OnEntityDestroy(Entity* entity) {
        var array = _f._ISignalOnEntityDestroySystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnEntityDestroy(_f, entity);
          }
        }
      }

      public void OnEntityCreated(Entity* entity) {
        var array = _f._ISignalOnEntityCreateSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnEntityCreated(_f, entity);
          }
        }
      }

      public void OnCollisionDynamic(DynamicCollisionInfo info) {
        var array = _f._ISignalOnCollisionDynamicSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionDynamic(_f, info);
          }
        }
      }

      public void OnNavMeshTargetReached(Core.NavMeshAgent* agent, Entity* entity)
      {
        var array = _f._ISignalOnNavMeshTargetReachedSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i)
        {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex))
          {
            s.OnNavMeshTargetReached(_f, agent, entity);
          }
        }
      }

      public void OnCollisionStatic(StaticCollisionInfo info) {
        var array = _f._ISignalOnCollisionStaticSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnCollisionStatic(_f, info);
          }
        }
      }

      public void OnTriggerDynamic(DynamicCollisionInfo info) {
        var array = _f._ISignalOnTriggerDynamicSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerDynamic(_f, info);
          }
        }
      }

      public void OnTriggerStatic(StaticCollisionInfo info) {
        var array = _f._ISignalOnTriggerStaticSystems;
        var systems = &(_f._globals->Systems);
        for (Int32 i = 0; i < array.Length; ++i) {
          var s = array[i];
          if (BitSet256.IsSet(systems, s.RuntimeIndex)) {
            s.OnTriggerStatic(_f, info);
          }
        }
      }
    }
  }
}
