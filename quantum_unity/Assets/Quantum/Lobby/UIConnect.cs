using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UIConnect : UIScreen<UIConnect> {
    public UI.InputField Username;
    public String GameVersion = "QuantumDemo_v1.0";

    public void OnConnectClicked() {
      if (String.IsNullOrEmpty(Username.text.Trim())) {
        UIDialog.Show("You need to enter a username");
        return;
      }

      PhotonNetwork.player.NickName = Username.text;
      PhotonNetwork.autoJoinLobby = true;
      PhotonNetwork.ConnectUsingSettings(GameVersion);

      UIConnect.HideScreen();
      UIConnecting.ShowScreen();
    }

    void Start() {
      // init runtime
      QuantumRunner.Init();

      // init database
      UnityDB.Init();
    }
  }
}