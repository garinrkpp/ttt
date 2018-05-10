using System;
using System.Collections.Generic;
using Quantum;
using Photon.Deterministic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorState = UnityEditor.Animations.AnimatorState;
using System.IO;

public class ExportAnimations : MonoBehaviour
{

  /*
  [MenuItem("Assets/Export Mecanim Animation Controller", false, 301)]
  private static void CreateAnimatorFile() {
    var animatorController = Selection.activeObject as AnimatorController;
    if (animatorController == null) {
      return;
    }

    CreateAsset(animatorController);
  }

  [MenuItem("Assets/Export Mecanim Animation Controller", true)]
  private static bool CreateAnimatorController() {
    return Selection.activeObject is AnimatorController;
  }
  */

  //  public static AnimatorGraphAsset Fetch(string name) {
  //    AnimatorGraphAsset output = null;

  //#if UNITY_EDITOR
  //    string pathToAnimationResource = "Assets/Resources/DB/Animator/";
  //    string animationFilePath = pathToAnimationResource + name + ".asset";
  //    output = UnityEditor.AssetDatabase.LoadAssetAtPath(animationFilePath, typeof(AnimatorGraphAsset)) as AnimatorGraphAsset;

  //    if (output == null) {
  //      output = CreateInstance<AnimatorGraphAsset>();
  //      animationFilePath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(animationFilePath);
  //      UnityEditor.AssetDatabase.CreateAsset(output, animationFilePath);
  //      UnityEditor.AssetDatabase.SaveAssets();
  //      UnityEditor.AssetDatabase.Refresh();
  //    }
  //#endif

  //    if (output != null) {
  //      if (output.Settings == null)
  //        output.Settings = new AnimatorGraph();
  //    }

  //    return output;
  //  }

  public static void CreateAsset(AnimatorGraphAsset dataAsset, AnimatorController controller)
  {
    if (!controller)
    {
      return;
    }

    if (!dataAsset)
    {
      return;
    }

    QuantumRunner.Init();//make sure we can get debug calls from Quantum
    dataAsset.controller = controller;
    int weightTableResolution = (int)dataAsset.weight_table_resolution;
    int variableCount = controller.parameters.Length;
    dataAsset.Settings.variables = new AnimatorVariable[variableCount];

    //Mecanim Parameters/Variables
    //make a dictionary of paramets by name for use when extracting conditions for transitions
    Dictionary<string, AnimatorControllerParameter> parameterDic = new Dictionary<string, AnimatorControllerParameter>();
    for (int v = 0; v < variableCount; v++)
    {
      AnimatorControllerParameter parameter = controller.parameters[v];
      parameterDic.Add(parameter.name, parameter);
      AnimatorVariable newVariable = new AnimatorVariable();

      newVariable.name = parameter.name;
      newVariable.index = v;
      switch (parameter.type)
      {
        case AnimatorControllerParameterType.Bool:
          newVariable.type = AnimatorVariable.VariableType.Bool;
          newVariable.defaultBool = parameter.defaultBool;
          break;
        case AnimatorControllerParameterType.Float:
          newVariable.type = AnimatorVariable.VariableType.FP;
          newVariable.defaultFp = FP.FromFloat_UNSAFE(parameter.defaultFloat);
          break;
        case AnimatorControllerParameterType.Int:
          newVariable.type = AnimatorVariable.VariableType.Int;
          newVariable.defaultInt = parameter.defaultInt;
          break;
        case AnimatorControllerParameterType.Trigger:
          newVariable.type = AnimatorVariable.VariableType.Trigger;
          break;
      }

      dataAsset.Settings.variables[v] = newVariable;
    }

    //Mecanim State Graph
    int layerCount = controller.layers.Length;
    dataAsset.clips.Clear();
    dataAsset.Settings.layers = new AnimatorLayer[layerCount];
    for (int l = 0; l < layerCount; l++)
    {
      AnimatorLayer newLayer = new AnimatorLayer();
      newLayer.name = controller.layers[l].name;
      newLayer.id = l;

      int stateCount = controller.layers[l].stateMachine.states.Length;
      newLayer.states = new Quantum.AnimatorState[stateCount + 1];//additional element for the any state
      Dictionary<UnityEditor.Animations.AnimatorState, Quantum.AnimatorState> stateDictionary = new Dictionary<AnimatorState, Quantum.AnimatorState>();

      for (int s = 0; s < stateCount; s++)
      {
        UnityEditor.Animations.AnimatorState state = controller.layers[l].stateMachine.states[s].state;
        Quantum.AnimatorState newState = new Quantum.AnimatorState();
        newState.name = state.name;
        newState.id = state.nameHash;
        newState.isDefault = controller.layers[l].stateMachine.defaultState == state;
        newState.speed = FP.FromFloat_UNSAFE(state.speed);
        newState.cycleOffset = FP.FromFloat_UNSAFE(state.cycleOffset);

        if (state.motion != null)
        {
          AnimationClip clip = state.motion as AnimationClip;
          if (clip != null)
          {
            dataAsset.clips.Add(clip);
            AnimatorClip newClip = new AnimatorClip();
            newClip.name = state.motion.name;
            newClip.data = Extract(clip);
            newState.motion = newClip;

          }
          else
          {
            BlendTree tree = state.motion as BlendTree;
            if (tree != null)
            {
              int childCount = tree.children.Length;

              AnimatorBlendTree newBlendTree = new AnimatorBlendTree();
              newBlendTree.name = state.motion.name;
              newBlendTree.motionCount = childCount;
              newBlendTree.motions = new AnimatorMotion[childCount];
              newBlendTree.positions = new FPVector2[childCount];
              newBlendTree.weights = new FP[childCount];

              string parameterXname = tree.blendParameter;
              string parameterYname = tree.blendParameterY;
              for (int v = 0; v < variableCount; v++)
              {
                if (controller.parameters[v].name == parameterXname)
                  newBlendTree.blendParameterIndex = v;
                if (controller.parameters[v].name == parameterYname)
                  newBlendTree.blendParameterIndexY = v;
              }

              for (int c = 0; c < childCount; c++)
              {
                ChildMotion cMotion = tree.children[c];
                AnimationClip cClip = cMotion.motion as AnimationClip;
                newBlendTree.positions[c] = new FPVector2(FP.FromFloat_UNSAFE(cMotion.position.x), FP.FromFloat_UNSAFE(cMotion.position.y));

                if (cClip != null)
                {
                  dataAsset.clips.Add(cClip);
                  AnimatorClip newClip = new AnimatorClip();
                  newClip.data = Extract(cClip);
                  newClip.name = newClip.clipName;
                  newBlendTree.motions[c] = newClip;
                }
              }


              FP val = FP._0 / 21;
              newBlendTree.CalculateWeightTable(weightTableResolution);

              newState.motion = newBlendTree;
            }
          }
        }
        newLayer.states[s] = newState;

        stateDictionary.Add(state, newState);
      }

      //State Transistions
      //once the states have all been created
      //we'll hook up the transitions
      for (int s = 0; s < stateCount; s++)
      {
        UnityEditor.Animations.AnimatorState state = controller.layers[l].stateMachine.states[s].state;
        Quantum.AnimatorState newState = newLayer.states[s];
        int transitionCount = state.transitions.Length;
        newState.transitions = new Quantum.AnimatorTransition[transitionCount];
        for (int t = 0; t < transitionCount; t++)
        {
          AnimatorStateTransition transition = state.transitions[t];
          if (!stateDictionary.ContainsKey(transition.destinationState)) continue;
          Quantum.AnimatorTransition newTransition = new Quantum.AnimatorTransition();
          newTransition.index = t;
          newTransition.name = string.Format("{0} to {1}", state.name, transition.destinationState.name);
          newTransition.duration = FP.FromFloat_UNSAFE(transition.duration * state.motion.averageDuration);
          newTransition.hasExitTime = transition.hasExitTime;
          newTransition.exitTime = FP.FromFloat_UNSAFE(transition.exitTime * state.motion.averageDuration);
          newTransition.offset = FP.FromFloat_UNSAFE(transition.offset * transition.destinationState.motion.averageDuration);
          newTransition.destinationStateId = stateDictionary[transition.destinationState].id;
          newTransition.destinationStateName = stateDictionary[transition.destinationState].name;
          newTransition.canTransitionToSelf = transition.canTransitionToSelf;


          int conditionCount = transition.conditions.Length;
          newTransition.conditions = new Quantum.AnimatorCondition[conditionCount];
          for (int c = 0; c < conditionCount; c++)
          {
            UnityEditor.Animations.AnimatorCondition condition = state.transitions[t].conditions[c];

            if (!parameterDic.ContainsKey(condition.parameter)) continue;
            AnimatorControllerParameter parameter = parameterDic[condition.parameter];
            Quantum.AnimatorCondition newCondition = new Quantum.AnimatorCondition();

            newCondition.variableName = condition.parameter;
            newCondition.mode = (Quantum.AnimatorCondition.Modes)condition.mode;

            switch (parameter.type)
            {
              case AnimatorControllerParameterType.Float:
                newCondition.thresholdFp = FP.FromFloat_UNSAFE(condition.threshold);
                break;

              case AnimatorControllerParameterType.Int:
                newCondition.thresholdInt = Mathf.RoundToInt(condition.threshold);
                break;
            }

            newTransition.conditions[c] = newCondition;
          }

          newState.transitions[t] = newTransition;
        }
      }

      //Create Any State
      Quantum.AnimatorState anyState = new Quantum.AnimatorState();
      anyState.name = "Any State";
      anyState.id = anyState.name.GetHashCode();
      anyState.isAny = true;//important for this one
      AnimatorStateTransition[] anyStateTransitions = controller.layers[l].stateMachine.anyStateTransitions;
      int anyStateTransitionCount = anyStateTransitions.Length;
      anyState.transitions = new Quantum.AnimatorTransition[anyStateTransitionCount];
      for (int t = 0; t < anyStateTransitionCount; t++)
      {
        AnimatorStateTransition transition = anyStateTransitions[t];
        if (!stateDictionary.ContainsKey(transition.destinationState)) continue;
        Quantum.AnimatorTransition newTransition = new Quantum.AnimatorTransition();
        newTransition.index = t;
        newTransition.name = string.Format("Any State to {0}", transition.destinationState.name);
        newTransition.duration = FP.FromFloat_UNSAFE(transition.duration);
        newTransition.hasExitTime = transition.hasExitTime;
        newTransition.exitTime = FP._1;
        newTransition.offset = FP.FromFloat_UNSAFE(transition.offset * transition.destinationState.motion.averageDuration);
        newTransition.destinationStateId = stateDictionary[transition.destinationState].id;
        newTransition.destinationStateName = stateDictionary[transition.destinationState].name;
        newTransition.canTransitionToSelf = transition.canTransitionToSelf;

        int conditionCount = transition.conditions.Length;
        newTransition.conditions = new Quantum.AnimatorCondition[conditionCount];
        for (int c = 0; c < conditionCount; c++)
        {
          UnityEditor.Animations.AnimatorCondition condition = anyStateTransitions[t].conditions[c];

          if (!parameterDic.ContainsKey(condition.parameter)) continue;
          AnimatorControllerParameter parameter = parameterDic[condition.parameter];
          Quantum.AnimatorCondition newCondition = new Quantum.AnimatorCondition();

          newCondition.variableName = condition.parameter;
          newCondition.mode = (Quantum.AnimatorCondition.Modes)condition.mode;

          switch (parameter.type)
          {
            case AnimatorControllerParameterType.Float:
              newCondition.thresholdFp = FP.FromFloat_UNSAFE(condition.threshold);
              break;

            case AnimatorControllerParameterType.Int:
              newCondition.thresholdInt = Mathf.RoundToInt(condition.threshold);
              break;
          }

          newTransition.conditions[c] = newCondition;
        }

        anyState.transitions[t] = newTransition;
      }
      newLayer.states[stateCount] = anyState;

      dataAsset.Settings.layers[l] = newLayer;
    }

    EditorUtility.SetDirty(dataAsset);
  }

  public static AnimatorData Extract(AnimationClip clip)
  {
    AnimatorData animationData = new AnimatorData();
    animationData.clipName = clip.name;

    EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);

    float usedTime = settings.stopTime - settings.startTime;

    animationData.frameRate = Mathf.RoundToInt(clip.frameRate);
    animationData.length = FP.FromFloat_UNSAFE(usedTime);
    animationData.frameCount = Mathf.RoundToInt(clip.frameRate * usedTime);
    animationData.frames = new AnimatorFrame[animationData.frameCount];
    animationData.looped = clip.isLooping && settings.loopTime;
    animationData.mirror = settings.mirror;

    //Read the curves of animation
    int frameCount = animationData.frameCount;
    int curveBindingsLength = curveBindings.Length;
    if (curveBindingsLength == 0) return animationData;

    AnimationCurve curveTx = null, curveTy = null, curveTz = null, curveRx = null, curveRy = null, curveRz = null, curveRw = null;

    for (int c = 0; c < curveBindingsLength; c++)
    {
      string propertyName = curveBindings[c].propertyName;
      if (propertyName == "m_LocalPosition.x" || propertyName == "RootT.x")
        curveTx = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      if (propertyName == "m_LocalPosition.y" || propertyName == "RootT.y")
        curveTy = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      if (propertyName == "m_LocalPosition.z" || propertyName == "RootT.z")
        curveTz = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);

      if (propertyName == "m_LocalRotation.x" || propertyName == "RootQ.x")
        curveRx = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      if (propertyName == "m_LocalRotation.y" || propertyName == "RootQ.y")
        curveRy = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      if (propertyName == "m_LocalRotation.z" || propertyName == "RootQ.z")
        curveRz = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
      if (propertyName == "m_LocalRotation.w" || propertyName == "RootQ.w")
        curveRw = AnimationUtility.GetEditorCurve(clip, curveBindings[c]);
    }

    //        if (curveBindingsLength >= 7)
    //        {
    //            //Position Curves
    //            curveTx = AnimationUtility.GetEditorCurve(clip, curveBindings[0]);
    //            curveTy = AnimationUtility.GetEditorCurve(clip, curveBindings[1]);
    //            curveTz = AnimationUtility.GetEditorCurve(clip, curveBindings[2]);
    //
    //            //Rotation Curves
    //            curveRx = AnimationUtility.GetEditorCurve(clip, curveBindings[3]);
    //            curveRy = AnimationUtility.GetEditorCurve(clip, curveBindings[4]);
    //            curveRz = AnimationUtility.GetEditorCurve(clip, curveBindings[5]);
    //            curveRw = AnimationUtility.GetEditorCurve(clip, curveBindings[6]);
    //        }

    bool hasPosition = curveTx != null && curveTy != null && curveTz != null;
    bool hasRotation = curveRx != null && curveRy != null && curveRz != null && curveRw != null;

    if (!hasPosition) Debug.LogWarning("No movement data was found in the animation: " + clip.name);
    if (!hasRotation) Debug.LogWarning("No rotation data was found in the animation: " + clip.name);

    //The initial pose might not be the first frame and might not face foward
    //calculate the initial direction and create an offset Quaternion to apply to transforms;

    Quaternion startRotUq = Quaternion.identity;
    FPQuaternion startRot = FPQuaternion.Identity;
    if (hasRotation)
    {
      float srotxu = curveRx.Evaluate(settings.startTime);
      float srotyu = curveRy.Evaluate(settings.startTime);
      float srotzu = curveRz.Evaluate(settings.startTime);
      float srotwu = curveRw.Evaluate(settings.startTime);

      FP srotx = FP.FromFloat_UNSAFE(srotxu);
      FP sroty = FP.FromFloat_UNSAFE(srotyu);
      FP srotz = FP.FromFloat_UNSAFE(srotzu);
      FP srotw = FP.FromFloat_UNSAFE(srotwu);

      startRotUq = new Quaternion(srotxu, srotyu, srotzu, srotwu);
      startRot = new FPQuaternion(srotx, sroty, srotz, srotw);
    }

    Quaternion offsetRotUq = Quaternion.Inverse(startRotUq);
    FPQuaternion offsetRot = FPQuaternion.Inverse(startRot);

    for (int i = 0; i < frameCount; i++)
    {
      var frameData = new AnimatorFrame();
      frameData.id = i;
      float percent = i / (frameCount - 1f);
      float frameTime = usedTime * percent;
      frameData.time = FP.FromFloat_UNSAFE(frameTime);
      float clipTIme = settings.startTime + percent * (settings.stopTime - settings.startTime);

      if (hasPosition)
      {
        FP posx = FP.FromFloat_UNSAFE(i > 0 ? curveTx.Evaluate(clipTIme) - curveTx.Evaluate(settings.startTime) : 0);
        FP posy = FP.FromFloat_UNSAFE(i > 0 ? curveTy.Evaluate(clipTIme) - curveTy.Evaluate(settings.startTime) : 0);
        FP posz = FP.FromFloat_UNSAFE(i > 0 ? curveTz.Evaluate(clipTIme) - curveTz.Evaluate(settings.startTime) : 0);
        FPVector3 newPosition = offsetRot * new FPVector3(posx, posy, posz);
        if (settings.mirror) newPosition.X = -newPosition.X;
        frameData.position = newPosition;
      }

      if (hasRotation)
      {
        float curveRxEval = curveRx.Evaluate(clipTIme);
        float curveRyEval = curveRy.Evaluate(clipTIme);
        float curveRzEval = curveRz.Evaluate(clipTIme);
        float curveRwEval = curveRw.Evaluate(clipTIme);
        Quaternion curveRotation = offsetRotUq * new Quaternion(curveRxEval, curveRyEval, curveRzEval, curveRwEval);
        if (settings.mirror)//mirror the Y axis rotation
        {
          Quaternion mirrorRotation = new Quaternion(curveRotation.x, -curveRotation.y, -curveRotation.z, curveRotation.w);

          if (Quaternion.Dot(curveRotation, mirrorRotation) < 0)
          {
            mirrorRotation = new Quaternion(-mirrorRotation.x, -mirrorRotation.y, -mirrorRotation.z, -mirrorRotation.w);
          }

          curveRotation = mirrorRotation;
        }

        FP rotx = FP.FromFloat_UNSAFE(curveRotation.x);
        FP roty = FP.FromFloat_UNSAFE(curveRotation.y);
        FP rotz = FP.FromFloat_UNSAFE(curveRotation.z);
        FP rotw = FP.FromFloat_UNSAFE(curveRotation.w);
        FPQuaternion newRotation = new FPQuaternion(rotx, roty, rotz, rotw);
        frameData.rotation = newRotation * offsetRot;

        float rotY = curveRotation.eulerAngles.y * Mathf.Deg2Rad;
        while (rotY < -Mathf.PI) rotY += Mathf.PI * 2;
        while (rotY > Mathf.PI) rotY += -Mathf.PI * 2;
        frameData.rotationY = FP.FromFloat_UNSAFE(rotY);
      }

      animationData.frames[i] = frameData;
    }

    return animationData;
  }
}
