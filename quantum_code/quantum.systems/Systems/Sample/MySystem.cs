using System;
using Photon.Deterministic;

namespace Quantum
{
    //has to inherit from base system class
    //marked as unsafe because we're going to use pointers
    public unsafe class MySystem : SystemBase, ISignalOnDamage, ISignalOnDamageBefore
    {

    public override void OnInit(Frame f)
		{
      base.OnInit(f);

      //all generated API concerning the game state
      //exists within the "Frame" object type.

      for (int i = 0; i < f.RuntimeConfig.Players.Length; i++)
      {
	
      	var aChar = f.CreateCharacter();

        aChar->Player = i;
        aChar->CharacterSpec = f.RuntimeConfig.Players[i].CharacterSpec;
        aChar->DynamicBody.InitDynamic(aChar->CharacterSpec.ShapeConfig  , 1);
        aChar->Prefab.Current = "Player";

      }
		}

		public override void Update(Frame f)
    {
        var allChars = f.GetAllCharacters();

        while(allChars.Next())
        {
            var curr = allChars.Current;
            var i = f.GetPlayerInput(curr->Player);
            curr->DynamicBody.Velocity = i->Movement * curr->CharacterSpec.Speed;

            if(i->Fire.WasPressed)
            {
                var b = f.CreateProjectile();
                b->DynamicBody.InitDynamic(Core.DynamicShape.CreateCircle(FP._0_25), 1);
                b->DynamicBody.IsTrigger = true;
                b->Source = curr->EntityRef;

                //initialize directions and velocity
                b->Transform2D.Position = curr->Transform2D.Position;
                var direction = FPVector2.Rotate(FPVector2.Right, curr->Transform2D.Rotation);
                b->DynamicBody.Velocity = direction * 8;
                b->Time = 2; 


            }
        }
    }

    void ISignalOnDamage.OnDamage(Frame f, DamageData dmg)
    {
      f.Events.CharacterDamage(dmg.Character, dmg.Damage);
    }

    void ISignalOnDamageBefore.OnDamageBefore(Frame f, DamageData* dmg)
    {
      
      Character* theChar = f.GetCharacter(dmg->Character);
      theChar->CharacterSpec.ProcessDamage(dmg);
    }
  }
}
