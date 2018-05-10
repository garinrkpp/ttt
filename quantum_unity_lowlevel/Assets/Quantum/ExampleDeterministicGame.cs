using Photon.Deterministic;
using System;

public class ExampleDeterministicGame : IDeterministicGame {
  DeterministicSession _session;
  DeterministicRuntimeConfig _runtimeConfig;

  public ExampleDeterministicGame(DeterministicRuntimeConfig runtimeConfig) {
    _runtimeConfig = runtimeConfig;
  }

  public void AssignSession(DeterministicSession session) {
    // This it the same object as created in ExampleQuantumRunner
    _session = session;
  }

  public DeterministicFrame CreateFrame() {
    // Create your frame here
    return null;
  }

  public void OnChecksumError(DeterministicTickChecksumError error, DeterministicFrame[] frames) {
    // Called in case of a checksum error
  }

  public void OnDestroy() {
    // Called when DeterministicSession.Destroy is called
  }

  public void OnGameEnded() {
    // Not currently invoked
  }

  public void OnGameStart(DeterministicFrame state) {
    // Called when game starts, with the first frame 
  }

  public Tuple<Byte[], DeterministicInputFlags> OnLocalInput(int player) {
    // POLL INPUT HERE
    return default(Tuple<Byte[], DeterministicInputFlags>);
  }

  public void OnSimulate(DeterministicFrame state) {
    // Perform simulation here
  }

  public void OnSimulateFinished(DeterministicFrame state) {
    // After simulation has been finished for a frame this is invoked to process events, etc.
    // Can ignore this for lockstep mode
  }

  public void OnUpdateDone() {
    // Called during unitys Update() 
    // Called after all OnSimulate and OnSimulateFinished calls have been invoked (could potentially be zero calls to OnSimulate/OnSimulateFinished)
  }
}
