using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UI = UnityEngine.UI;

namespace Quantum.Example {
  public class UICreateRoom : UIScreen<UICreateRoom>, IPunCallbacks {
    public UI.InputField RoomName;
    public UI.InputField PlayerCount;

    public override void OnShowScreen(Boolean first) {
      if (String.IsNullOrEmpty(PlayerCount.text.Trim())) {
        PlayerCount.text = "4";
      }
    }

    public void OnBackClicked() {
      UICreateRoom.HideScreen();
      UILobby.ShowScreen();
    }

    public void OnCreateClicked() {
      if (String.IsNullOrEmpty(RoomName.text.Trim())) {
        UIDialog.Show("You must enter a room name");
        return;
      }

      if (String.IsNullOrEmpty(PlayerCount.text.Trim())) {
        UIDialog.Show("You must enter a max player count");
        return;
      }

      RoomOptions roomOptions = new RoomOptions();
      roomOptions.IsVisible = true;
      roomOptions.MaxPlayers = Byte.Parse(PlayerCount.text);

      PhotonNetwork.CreateRoom(RoomName.text, roomOptions, TypedLobby.Default);
    }

    public void OnConnectedToPhoton() {
    }

    public void OnLeftRoom() {
    }

    public void OnMasterClientSwitched(PhotonPlayer newMasterClient) {
    }

    public void OnPhotonCreateRoomFailed(object[] codeAndMsg) {
      UIDialog.Show("Room Creation Failed");
    }

    public void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
      UIDialog.Show("Room Join Failed");
    }

    public void OnCreatedRoom() {
      if (IsScreenVisible()) {
        UICreateRoom.HideScreen();
        UIRoom.ShowScreen();
      }
    }

    public void OnJoinedLobby() {
    }

    public void OnLeftLobby() {
    }

    public void OnFailedToConnectToPhoton(DisconnectCause cause) {
    }

    public void OnConnectionFail(DisconnectCause cause) {
    }

    public void OnDisconnectedFromPhoton() {
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
    }

    public void OnReceivedRoomListUpdate() {
    }

    public void OnJoinedRoom() {
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer) {
    }

    public void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
    }

    public void OnConnectedToMaster() {
    }

    public void OnPhotonMaxCccuReached() {
    }

    public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
    }

    public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
    }

    public void OnUpdatedFriendList() {
    }

    public void OnCustomAuthenticationFailed(string debugMessage) {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    }

    public void OnWebRpcResponse(OperationResponse response) {
    }

    public void OnOwnershipRequest(object[] viewAndPlayer) {
    }

    public void OnLobbyStatisticsUpdate() {
    }

    public void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer) {
    }

    public void OnOwnershipTransfered(object[] viewAndPlayers) {
    }
  }
}