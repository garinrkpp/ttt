using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Example {
  public class UIMain : MonoBehaviour {
    void Awake() {
      DontDestroyOnLoad(gameObject);
    }
  }
}