using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;

public unsafe class EventManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		EventCharacterDamage.OnRaised += OnDamage;
	}

	private void OnDestroy()
	{
		EventCharacterDamage.OnRaised -= OnDamage;
	}

	void OnDamage(EventCharacterDamage e){
		var c = QuantumGame.Frame.GetCharacter(e.Character);
		var spec = UnityDB.FindAsset<CharacterSpecAsset>(c->CharacterSpec) as MageSpecAsset;
		if (spec != null)
			Debug.Log("Damage from mage: " + e.Damage);
		else
			Debug.Log("Damage otherwise: " + e.Damage);
		Debug.Log("Hit for " + e.Damage + " to " + e.Character.ToString());
        
	}
}
