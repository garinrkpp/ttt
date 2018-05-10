using System;
using Photon.Deterministic;

namespace Quantum
{

  //[AssetObjectConfig()]
  abstract unsafe partial class CharacterSpec
  {
    public FP Speed;
    public DynamicShapeConfig ShapeConfig;

    public virtual void ProcessDamage(DamageData* dmg)
    {
      
    }
  }

  [Serializable]
  public class MageSpec : CharacterSpec
  {
    public FP Mana;
  }

  [Serializable]
  public class WarriorSpec : CharacterSpec
  {
    public FP Armor;

    public override unsafe void ProcessDamage(DamageData* dmg)
    {
      dmg->Damage -= Armor;
    }

  }

}
