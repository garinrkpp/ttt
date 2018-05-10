using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefabRoot : MonoBehaviour {
  [NonSerialized]
  public String AssetGuid;

  [NonSerialized]
  public Quantum.EntityRef EntityRef;

  [Range(0, 1)]
  public Single InterpolatePositionSpeed = 0.1f;

  [Range(0, 1)]
  public Single InterpolateRotationSpeed = 0.1f;

  public QuantumAnimator QuantumAnimator;
}
