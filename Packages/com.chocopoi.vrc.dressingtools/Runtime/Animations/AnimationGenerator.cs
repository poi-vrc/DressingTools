using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    public class AnimationGenerator
    {
        private DTReport report;

        private GameObject avatarObject;

        private DTWearableConfig config;

        private GameObject wearableObject;

        private List<IDynamicsProxy> wearableDynamics;

        public AnimationGenerator(DTReport report, GameObject avatarObject, DTWearableConfig config, GameObject wearableObject, List<IDynamicsProxy> wearableDynamics)
        {
            this.report = report;
            this.avatarObject = avatarObject;
            this.config = config;
            this.wearableObject = wearableObject;
            this.wearableDynamics = wearableDynamics;
        }

        private bool TryGetBlendshapeValue(GameObject obj, string blendshapeName, out float value)
        {
            value = -1.0f;

            SkinnedMeshRenderer smr;
            if ((smr = obj.GetComponent<SkinnedMeshRenderer>()) == null)
            {
                report.LogWarn(0, string.Format("Object {0} does not have SkinnedMeshRenderer attached", obj.name));
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                report.LogWarn(0, string.Format("SkinnedMeshRenderer in {0} has no mesh attached", obj.name));
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                report.LogWarn(0, string.Format("Mesh in {0} has no such blendshape named \"{1}\"", obj.name, blendshapeName));
                return false;
            }

            value = smr.GetBlendShapeWeight(blendshapeIndex);
            return true;
        }

        public AnimationClip GenerateWearAnimation(bool invertStates, bool writeDefaults)
        {
            var clip = new AnimationClip();

            // prevent unexpected behaviour
            if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            // if write defaults, write enabled=true once
            // if not write defaults, write enabled=true and false for both
            if (!invertStates || (invertStates && !writeDefaults))
            {
                // avatar toggles
                foreach (var toggle in config.avatarAnimationOnWear.toggles)
                {
                    var obj = avatarObject.transform.Find(toggle.path);
                    if (obj == null)
                    {
                        report.LogWarn(0, "Could not find avatar toggle GameObject at path, ignoring: " + toggle.path);
                    }
                    else
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(clip, toggle.path, toggle.state ^ invertStates);
                    }
                }

                // wearable toggles
                foreach (var toggle in config.wearableAnimationOnWear.toggles)
                {
                    var obj = wearableObject.transform.Find(toggle.path);
                    if (obj == null)
                    {
                        report.LogWarn(0, "Could not find wearable toggle GameObject at path, ignoring: " + toggle.path);
                    }
                    else
                    {
                        AnimationUtils.SetSingleFrameGameObjectEnabledCurve(clip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), toggle.state ^ invertStates);
                    }
                }

                // dynamics
                var visitedDynamicsTransforms = new List<Transform>();
                foreach (var dynamics in wearableDynamics)
                {
                    if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, dynamics.Transform))
                    {
                        throw new System.Exception(string.Format("Dynamics {0} is not inside avatar {1}, aborting", dynamics.Transform.name, avatarObject.name));
                    }

                    if (visitedDynamicsTransforms.Contains(dynamics.Transform))
                    {
                        // skip duplicates since it's meaningless
                        continue;
                    }

                    // enable/disable dynamics object
                    AnimationUtils.SetSingleFrameGameObjectEnabledCurve(clip, AnimationUtils.GetRelativePath(dynamics.Transform, avatarObject.transform), !invertStates);

                    // mark as visited
                    visitedDynamicsTransforms.Add(dynamics.Transform);
                }
            }

            // avatar blendshapes
            foreach (var blendshape in config.avatarAnimationOnWear.blendshapes)
            {
                var obj = avatarObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    report.LogWarn(0, string.Format("Could not find avatar object {0} for blendshape animating at path, ignoring: {1}", avatarObject.name, blendshape.path));
                    continue;
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    report.LogWarn(0, string.Format("Could not get the original blendshape value of avatar object {0} for blendshape animating at path, ignoring: {1}", avatarObject.name, blendshape.path));
                    continue;
                }

                if (invertStates)
                {
                    if (!writeDefaults)
                    {
                        // write the original value if not write defaults
                        AnimationUtils.SetSingleFrameBlendshapeCurve(clip, blendshape.path, blendshape.blendshapeName, originalValue);
                    }
                }
                else
                {
                    AnimationUtils.SetSingleFrameBlendshapeCurve(clip, blendshape.path, blendshape.blendshapeName, blendshape.value);
                }
            }

            // wearable blendshapes
            foreach (var blendshape in config.wearableAnimationOnWear.blendshapes)
            {
                var obj = wearableObject.transform.Find(blendshape.path);
                if (obj == null)
                {
                    report.LogWarn(0, "Could not find wearable object " + wearableObject.name + " for blendshape animating at path, ignoring: " + blendshape.path);
                }

                if (!TryGetBlendshapeValue(obj.gameObject, blendshape.blendshapeName, out var originalValue))
                {
                    report.LogWarn(0, string.Format("Could not get the original blendshape value of wearable object {0} for blendshape animating at path, ignoring: {1}", avatarObject.name, blendshape.path));
                    continue;
                }

                if (invertStates)
                {
                    if (!writeDefaults)
                    {
                        // write the original value if not write defaults
                        AnimationUtils.SetSingleFrameBlendshapeCurve(clip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), blendshape.blendshapeName, originalValue);
                    }
                }
                else
                {
                    AnimationUtils.SetSingleFrameBlendshapeCurve(clip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), blendshape.blendshapeName, blendshape.value);
                }
            }

            return clip;
        }
    }
}
