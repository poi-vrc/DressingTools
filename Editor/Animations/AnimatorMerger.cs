using System;
using System.Collections.Generic;
using System.Linq;
using Chocopoi.DressingFramework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Animations.VRChat;
#endif

namespace Chocopoi.DressingTools.Animations
{
    /// <summary>
    /// Temporarily made a copy here from AvatarLib
    /// </summary>
    internal class AnimatorMerger
    {
        public enum WriteDefaultsMode
        {
            DoNothing = 0,
            On = 1,
            Off = 2
        }

        private readonly Context _ctx;
        private readonly AnimatorController _mergedAnimator;
        private readonly Dictionary<Tuple<string, AnimatorStateMachine>, AnimatorStateMachine> _stateMachineCache;
        private readonly Dictionary<string, AnimatorControllerParameter> _parameters;
        private readonly Dictionary<string, AnimatorControllerLayer> _layers;
        private readonly Dictionary<string, int> _layerNameDuplicateCount;

        public AnimatorMerger(Context ctx, string name)
        {
            _ctx = ctx;
            _mergedAnimator = new AnimatorController();
            ctx.CreateUniqueAsset(_mergedAnimator, name);
            _stateMachineCache = new Dictionary<Tuple<string, AnimatorStateMachine>, AnimatorStateMachine>();
            _parameters = new Dictionary<string, AnimatorControllerParameter>();
            _layers = new Dictionary<string, AnimatorControllerLayer>();
            _layerNameDuplicateCount = new Dictionary<string, int>();
        }

        public void AddAnimator(string rebasePath, AnimatorController animator, WriteDefaultsMode writeDefaultsMode = WriteDefaultsMode.DoNothing)
        {
            // deep copy parameters
            DeepCopyParameters(animator);

            // deep copy layers
            var layers = animator.layers;
            for (var i = 0; i < layers.Length; i++)
            {
                var originalLayer = layers[i];
                var weight = i == 0 ? 1 : originalLayer.defaultWeight;

                var copyCache = new Dictionary<Object, Object>();

                var newLayerName = originalLayer.name;
                if (_layers.ContainsKey(originalLayer.name))
                {
                    if (!_layerNameDuplicateCount.TryGetValue(originalLayer.name, out var count))
                    {
                        count = 1;
                    }
                    newLayerName += "_" + count;
                    _layerNameDuplicateCount[originalLayer.name] = ++count;
                }

                // create copy
                var newLayer = new AnimatorControllerLayer()
                {
                    name = newLayerName,
                    defaultWeight = weight,
                    blendingMode = originalLayer.blendingMode,
                    iKPass = originalLayer.iKPass,
                    avatarMask = originalLayer.avatarMask,
                    syncedLayerIndex = originalLayer.syncedLayerIndex,
                    syncedLayerAffectsTiming = originalLayer.syncedLayerAffectsTiming,
                    stateMachine = DeepCopyStateMachineWithRebasing(rebasePath, originalLayer.stateMachine, copyCache),
                };

                HandleWriteDefaults(newLayer.stateMachine, writeDefaultsMode);

                if (originalLayer.syncedLayerIndex != -1 && originalLayer.syncedLayerIndex >= 0 && originalLayer.syncedLayerIndex < layers.Length)
                {
                    HandleSyncedLayer(originalLayer, layers[originalLayer.syncedLayerIndex], newLayer, copyCache);
                }

                _layers.Add(newLayer.name, newLayer);
            }
        }

        public AnimatorController Merge()
        {
            _mergedAnimator.layers = _layers.Values.ToArray();
            _mergedAnimator.parameters = _parameters.Values.ToArray();
            EditorUtility.SetDirty(_mergedAnimator);
            return _mergedAnimator;
        }

        private void HandleSyncedLayer(AnimatorControllerLayer originalLayer, AnimatorControllerLayer baseLayer, AnimatorControllerLayer newLayer, Dictionary<Object, Object> copyCache)
        {
            var it = WalkStateMachine(baseLayer.stateMachine);
            foreach (var state in it)
            {
                var animState = (AnimatorState)copyCache[state];

                // copy override motion
                var motion = originalLayer.GetOverrideMotion(state);
                if (motion != null)
                {
                    newLayer.SetOverrideMotion(animState, motion);
                }

                // copy override behaviours
                var behaviours = originalLayer.GetOverrideBehaviours(state);
                if (behaviours != null)
                {
                    var copy = new StateMachineBehaviour[behaviours.Length];
                    behaviours.CopyTo(copy, 0);

                    for (var i = 0; i < copy.Length; i++)
                    {
                        behaviours[i] = GenericDeepCopy(behaviours[i]);
                        HandleAnimatorBehaviour(behaviours[i]);
                    }

                    newLayer.SetOverrideBehaviours(animState, copy);
                }
            }
            newLayer.syncedLayerIndex += _layers.Count;
        }

        private static string RebaseCurvePath(EditorCurveBinding curveBinding, string rebasePath)
        {
            if (rebasePath == "" && curveBinding.type == typeof(Animator))
            {
                return "";
            }

            return curveBinding.path == "" ?
                rebasePath :
                (rebasePath + "/" + curveBinding.path);
        }

        private void HandleWriteDefaults(AnimatorStateMachine stateMachine, WriteDefaultsMode writeDefaultsMode)
        {
            if (writeDefaultsMode == WriteDefaultsMode.DoNothing)
            {
                return;
            }

            // iterate all states
            foreach (var state in stateMachine.states)
            {
                state.state.writeDefaultValues = writeDefaultsMode == WriteDefaultsMode.On;
            }

            // iterate all child state machines
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                HandleWriteDefaults(childStateMachine.stateMachine, writeDefaultsMode);
            }
        }

        private AnimatorStateMachine DeepCopyStateMachineWithRebasing(string rebasePath, AnimatorStateMachine stateMachine, Dictionary<Object, Object> copyCache = null)
        {
            if (copyCache == null)
            {
                copyCache = new Dictionary<Object, Object>();
            }

            // attempt to find cache to not copy again
            var key = new Tuple<string, AnimatorStateMachine>(rebasePath, stateMachine);
            if (_stateMachineCache.TryGetValue(key, out var cachedStateMachine))
            {
                return cachedStateMachine;
            }

            var newStateMachine = GenericDeepCopy(stateMachine, obj =>
            {
                if (!(obj is AnimationClip))
                {
                    return null;
                }

                var anim = (AnimationClip)obj;
                if (rebasePath == "") return anim;
#if DT_VRCSDK3A
                if (VRCAnimUtils.IsProxyAnimation(anim)) return anim;
#endif

                // create new copy
                var newAnim = new AnimationClip
                {
                    name = anim.name + "_Rebase",
                    legacy = anim.legacy,
                    frameRate = anim.frameRate,
                    localBounds = anim.localBounds,
                    wrapMode = anim.wrapMode
                };
                AnimationUtility.SetAnimationClipSettings(newAnim, AnimationUtility.GetAnimationClipSettings(anim));

                AssetDatabase.AddObjectToAsset(newAnim, _mergedAnimator);

                // object reference curves
                var objRefBindings = AnimationUtility.GetObjectReferenceCurveBindings(anim);
                foreach (var objRefBinding in objRefBindings)
                {
                    var newObjRefBinding = objRefBinding;
                    newObjRefBinding.path = RebaseCurvePath(objRefBinding, rebasePath);
                    AnimationUtility.SetObjectReferenceCurve(newAnim, newObjRefBinding, AnimationUtility.GetObjectReferenceCurve(anim, objRefBinding));
                }

                // curves
                var curveBindings = AnimationUtility.GetCurveBindings(anim);
                foreach (var curveBinding in curveBindings)
                {
                    newAnim.SetCurve(RebaseCurvePath(curveBinding, rebasePath), curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(anim, curveBinding));
                }

                return newAnim;
            }, copyCache);

            // update animator behaviours
            var it = WalkStateMachine(stateMachine);
            foreach (var state in it)
            {
                foreach (var behaviour in state.behaviours)
                {
                    HandleAnimatorBehaviour(behaviour);
                }
            }

            // save to cache
            _stateMachineCache[key] = newStateMachine;
            return newStateMachine;
        }

        private void HandleAnimatorBehaviour(StateMachineBehaviour behaviour)
        {
#if DT_VRCSDK3A
            if (behaviour is VRC.SDK3.Avatars.Components.VRCAnimatorLayerControl control)
            {
                // update control layer index
                control.layer = _layers.Count;
            }
#endif
        }

        private void DeepCopyParameters(AnimatorController animator)
        {
            foreach (var param in animator.parameters)
            {
                if (_parameters.TryGetValue(param.name, out var existingType))
                {
                    if (param.type != existingType.type)
                    {
                        // throw exception if type mismatch
                        _ctx.Report.LogWarn("AnimatorMerger", $"Animator types mismatch for {param.name}: current: {param.type} existing {existingType}, forcing to use float type");
                        _parameters[param.name].type = AnimatorControllerParameterType.Float;
                    }
                }
                else
                {
                    // deep copy parameter
                    var newParam = new AnimatorControllerParameter
                    {
                        name = param.name,
                        type = param.type,
                        defaultBool = param.defaultBool,
                        defaultFloat = param.defaultFloat,
                        defaultInt = param.defaultInt
                    };
                    _parameters.Add(param.name, newParam);
                }
            }
        }

        private static IEnumerable<AnimatorState> WalkStateMachine(AnimatorStateMachine stateMachine, HashSet<AnimatorStateMachine> visitedStateMachines = null)
        {
            if (visitedStateMachines == null)
            {
                visitedStateMachines = new HashSet<AnimatorStateMachine>();
            }

            // do not walk visited state machines
            if (visitedStateMachines.Contains(stateMachine))
            {
                yield break;
            }
            visitedStateMachines.Add(stateMachine);

            // yield all states
            foreach (var state in stateMachine.states)
            {
                if (state.state == null)
                {
                    continue;
                }

                yield return state.state;
            }

            // recursive visit other state machines
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                if (stateMachine.stateMachines == null)
                {
                    continue;
                }

                if (visitedStateMachines.Contains(childStateMachine.stateMachine))
                {
                    continue;
                }
                visitedStateMachines.Add(childStateMachine.stateMachine);

                // walk the child state machine
                var states = WalkStateMachine(childStateMachine.stateMachine, visitedStateMachines);
                foreach (var state in states)
                {
                    yield return state;
                }
            }
        }

        // TODO: reuse from DK, probably move to avatarlib with a more generic implementation
        private T GenericDeepCopy<T>(T originalObject, Func<Object, Object> genericCopyFunc = null, Dictionary<Object, Object> copyCache = null) where T : Object
        {
            if (copyCache == null)
            {
                copyCache = new Dictionary<Object, Object>();
            }

            if (originalObject == null)
            {
                return null;
            }

            var originalObjectType = originalObject.GetType();

            // do not copy these types and return original
            if (originalObject is MonoScript ||
                originalObject is ScriptableObject ||
                originalObject is Texture ||
                originalObject is Material)
            {
                return originalObject;
            }

            // only copy known types
            if (!(originalObject is Motion ||
                originalObject is AnimatorController ||
                originalObject is AnimatorStateMachine ||
                originalObject is StateMachineBehaviour ||
                originalObject is AnimatorState ||
                originalObject is AnimatorTransitionBase))
            {
                throw new Exception(string.Format("Unknown type detected while animator merging: {0}", originalObjectType.FullName));
            }

            // try obtain from cache
            if (copyCache.TryGetValue(originalObject, out var obj))
            {
                return (T)obj;
            }

            Object newObj;

            // attempt to copy with generic copy function
            if (genericCopyFunc != null)
            {
                newObj = genericCopyFunc(originalObject);
                if (newObj != null)
                {
                    return (T)newObj;
                }
            }

            // initialize a new object in a generic way
            var constructor = originalObjectType.GetConstructor(System.Type.EmptyTypes);
            if (constructor != null && !(originalObject is ScriptableObject))
            {
                newObj = (T)System.Activator.CreateInstance(originalObjectType);
                // copy serialized properties
                EditorUtility.CopySerialized(originalObject, newObj);
            }
            else
            {
                newObj = Object.Instantiate(originalObject);
            }
            copyCache[originalObject] = newObj;

            // save to assets
            AssetDatabase.AddObjectToAsset(newObj, _mergedAnimator);

            // deep copy serialized properties
            var serializedObj = new SerializedObject(newObj);
            var it = serializedObj.GetIterator();

            bool traverseDown = true;
            while (it.Next(traverseDown))
            {
                // reset
                traverseDown = true;

                if (it.propertyType == SerializedPropertyType.String)
                {
                    // disable traversal
                    traverseDown = false;
                }
                else if (it.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // recursively perform deep copy
                    it.objectReferenceValue = GenericDeepCopy(it.objectReferenceValue, genericCopyFunc, copyCache);
                }
            }

            // apply changes
            serializedObj.ApplyModifiedPropertiesWithoutUndo();

            return (T)newObj;
        }
    }
}
