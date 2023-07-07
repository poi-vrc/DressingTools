using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public class RootObjectsHook : IDefaultDresserHook
    {
        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings, List<DTObjectMapping> objectMappings)
        {
            // map all objects in wearable root except the armature
            for (int i = 0; i < settings.targetWearable.transform.childCount; i++)
            {
                var child = settings.targetWearable.transform.GetChild(i);

                if (child.name != settings.wearableArmatureName)
                {
                    objectMappings.Add(new DTObjectMapping()
                    {
                        avatarObjectPath = "", // root
                        wearableObjectPath = AnimationUtils.GetRelativePath(child.transform, settings.targetWearable.transform)
                    });
                }
            }

            return true;
        }
    }
}
