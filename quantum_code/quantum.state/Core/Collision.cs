using Photon.Deterministic;
using System;

namespace Quantum {
  public interface ISignalOnCollisionDynamic {
    Int32 RuntimeIndex { get; }
    void OnCollisionDynamic(Frame f, DynamicCollisionInfo info);
  }

  public interface ISignalOnCollisionStatic {
    Int32 RuntimeIndex { get; }
    void OnCollisionStatic(Frame f, StaticCollisionInfo info);
  }

  public interface ISignalOnTriggerDynamic {
    Int32 RuntimeIndex { get; }
    void OnTriggerDynamic(Frame f, DynamicCollisionInfo info);
  }

  public interface ISignalOnTriggerStatic {
    Int32 RuntimeIndex { get; }
    void OnTriggerStatic(Frame f, StaticCollisionInfo info);
  }

  public unsafe struct StaticCollisionInfo {
    public Entity* Entity;
    public StaticColliderData StaticData;
    public FPVector2 ContactPoint;
    public FPVector2 ContactNormal;
    public FP Penetration;
  }

  public unsafe struct DynamicCollisionInfo {
    public Entity* EntityA;
    public Entity* EntityB;
    public FPVector2 ContactPoint;
    public FPVector2 ContactNormal;
    public FP Penetration;
  }
}
