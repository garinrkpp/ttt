using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Example {
  public class UIConnecting : UIScreen<UIConnecting> {
    public override void OnShowScreen(bool first) {
      StartCoroutine(WaitForConnected());
    }

    IEnumerator WaitForConnected() {
      while (PhotonNetwork.connected == false) {
        yield return null;
      }

      if (IsScreenInstanceVisible()) {
        UIConnecting.HideScreen();
        UILobby.ShowScreen();
      }
    }
  }
}