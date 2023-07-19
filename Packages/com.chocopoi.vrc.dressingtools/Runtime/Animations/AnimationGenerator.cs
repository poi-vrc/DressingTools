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

        public AnimationClip GenerateWearAnimation(bool invertStates = false)
        {
            if (!DTRuntimeUtils.IsGrandParent(avatarObject.transform, wearableObject.transform))
            {
                throw new System.Exception("Wearable object is not inside avatar! Cannot proceed animation generation.");
            }

            var clip = new AnimationClip();

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

            if (!invertStates)
            {
                // avatar blendshapes
                foreach (var blendshape in config.avatarAnimationOnWear.blendshapes)
                {
                    AnimationUtils.SetSingleFrameBlendshapeCurve(clip, blendshape.path, blendshape.blendshapeName, blendshape.value);
                }

                // wearable blendshapes
                foreach (var blendshape in config.wearableAnimationOnWear.blendshapes)
                {
                    var obj = wearableObject.transform.Find(blendshape.path);
                    if (obj == null)
                    {
                        report.LogWarn(0, "Could not find wearable object " + wearableObject.name + " for blendshape animating at path, ignoring: " + blendshape.path);
                    }
                    else if (obj.GetComponent<SkinnedMeshRenderer>() == null)
                    {
                        report.LogWarn(0, "Wearable object " + wearableObject.name + " has no SkinnedMeshRenderer attached, ignoring: " + blendshape.path);
                    }
                    else
                    {
                        AnimationUtils.SetSingleFrameBlendshapeCurve(clip, AnimationUtils.GetRelativePath(obj.transform, avatarObject.transform), blendshape.blendshapeName, blendshape.value);
                    }
                }
            }

            return clip;
        }
    }
}
