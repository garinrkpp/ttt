using ExitGames.Client.Photon;
using Photon.Deterministic;
using System;
using System.Collections.Generic;

namespace Quantum.Core {
  public class QuantumNetworkCommunicator : ICommunicator {
    RaiseEventOptions _eventOptions;
    LoadBalancingPeer _loadBalancingPeer;
    PhotonNetwork.EventCallback _lastEventCallback;

    Dictionary<Byte, Object> _parameters;

    Boolean _autoDisconenct;
    SendOptions _sendOperationOptions;

    public Boolean IsConnected {
      get {
        return PhotonNetwork.connected;
      }
    }

    public Int32 RoundTripTime {
      get {
        return _loadBalancingPeer.RoundTripTime;
      }
    }

    public Byte LocalPLayerId {
      get {
        return (Byte)PhotonNetwork.player.ID;
      }
    }

    internal QuantumNetworkCommunicator(LoadBalancingPeer loadBalancingPeer, Boolean autoDisconnect) {
      _autoDisconenct = autoDisconnect;

      _loadBalancingPeer = loadBalancingPeer;
      _loadBalancingPeer.TimePingInterval = 50;

      _parameters = new Dictionary<Byte, Object>();
      _parameters[ParameterCode.ReceiverGroup] = (byte)ReceiverGroup.All;

      _eventOptions = new RaiseEventOptions();

      _sendOperationOptions = new SendOptions { DeliveryMode = DeliveryMode.Unreliable };
    }

    public void OpRaiseEvent(Byte eventCode, Object message, Boolean reliable, Int32[] toPlayers) {
      if (_loadBalancingPeer.PeerState != PeerStateValue.Connected) {
        return;
      }

      _parameters[ParameterCode.Code] = eventCode;
      _parameters[ParameterCode.Data] = message;

      _eventOptions.TargetActors = toPlayers;

      if (eventCode != MessageTypes.SEND_CODE) {
        _loadBalancingPeer.OpRaiseEvent(eventCode, message, reliable, _eventOptions);
      }
      else {
        PhotonNetwork.networkingPeer.SendOperation(OperationCode.RaiseEvent, _parameters, _sendOperationOptions);
      }

      _loadBalancingPeer.SendOutgoingCommands();
    }

    public void AddEventListener(OnEventReceived onEventReceived) {
      RemoveEventListener();

      // save callback we know how to de-register it
      _lastEventCallback = (Byte eventCode, Object content, Int32 senderId) => onEventReceived(eventCode, content);

      // attach callback
      PhotonNetwork.OnEventCall += _lastEventCallback;
    }

    public void Service() {
      _loadBalancingPeer.Service();
    }

    public void RemoveEventListener() {
      if (_lastEventCallback != null) {
        PhotonNetwork.OnEventCall -= _lastEventCallback;
      }
    }

    public void OnDestroy() {
      RemoveEventListener();

      // leave room
      PhotonNetwork.LeaveRoom();
    }
  }
}
