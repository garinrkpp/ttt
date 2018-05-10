using Photon.Deterministic;
using Quantum;
using System;
using System.Collections.Generic;
using UnityEngine;

public unsafe class EntityPrefabViewUpdater : QuantumCallbacks {
  // current set of entities that should be removed
  HashSet<EntityRef> _removeEntities = new HashSet<EntityRef>();

  // current set of active entities
  HashSet<EntityRef> _activeEntities = new HashSet<EntityRef>();

  // current set of active prefabs
  Dictionary<EntityRef, EntityPrefabRoot> _activePrefabs = new Dictionary<EntityRef, EntityPrefabRoot>(256);

  public override void OnUpdateView() {
    if (QuantumGame.Running) {
      _activeEntities.Clear();
      _removeEntities.Clear();

      using (var entities = QuantumGame.Frame.GetAllEntities()) {
        for (Int32 i = 0; i < entities.Count; ++i) {
          var instance = default(EntityPrefabRoot);

          var entity = entities.Items[i].Entity;
          var transform = Entity.GetTransform2D(entity);

          var prefab = Entity.GetPrefab(entity);
          if (prefab != null) {
            var entityPrefab = prefab->Current;
            if (entityPrefab != null) {
              _activeEntities.Add(entity->EntityRef);

              if (_activePrefabs.TryGetValue(entity->EntityRef, out instance)) {
                if (instance.AssetGuid != entityPrefab.Guid) {
                  // destroy current
                  DestroyPrefab(entity->EntityRef);

                  // create new
                  instance = CreatePrefab(entity->EntityRef, entityPrefab, transform);

                  // add to active set
                  _activeEntities.Add(entity->EntityRef);
                }
              }
              else {
                // create new
                instance = CreatePrefab(entity->EntityRef, entityPrefab, transform);

                // add to active set
                _activeEntities.Add(entity->EntityRef);
              }
            }
            else {
              DestroyPrefab(entity->EntityRef);

              // clear active
              instance = null;
            }
          }

          if (instance) {
            if (transform != null) {
              var position = transform->Position.ToUnityVector3();

              var entityPrevious = QuantumGame.FramePredictedPrevious.GetEntity(entity->EntityRef);
              if (entityPrevious != null) {
                var transformPrevious = Entity.GetTransform2D(entityPrevious);
                if (transformPrevious != null) {
                  position = Vector3.Lerp(transformPrevious->Position.ToUnityVector3(), position, QuantumGame.FrameInterpolationFactor);
                }
              }

              if (instance.InterpolatePositionSpeed > 0) {
                instance.transform.position = Vector3.Lerp(instance.transform.position, position, (1 / instance.InterpolatePositionSpeed) * Time.deltaTime);
              }
              else {
                instance.transform.position = position;
              }

              if (instance.InterpolateRotationSpeed > 0) {
                instance.transform.rotation = Quaternion.Lerp(instance.transform.rotation, transform->Rotation.ToUnityQuaternion(), (1 / instance.InterpolateRotationSpeed) * Time.deltaTime);
              }
              else {
                instance.transform.rotation = transform->Rotation.ToUnityQuaternion();
              }
            }

            if (instance.QuantumAnimator) {
              var animator = Entity.GetAnimator(entity);
//              AnimatorGraph graph = (AnimatorGraph)DB.FastUnsafe[animator->id];
//              Debug.Log("OnUpdateView "+ graph.rootMotion);
              if (animator != null) {
                instance.QuantumAnimator.Animate(animator);
              }
            }
          }
        }
      }

      // find outdated ones
      foreach (var key in _activePrefabs) {
        if (_activeEntities.Contains(key.Key) == false) {
          _removeEntities.Add(key.Key);
        }
      }

      // destroy outdated
      foreach (var key in _removeEntities) {
        DestroyPrefab(key);
      }
    }
  }

  EntityPrefabRoot CreatePrefab(EntityRef entityRef, EntityPrefab prefab, Transform2D* transform) {
    var asset = UnityDB.FindAsset<EntityPrefabAsset>(prefab.Id);
    if (asset) {
      EntityPrefabRoot instance;

      instance = transform == null ? Instantiate(asset.Prefab) : Instantiate(asset.Prefab, transform->Position.ToUnityVector2(), transform->Rotation.ToUnityQuaternion());
      instance.AssetGuid = prefab.Guid;
      instance.EntityRef = entityRef;

      if (transform != null) {
        instance.transform.position = transform->Position.ToUnityVector3();
        instance.transform.rotation = transform->Rotation.ToUnityQuaternion();
      }

      // add to lookup
      _activePrefabs.Add(entityRef, instance);

      // return instance
      return instance;
    }

    return null;
  }

  void DestroyPrefab(EntityRef entityRef) {
    EntityPrefabRoot prefab;

    if (_activePrefabs.TryGetValue(entityRef, out prefab)) {
      Destroy(prefab.gameObject);
    }

    _activePrefabs.Remove(entityRef);
  }

  void OnDestroy() {
    foreach (var kvp in _activePrefabs) {
      if (kvp.Value && kvp.Value.gameObject) {
        Destroy(kvp.Value.gameObject);
      }
    }
  }
}
