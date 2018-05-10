using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(DeterministicSessionConfigAsset))]
  public class DeterministicSessionConfigAssetEditor : UnityEditor.Editor {
    const String PREFS_KEY = "$SHOW_QUANTUM_CONFIG_HELP$";

    public override void OnInspectorGUI() {

      var asset = target as DeterministicSessionConfigAsset;
      if (asset) {
        OnInspectorGUI(asset);
      }

    }

    void OnInspectorGUI(DeterministicSessionConfigAsset asset) {
      EditorGUI.BeginChangeCheck();

      EditorPrefs.SetBool(PREFS_KEY, EditorGUILayout.Toggle("Show Help Info", EditorPrefs.GetBool(PREFS_KEY, true)));
      HelpBox("Should we show these help boxes?");

      GUILayout.Label("Simulation", EditorStyles.boldLabel);

      asset.Config.UpdateFPS = Math.Max(1, EditorGUILayout.IntField("Simulation Rate", asset.Config.UpdateFPS));
      HelpBox("How many ticks per second Quantum should execute.");

      asset.Config.LockstepSimulation = EditorGUILayout.Toggle("Lockstep", asset.Config.LockstepSimulation);
      HelpBox("Runs the quantum simulation in lockstep mode, where no rollbacks are performed. It's recommended to set input 'Static Delay' to at least 10 and 'Send Rate' to 1.");

      asset.Config.RunInBackgroundThread = EditorGUILayout.Toggle("Run In Background Thread", asset.Config.RunInBackgroundThread);
      WarnBox("!EXPERIMENTAL! Runs the quantum simulation in a background thread, NOT usable in lockstep mode.");

      EditorGUI.BeginDisabledGroup(asset.Config.RunInBackgroundThread == false);
      asset.Config.BackgroundThreadPriority = (System.Threading.ThreadPriority)EditorGUILayout.EnumPopup("Background Thread Priority", asset.Config.BackgroundThreadPriority);
      HelpBox("Which thread priority the background thread should run at.");
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup(asset.Config.LockstepSimulation);

      if (asset.Config.LockstepSimulation) {
        asset.Config.RollbackWindow = asset.Config.UpdateFPS;
      }

      asset.Config.RollbackWindow = Math.Max(asset.Config.UpdateFPS, EditorGUILayout.IntField("Rollback Window", asset.Config.RollbackWindow));
      HelpBox("How many frames are kept in the local ring buffer on each client. Controls how much Quantum can predict into the future. Not used in lockstep mode.");


      EditorGUI.BeginDisabledGroup(asset.Config.ExposeVerifiedStatusInsideSimulation);
      asset.Config.SkipRollbackWhenPossible = EditorGUILayout.Toggle("Skip Rollbacks When Possible", asset.Config.SkipRollbackWhenPossible);
      HelpBox("If Quantum should skip performing rollbacks and re-predict when it's not needed to retain determinism. Not used in lockstep mode. Mutually exclusive with the 'Expose Verified Status In Simulation' setting.");
      EditorGUI.EndDisabledGroup();

      if (asset.Config.SkipRollbackWhenPossible) {
        asset.Config.ExposeVerifiedStatusInsideSimulation = false;
      }

      asset.Config.SkipFrameBufferIntegrityChecks = EditorGUILayout.Toggle("Skip Frame Buffer Integrity Checks", asset.Config.SkipFrameBufferIntegrityChecks);
      HelpBox("If Quantum should skip performing integrity checks on its internal frame buffer (useful during development, enable skipping for release). Not used in lockstep mode.");

      EditorGUI.BeginDisabledGroup(asset.Config.SkipRollbackWhenPossible);
      asset.Config.ExposeVerifiedStatusInsideSimulation = EditorGUILayout.Toggle("Expose Verified Status In Simulation", asset.Config.ExposeVerifiedStatusInsideSimulation);
      WarnBox("!EXPERIMENTAL! Exposes the real verified status of a frame inside of the simulation. Mutually exclusive with the 'Skip Rollbacks When Possible' setting.");
      EditorGUI.EndDisabledGroup();

      if (asset.Config.ExposeVerifiedStatusInsideSimulation) {
        asset.Config.SkipRollbackWhenPossible = false;
      }

      EditorGUI.EndDisabledGroup();

      asset.Config.ChecksumInterval = Math.Max(0, EditorGUILayout.IntField("Checksum Interval", asset.Config.ChecksumInterval));
      HelpBox("How often we should send checksums of the frame state to the server for verification (useful during development, set to zero for release). Defined in frames.");

      GUILayout.Label("Input", EditorStyles.boldLabel);

      asset.Config.AggressiveSendMode = EditorGUILayout.Toggle("Aggressive Send", asset.Config.AggressiveSendMode);
      HelpBox("If the server should skip buffering and perform aggressive input sends, only suitable for games with <= 4 players.");

      asset.Config.InputDelay = Math.Max(0, EditorGUILayout.IntField("Static Delay", asset.Config.InputDelay));
      HelpBox("How much input delay that always is applied to local player input. Defined in frames.");

      asset.Config.InputPacking = Math.Max(1, EditorGUILayout.IntField("Send Rate", asset.Config.InputPacking));
      HelpBox("How often Quantum sends input to the server. 1 = Every frame, 2 = Every other frame, etc.");

      asset.Config.InputRedundancyStagger = Math.Max(0, EditorGUILayout.IntField("Send Staggering", asset.Config.InputRedundancyStagger));
      HelpBox("How much staggering the Quantum client should apply to redundant input resends. 1 = Wait one frame, 2 = Wait two frames, etc.");

      asset.Config.InputRepeatMaxDistance = Math.Max(0, EditorGUILayout.IntField("Repeat Max Distance", asset.Config.InputRepeatMaxDistance));
      HelpBox("How many frames Quantum will scan for repeatable inputs. 5 = Scan five frames forward and backwards, 10 = Scan ten frames, etc.");

      EditorGUI.BeginDisabledGroup(asset.Config.AggressiveSendMode);
      asset.Config.InputSoftTolerance = Math.Max(0, EditorGUILayout.IntField("Soft Tolerance", asset.Config.InputSoftTolerance));
      HelpBox("How many frames the server will wait until it will pre-emptively send any received inputs for a frame. Not used when 'Aggresive Send' is enabled.");
      EditorGUI.EndDisabledGroup();

      asset.Config.InputHardTolerance = Math.Max(-10, EditorGUILayout.IntField("Hard Tolerance", asset.Config.InputHardTolerance));
      HelpBox("How many frames the server will wait until it expires a frame and replaces all non-received inputs with repeated inputs or null's and sends it out to all players.");

      asset.Config.MinOffsetCorrectionDiff = Math.Max(1, EditorGUILayout.IntField("Offset Correction Limit", asset.Config.MinOffsetCorrectionDiff));
      HelpBox("How many frames the current local input delay must diff to the current requested offset for Quantum to update the local input offset. Defined in frames.");

      asset.Config.RttBias = Mathf.Clamp(EditorGUILayout.FloatField("Offset RTT Bias", asset.Config.RttBias), 0.5f, 1.0f);
      HelpBox("The multiplier used by the Quantum simulator when calculating your a players dynamic input offset. Defined as a multiplier.");

      GUILayout.Label("Time", EditorStyles.boldLabel);

      asset.Config.TimeCorrectionRate = Math.Max(0, EditorGUILayout.IntField("Correction Send Rate", asset.Config.TimeCorrectionRate));
      HelpBox("How many times per second the server will send out time correction packages to make sure every clients time is synchronized.");

      asset.Config.MinTimeCorrectionFrames = Math.Max(0, EditorGUILayout.IntField("Correction Frames Limit", asset.Config.MinTimeCorrectionFrames));
      HelpBox("How much the local client time must differ with the server time when a time correction package is received for the client to adjust it's local clock. Defined in frames.");

      asset.Config.TimeScaleMin = Mathf.Clamp(EditorGUILayout.IntField("Time Scale Minimum (%)", asset.Config.TimeScaleMin), 10, 100);
      HelpBox("The smallest timescale that can be applied by the server. Defined in percent.");

      asset.Config.TimeScalePingMin = Mathf.Clamp(EditorGUILayout.IntField("Time Scale Ping Start (ms)", asset.Config.TimeScalePingMin), 0, 1000);
      HelpBox("The ping value that the server will start lowering the time scale towards 'Time Scale Minimum'. Defined in milliseconds.");

      asset.Config.TimeScalePingMax = Mathf.Clamp(EditorGUILayout.IntField("Time Scale Ping End (ms)", asset.Config.TimeScalePingMax), asset.Config.TimeScalePingMin + 1, 1000);
      HelpBox("The ping value that the server will reach the 'Time Scale Minimum' value at, i.e. be at its slowest setting. Defined in milliseconds.");

      if (EditorGUI.EndChangeCheck()) {
        EditorUtility.SetDirty(asset);
      }
    }

    void WarnBox(String format, params System.Object[] args) {
      if (EditorPrefs.GetBool(PREFS_KEY, true)) {
        EditorGUILayout.HelpBox(String.Format(format, args), MessageType.Warning);
        EditorGUILayout.Space();
      }
    }

    void HelpBox(String format, params System.Object[] args) {
      if (EditorPrefs.GetBool(PREFS_KEY, true)) {
        EditorGUILayout.HelpBox(String.Format(format, args), MessageType.Info);
        EditorGUILayout.Space();
      }
    }
  }

}