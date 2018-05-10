using Quantum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UE = UnityEngine;

public unsafe class QuantumAnimator : MonoBehaviour {
  UE.Animator _animator;
  Dictionary<String, AnimationClip> _clips = new Dictionary<String, AnimationClip>();

  PlayableGraph _graph;
  AnimationMixerPlayable _mixerPlayable;
  AnimationPlayableOutput _output;

  Boolean _loaded = false;

  // used during SetAnimationData
  List<Int32> _indexes = new List<Int32>(64);
  List<AnimationClipPlayable> _playables = new List<AnimationClipPlayable>(64);
  List<AnimatorRuntimeBlendData> _blendData = new List<AnimatorRuntimeBlendData>(64);
  List<AnimatorMotion> _motionData = new List<AnimatorMotion>(32);

  void Awake() {
    _animator = GetComponentInChildren<UE.Animator>();
  }

  void OnEnable() {
    _animator = GetComponentInChildren<UE.Animator>();

    if (_animator) {
      _graph = PlayableGraph.Create();
      _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
      _output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
    }
  }

  void OnDisable() {
    _graph.Destroy();
  }

  public void Animate(Quantum.Animator* a) {
    var asset = UnityDB.FindAsset<AnimatorGraphAsset>(a->id);
    if (asset) {
      // load clips
      LoadClips(asset.clips);

      // clear old blend data
      _blendData.Clear();

      // calculate blend data
      asset.Settings.GenerateBlendList(a, _blendData);

      // update animation state
      SetAnimationData(asset.Settings, _blendData);

      //      Debug.Log(Quantum.Animator.GetFixedPoint(a, "Forward") +" "+ Quantum.Animator.GetFixedPoint(a, "Turn"));
      //      Debug.Log(Quantum.Animator.GetBoolean(a, "Dead") +" "+ Quantum.Animator.GetFixedPoint(a, "Speed"));
      //      Debug.Log(a->current_state_id+" "+a->from_state_id+" "+a->to_state_id+" "+a->transition_time+" "+a->transition_index);
    }
  }

  void LoadClips(List<AnimationClip> clipList) {
    if (_loaded) {
      return;
    }

    _loaded = true;

    for (int c = 0; c < clipList.Count; c++) {
      if (_clips.ContainsKey(clipList[c].name) == false) {
        _clips.Add(clipList[c].name, clipList[c]);
      }
    }
  }


  void SetAnimationData(AnimatorGraph graph, List<AnimatorRuntimeBlendData> blend_data) {
    if (!_animator) {
      Log.Error("No Animator component attached to '{0}' or one of it's children, cant play animations", gameObject.name);
      return;
    }

    var blendCount = blend_data.Count;
    if (blendCount == 0) {
      //      Log.Warn("No blends sent");
      return;
    }

    _indexes.Clear();
    _playables.Clear();

    for (Int32 b = 0; b < blendCount; b++) {
      _motionData.Clear();

      var var = blend_data[b];
      var state = graph.GetState(var.stateId);
      var motion = state.GetMotion(var.animationIndex, _motionData) as AnimatorClip;

      if (motion != null && !String.IsNullOrEmpty(motion.clipName)) {
        _playables.Add(AnimationClipPlayable.Create(_graph, _clips[motion.clipName]));
        _indexes.Add(b);
      }
    }

    var playableCount = _playables.Count;
    //    Debug.Log("====: "+playableCount);
    if (playableCount > 1) {
      _mixerPlayable = AnimationMixerPlayable.Create(_graph, playableCount);

      for (Int32 p = 0; p < playableCount; p++) {
        _graph.Connect(_playables[p], 0, _mixerPlayable, p);
      }
      _output.SetSourcePlayable(_mixerPlayable);

      for (Int32 p = 0; p < playableCount; p++) {
        var data = blend_data[_indexes[p]];
        float normalTime = data.normalTime.AsFloat;
        float clipLength = _playables[p].GetAnimationClip().length;
        //        float clipLength = (float)_playables[p].GetDuration();

        _playables[p].SetTime(normalTime * clipLength);//currentTime.AsFloat);

        //        _playables[p].SetTime(data.normalTime.AsFloat);
        //        Debug.Log(p + " \t " + _playables[p].GetAnimationClip().name.Substring(0,10) + " \t " + data.currentTime.AsFloat.ToString("F4") + " \t " + data.normalTime.AsFloat.ToString("F4") + " \t " + _playables[p].GetAnimationClip().length.ToString("F4") + " \t " + data.weight.AsFloat.ToString("F4") + " \t " + data.length.AsFloat.ToString("F4") + " \t " + data.calculatedLength.AsFloat.ToString("F4"));

        _playables[p].SetPlayState(PlayState.Paused);

        _mixerPlayable.SetInputWeight(p, data.weight.AsFloat);
      }
    }
    else if (playableCount == 1) {
      var data = blend_data[_indexes[0]];
      float normalTime = data.normalTime.AsFloat;
      float clipLength = _playables[0].GetAnimationClip().length;

      _playables[0].SetTime(normalTime * clipLength);
      //      Debug.Log("X \t " + _playables[0].GetAnimationClip().name.Substring(0, 10) + " \t " + data.currentTime.AsFloat.ToString("F4") + " \t " + data.normalTime.AsFloat.ToString("F4") + " \t " + _playables[0].GetAnimationClip().length.ToString("F4") + " \t " + data.weight.AsFloat.ToString("F4") + " \t " + data.length.AsFloat.ToString("F4") + " \t " + data.calculatedLength.AsFloat.ToString("F4"));

      _output.SetSourcePlayable(_playables[0]);
    }

    if (playableCount > 0) {
      _graph.Play();
    }
    else {
      _graph.Stop();
    }
  }
}
