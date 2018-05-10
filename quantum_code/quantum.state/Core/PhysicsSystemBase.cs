using System;
using Quantum.Core;

namespace Quantum
{
  public unsafe partial class PhysicsSystemBase : SystemBase, Core.ICollisionCallbacks
  {
    public Frame _f;
    public override void Update(Frame f)
    {
      _f = f;
      f.Scene.Update(f, f.Map, f.DeltaTime, this);
      _f = null;
    }
    public void OnCollision(DynamicScene.Manifold manifold)
    {
      var aIsNull = manifold.A.Entity == null;
      if (aIsNull || manifold.B.Entity == null)
      {
        StaticCollisionInfo info;
        info = default(StaticCollisionInfo);
        info.Entity = (Entity*)(aIsNull ? manifold.B.Entity : manifold.A.Entity);
        info.StaticData = aIsNull ? manifold.A.StaticData : manifold.B.StaticData;
        info.ContactNormal = manifold.ContactNormal;
        info.ContactPoint = manifold.GetContactPoint(0);
        info.Penetration = manifold.Penetration;
        _f.Signals.OnCollisionStatic(info);
        var typesHash = (Int32)info.Entity->Type;
        OnCollisionInternal(info, typesHash);
      } else
      {
        DynamicCollisionInfo info;
        info.EntityA = (Entity*)manifold.A.Entity;
        info.EntityB = (Entity*)manifold.B.Entity;
        info.ContactNormal = manifold.ContactNormal;
        info.ContactPoint = manifold.GetContactPoint(0);
        info.Penetration = manifold.Penetration;
        _f.Signals.OnCollisionDynamic(info);
        var typesHash = (Int32)info.EntityA->Type | ((Int32)info.EntityB->Type << 16);
        OnCollisionInternal(info, typesHash);
      }
    }

    partial void OnCollisionInternal(StaticCollisionInfo info, Int32 typesHash);
    partial void OnTriggerInternal(StaticCollisionInfo info, Int32 typesHash);
    partial void OnCollisionInternal(DynamicCollisionInfo info, Int32 typesHash);
    partial void OnTriggerInternal(DynamicCollisionInfo info, Int32 typesHash);

    public void OnTrigger(DynamicScene.Manifold manifold)
    {
      var aIsNull = manifold.A.Entity == null;
      if (aIsNull || manifold.B.Entity == null)
      {
        StaticCollisionInfo info;
        info = default(StaticCollisionInfo);
        info.Entity = (Entity*)(aIsNull ? manifold.B.Entity : manifold.A.Entity);
        info.StaticData = aIsNull ? manifold.A.StaticData : manifold.B.StaticData;
        _f.Signals.OnTriggerStatic(info);
        var typesHash = (Int32)info.Entity->Type;
        OnTriggerInternal(info, typesHash);
      } else
      {
        DynamicCollisionInfo info = default(DynamicCollisionInfo);
        info.EntityA = (Entity*)manifold.A.Entity;
        info.EntityB = (Entity*)manifold.B.Entity;
        _f.Signals.OnTriggerDynamic(info);
        var typesHash = (Int32)info.EntityA->Type | ((Int32)info.EntityB->Type << 16);
        OnTriggerInternal(info, typesHash);
      }
    }
  }
}
