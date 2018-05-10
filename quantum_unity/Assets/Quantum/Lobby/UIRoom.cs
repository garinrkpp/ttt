using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UIRoom : UIScreen<UIRoom> {
    public UI.Text RoomName;
    public UI.Button StartButton;
    public UI.Text WaitingMessage;
    public UI.Dropdown MapSelect;

    public UI.GridLayoutGroup PlayerGrid;
    public UIRoomPlayer PlayerTemplate;
    public Boolean CloseRoomOnStart = true;

    Boolean _started;
    List<UIRoomPlayer> _players = new List<UIRoomPlayer>();

    void Start() {
      PlayerTemplate.Hide();
    }

    void Update() {
      if (IsScreenVisible()) {
        if (PhotonNetwork.inRoom) {
          // toggle start button state
          StartButton.Toggle(PhotonNetwork.isMasterClient);

          // toggle map select
          MapSelect.Toggle(PhotonNetwork.isMasterClient);

          // toggle
          WaitingMessage.Toggle(PhotonNetwork.isMasterClient == false);

          // update room name
          RoomName.text = String.Format("{0} ({1}/{2})", PhotonNetwork.room.Name, PhotonNetwork.room.PlayerCount, PhotonNetwork.room.MaxPlayers);

          // update players list
          UpdatePlayerList();

          //
          CheckForGameStart();
        }
        else {
          UIRoom.HideScreen();
          UILobby.ShowScreen();
        }
      }
    }

    void CheckForGameStart() {
      if (_started) {
        return;
      }

      var start = false;
      var map = default(String);

      if (TryGetRoomProperty<Boolean>("START", out start) && TryGetRoomProperty<String>("MAP", out map)) {
        if (start && String.IsNullOrEmpty(map) == false) {
          _started = true;

          RuntimeConfig config;
          config = new RuntimeConfig();
          config.Players = new RuntimePlayer[PhotonNetwork.room.MaxPlayers];

          for (Int32 i = 0; i < config.Players.Length; ++i) {
            config.Players[i] = new RuntimePlayer();
            config.Players[i].CharacterSpec.Guid = "mage";
          }

          config.Map.Guid = UnityDB.AllOf<MapAsset>().First(x => x.Settings.Scene == map).Settings.Guid;
          config.GameMode = Photon.Deterministic.DeterministicGameMode.Multiplayer;

          QuantumRunner.StartGame(config);

          UIRoom.HideScreen();
          UILeaveGame.ShowScreen();
        }
      }
    }

    void UpdatePlayerList() {
      while (_players.Count < PhotonNetwork.room.MaxPlayers) {
        UIRoomPlayer instance;
        instance = Instantiate(PlayerTemplate);
        instance.transform.SetParent(PlayerGrid.transform, false);
        instance.transform.SetAsLastSibling();

        _players.Add(instance);
      }

      var i = 0;

      for (; i < PhotonNetwork.playerList.Length; ++i) {
        _players[i].Name.text = GetPlayerName(PhotonNetwork.playerList[i]);
        _players[i].Show();
      }

      for (; i < _players.Count; ++i) {
        _players[i].Hide();
      }

      WaitingMessage.transform.SetAsLastSibling();
    }

    String GetPlayerName(PhotonPlayer player) {
      String name = player.NickName;

      if (player.IsLocal) {
        name += " (You)";
      }

      if (player.IsMasterClient) {
        name += " (Room Owner)";
      }

      return name;
    }

    Boolean TryGetRoomProperty<T>(String key, out T value) {
      System.Object v;

      if (PhotonNetwork.room.CustomProperties.TryGetValue(key, out v)) {
        if (v is T) {
          value = (T)v;
          return true;
        }
      }

      value = default(T);
      return false;
    }

    public void OnLeaveClicked() {
      PhotonNetwork.LeaveRoom();
    }

    public void OnStartClicked() {
      if (PhotonNetwork.isMasterClient && PhotonNetwork.room.IsOpen) {
        var ht = new ExitGames.Client.Photon.Hashtable();
        ht.Add("MAP", MapSelect.options[MapSelect.value].text);
        ht.Add("START", true);

        if (CloseRoomOnStart) {
          PhotonNetwork.room.IsOpen = false;
        }

        PhotonNetwork.room.SetCustomProperties(ht);
      }
    }

    public override void OnShowScreen(bool first) {
      _started = false;

      MapSelect.ClearOptions();
      MapSelect.AddOptions(UnityDB.AllOf<MapAsset>().Select(x => x.Settings.Scene).ToList());
      MapSelect.value = 0;
    }
  }
}