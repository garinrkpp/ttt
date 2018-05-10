using System.Diagnostics;
using UnityEngine;
using UI = UnityEngine.UI;

public class QuantumStats : MonoBehaviour {
  public UI.Text Frame;
  public UI.Text Predicted;
  public UI.Text Resimulated;
  public UI.Text SimulateTime;
  public UI.Text SimulationState;
  public UI.Text NetworkPing;
  public UI.Text NetworkIn;
  public UI.Text NetworkOut;
  public UI.Text InputOffset;

  Stopwatch _networkTimer;

  void Awake() {
    DontDestroyOnLoad(gameObject);
  }

  void Update() {
    if (QuantumRunner.Current) {
      if (QuantumRunner.Current.Session.FramePredicted != null) {
        Frame.text = QuantumRunner.Current.Session.FramePredicted.Number.ToString();
      }

      Predicted.text = QuantumRunner.Current.Session.PredictedFrames.ToString();
      NetworkPing.text = QuantumRunner.Current.Session.Stats.Ping.ToString();
      SimulateTime.text = QuantumRunner.Current.Session.Stats.UpdateTime.ToString();
      InputOffset.text = QuantumRunner.Current.Session.Stats.Offset.ToString();
      Resimulated.text = QuantumRunner.Current.Session.Stats.ResimulatedFrames.ToString();

      if (QuantumRunner.Current.Session.IsStalling) {
        SimulationState.text = "Stalling";
        SimulationState.color = Color.red;
      }
      else {
        SimulationState.text = "Running";
        SimulationState.color = Color.green;
      }

      if (PhotonNetwork.connected) {
        PhotonNetwork.networkingPeer.TrafficStatsEnabled = true;

        if (_networkTimer == null) {
          _networkTimer = Stopwatch.StartNew();
        }

        NetworkIn.text = (PhotonNetwork.networkingPeer.TrafficStatsIncoming.TotalPacketBytes / _networkTimer.Elapsed.TotalSeconds).ToString() + " bytes/second";
        NetworkOut.text = (PhotonNetwork.networkingPeer.TrafficStatsOutgoing.TotalPacketBytes / _networkTimer.Elapsed.TotalSeconds).ToString() + " bytes/second";
      }
    }
    else {
      _networkTimer = null;
    }
  }

  public void ResetNetworkStats() {
    _networkTimer = null;

    if (PhotonNetwork.connected) {
      PhotonNetwork.networkingPeer.TrafficStatsReset();
    }
  }

  public static void Show() {
    if (FindObjectOfType<QuantumStats>()) {
      return;
    }

    Instantiate(Resources.Load<QuantumStats>(typeof(QuantumStats).Name));
  }
}
