using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UILobbyRoomTemplate : MonoBehaviour {
    public String RoomName {
      get;
      set;
    }

    public UI.Text Info;
    public UI.Button Join;

    public void Refresh(RoomInfo room) {
      // assign room name
      RoomName = room.Name;

      // update info
      Info.text = String.Format("{0} ({1}/{2})", room.Name, room.PlayerCount, room.MaxPlayers);
    }
  }
}