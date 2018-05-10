using System.Collections;
using UnityEngine;

namespace Quantum.Example {
  public class UILeaveGame : UIScreen<UILeaveGame> {
    public GameObject UICamera;
    public GameObject Background;

    Coroutine _leaveRoutine;

    public override void OnShowScreen(bool first) {
      UICamera.Hide();
      Background.Hide();
    }

    public override void OnHideScreen(bool first) {
      UICamera.Show();
      Background.Show();

      _leaveRoutine = null;
    }

    public void OnLeaveClicked() {
      if (_leaveRoutine != null) {
        return;
      }

      _leaveRoutine = StartCoroutine(OnLeaveRoutine());
    }

    IEnumerator OnLeaveRoutine() {
      if (QuantumRunner.Current) {

        // unload current map
        if (QuantumGame.Running) {
          var mapData = FindObjectOfType<MapData>();
          if (mapData) {
            Destroy(mapData.gameObject);
          }
        }

        // leave room
        PhotonNetwork.LeaveRoom();

        // shutdown runner
        QuantumRunner.Current.Shutdown();

        // wait one second
        yield return new WaitForSeconds(1f);

        // hide leave game button
        UILeaveGame.HideScreen();

        // are we still connected?
        if (PhotonNetwork.connected) {

          // goto lobby
          UILobby.ShowScreen();

        } else {

          // goto connect screen
          UIConnect.ShowScreen();

        }
      }

      _leaveRoutine = null;
    }
  }
}