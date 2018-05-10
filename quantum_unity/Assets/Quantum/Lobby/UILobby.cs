using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UILobby : UIScreen<UILobby> {
    public UILobbyRoomTemplate RoomTemplate;
    public UI.GridLayoutGroup RoomGrid;

    // list of active rooms we can join
    List<UILobbyRoomTemplate> _activeRooms = new List<UILobbyRoomTemplate>();

    void Start() {
      RoomTemplate.Hide();
    }

    void Update() {
      if (IsScreenVisible()) {
        if (PhotonNetwork.connected) {
          if (PhotonNetwork.inRoom) {
            UILobby.HideScreen();
            UIRoom.ShowScreen();
          }
          else if (PhotonNetwork.insideLobby) {
            UpdateRoomList(PhotonNetwork.GetRoomList());
          }
        }
        else {
          UIDialog.Show("Disconnected From Photon");

          UILobby.HideScreen();
          UIConnect.ShowScreen();
        }
      }
    }

    void UpdateRoomList(RoomInfo[] rooms) {
      // remove old rooms and update existing rooms
      for (Int32 i = _activeRooms.Count - 1; i >= 0; --i) {
        var room = rooms.FirstOrDefault(x => x.Name == _activeRooms[i].RoomName);
        if (room == null || room.IsOpen == false) {
          // destroy room
          Destroy(_activeRooms[i].gameObject);

          // remove room
          _activeRooms.RemoveAt(i);
        }
        else {
          _activeRooms[i].Refresh(room);
        }
      }

      // create new rooms
      for (Int32 i = 0; i < rooms.Length; ++i) {
        var room = rooms[i];
        if (room.IsOpen) {
          var activeRoom = _activeRooms.FirstOrDefault(x => x.RoomName == room.Name);
          if (activeRoom == null) {
            UILobbyRoomTemplate instance;

            instance = Instantiate(RoomTemplate);
            instance.Show();
            instance.transform.SetParent(RoomGrid.transform, false);
            instance.transform.SetAsLastSibling();
            instance.Join.onClick.AddListener(() => JoinRoom(room));
            instance.Refresh(room);

            _activeRooms.Add(instance);
          }
        }
      }
    }

    void JoinRoom(RoomInfo room) {
      PhotonNetwork.JoinRoom(room.Name);
    }

    void ClearActiveRooms() {
      for (Int32 i = 0; i < _activeRooms.Count; ++i) {
        if (_activeRooms[i]) {
          Destroy(_activeRooms[i].gameObject);
        }
      }

      _activeRooms.Clear();
    }

    public void OnCreateNewRoomClicked() {
      UILobby.HideScreen();
      UICreateRoom.ShowScreen();
    }

    public override void OnHideScreen(bool first) {
      ClearActiveRooms();
    }

    public override void OnShowScreen(bool first) {
      ClearActiveRooms();
    }
  }
}