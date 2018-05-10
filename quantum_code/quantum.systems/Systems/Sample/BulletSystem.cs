using System;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class BulletSystem : SystemBase, ISignalOnTriggerDynamic
    {

    //int ISignalOnTriggerDynamic.RuntimeIndex => throw new NotImplementedException();

    public override void Update(Frame f)
        {
          var allBullets = f.GetAllProjectiles();
          while(allBullets.Next())
          {
              var bullet = allBullets.Current;

              if(bullet->Time <= 0)
              {
                  f.DestroyProjectile(bullet);
              }
              else
              {
                  bullet->Time -= f.DeltaTime;    
              }


          }
        }

        void ISignalOnTriggerDynamic.OnTriggerDynamic(Frame f, DynamicCollisionInfo info)
        {
          var charOne = Entity.CastToCharacter(info.EntityA);
          if (charOne == null)
              charOne = Entity.CastToCharacter(info.EntityB);


          var charTwo = Entity.CastToProjectile(info.EntityA);
          if (charTwo == null)
              charTwo = Entity.CastToProjectile(info.EntityB);

          if(charTwo != null && charOne != null && !charTwo->Source.Equals(charOne->EntityRef))
          {
            //if it's a valid hit and we didn't hit ourself
            //destroy the bullet
            f.DestroyProjectile(charTwo);
            DamageData dmg = new DamageData()
            {
              Character = charOne->EntityRef,
              Type = DamageType.Magical,
              Damage = 20
            };
            f.Signals.OnDamageBefore(&dmg);
            f.Signals.OnDamage(dmg);


            //f.Events.CharacterDamage(charOne->EntityRef, 10);
          }
        }
    }
}
