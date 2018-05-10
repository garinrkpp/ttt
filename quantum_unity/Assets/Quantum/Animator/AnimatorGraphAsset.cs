using System.Collections.Generic;
using Quantum;
using UnityEngine;
using Photon.Deterministic;
using AnimatorState = Quantum.AnimatorState;

[CreateAssetMenu(menuName = "Quantum/Assets/Animator Graph")]
public class AnimatorGraphAsset : AssetBase, ISerializationCallbackReceiver {
  public AnimatorGraph Settings;

  public override AssetObject AssetObject {
    get {
      return Settings;
    }
  }

  public enum Resolutions {
    _8 = 8,
    _16 = 16,
    _32 = 32,
    _64 = 64
  }

  public Resolutions weight_table_resolution = Resolutions._32;

  public RuntimeAnimatorController controller;

  public List<AnimationClip> clips = new List<AnimationClip>();


  public void OnAfterDeserialize() {
    Deserialize();
  }

  public void OnBeforeSerialize() {
    Serialize();
  }

  private void Serialize() {
    if (Settings == null) Settings = new AnimatorGraph();
    if (Settings.layers == null) {
      return;
    }

    int layerCount = Settings.layers.Length;
    for (int l = 0; l < layerCount; l++) {
      AnimatorLayer layer = Settings.layers[l];
      if (layer.states == null) layer.states = new AnimatorState[0];
      int stateCount = layer.states.Length;
      for (int s = 0; s < stateCount; s++) {
        AnimatorState state = layer.states[s];
        if (state.serialisedMotions == null)
          state.serialisedMotions = new List<SerializableMotion>();
        state.serialisedMotions.Clear();
        SerialiseObject(state);
      }
    }
  }

  public void Deserialize() {
    AnimatorGraph animator = Settings;
    int layerCount = animator.layers.Length;
    for (int l = 0; l < layerCount; l++) {
      AnimatorLayer layer = animator.layers[l];
      int stateCount = layer.states.Length;
      for (int s = 0; s < stateCount; s++) {
        AnimatorState state = layer.states[s];
        if (state.serialisedMotions.Count > 0) {
          state.motion = ReadNodeFromSerializedNodes(state, 0);
        }
      }
    }
  }

  public void SerialiseObject(AnimatorState state, AnimatorMotion mo = null) {
    SerializableMotion serialisedBo = new SerializableMotion();

    if (mo == null)//initial blend object will be patched in on first call
      mo = state.motion;

    if (mo is AnimatorClip) {
      AnimatorClip anim = mo as AnimatorClip;
      serialisedBo.isAnimation = true;
      serialisedBo.name = anim.data.clipName;
      serialisedBo.animatorData = anim.data;
      serialisedBo.childCount = 0;
      serialisedBo.indexOfFirstChild = state.serialisedMotions.Count + 1;

      state.serialisedMotions.Add(serialisedBo);
    }

    if (mo is AnimatorBlendTree) {
      AnimatorBlendTree blend = mo as AnimatorBlendTree;
      serialisedBo.isAnimation = false;
      serialisedBo.name = blend.name;//string.Format("Tree of {0}", blend.motionCount);
      serialisedBo.positions = blend.positions;
      serialisedBo.blendParameterIndex = blend.blendParameterIndex;
      serialisedBo.blendParameterIndexY = blend.blendParameterIndexY;
      serialisedBo.weightTable = SerializeWeightTable(blend.weightTable);
      serialisedBo.resolution = blend.resolution;

      serialisedBo.childCount = blend.motionCount;
      serialisedBo.indexOfFirstChild = state.serialisedMotions.Count + 1;

      state.serialisedMotions.Add(serialisedBo);
      foreach (var child in blend.motions)
        SerialiseObject(state, child);
    }
  }

  private AnimatorMotion ReadNodeFromSerializedNodes(AnimatorState state, int index) {
    SerializableMotion serialisedBo = state.serialisedMotions[index];
    List<AnimatorMotion> children = new List<AnimatorMotion>();
    for (int i = 0; i < serialisedBo.childCount; i++)
      children.Add(ReadNodeFromSerializedNodes(state, serialisedBo.indexOfFirstChild + i));

    if (serialisedBo.isAnimation) {
      AnimatorClip anim = new AnimatorClip();
      anim.name = serialisedBo.name;
      anim.data = serialisedBo.animatorData;
      anim.treeIndex = index;
      return anim;
    }
    else {
      AnimatorBlendTree blend = new AnimatorBlendTree();
      blend.name = serialisedBo.name;
      blend.positions = serialisedBo.positions;
      blend.blendParameterIndex = serialisedBo.blendParameterIndex;
      blend.blendParameterIndexY = serialisedBo.blendParameterIndexY;
      blend.resolution = serialisedBo.resolution;
      blend.weightTable = DeserializeWeightTable(serialisedBo.weightTable);
      blend.motions = children.ToArray();
      blend.motionCount = children.Count;
      blend.weights = new FP[blend.motionCount];
      blend.treeIndex = index;
      return blend;
    }
  }

  private FP[,][] DeserializeWeightTable(SerializableWeightDimentionX table) {
    int xLength = table.data.Length;
    if (xLength == 0) return new FP[0, 0][];
    int yLength = table.data[0].data.Length;
    FP[,][] output = new FP[xLength, yLength][];
    for (int x = 0; x < xLength; x++) {
      for (int y = 0; y < yLength; y++) {
        output[x, y] = table.data[x].data[y].data;
      }
    }
    return output;
  }

  private SerializableWeightDimentionX SerializeWeightTable(FP[,][] table) {
    int xLength = table.GetLength(0);
    int yLength = table.GetLength(1);
    if (xLength == 0) return new SerializableWeightDimentionX();
    SerializableWeightDimentionX output = new SerializableWeightDimentionX();
    output.data = new SerializableWeightDimentionY[xLength];
    for (int x = 0; x < xLength; x++) {
      output.data[x].data = new SerializableWeightDimentionZ[yLength];
      for (int y = 0; y < yLength; y++) {
        output.data[x].data[y].data = table[x, y];
      }
    }
    return output;
  }

}