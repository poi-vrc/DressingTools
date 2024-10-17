/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Dresser.Tags;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Localization;
using UnityEngine;
using static Chocopoi.DressingTools.Dresser.Standard.StandardDresserSettings;

namespace Chocopoi.DressingTools.Dresser.Standard
{
    internal class StandardDresser : IDresser
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        public const string LogLabel = "StandardDresser";
        private const string BoneContainerSuffix = "_DT";

        public static class MessageCode
        {
            //Warnings
            public const string BonesNotMatchingInArmatureFirstLevel = "dressers.standard.msgCode.warn.bonesNotMatchingInArmatureFirstLevel";

            //Errors
            public const string NoBonesInTargetArmatureFirstLevel = "dressers.standard.msgCode.error.noBonesInTargetArmatureFirstLevel";
            public const string NoBonesInSourceArmatureFirstLevel = "dressers.standard.msgCode.error.noBonesInSourceArmatureFirstLevel";
            public const string MissingScriptsDetectedInAvatar = "dressers.standard.msgCode.error.missingScriptsDetectedInAvatar";
            public const string MissingTargetArmaturePath = "dressers.standard.msgCode.error.missingTargetArmaturePath";
            public const string MissingSourceArmature = "dressers.standard.msgCode.error.missingSourceArmature";
            public const string CouldNotLocateTargetArmature = "dressers.standard.msgCode.error.couldNotLocateTargetArmature";
            public const string SourceArmatureNotInsideAvatar = "dressers.standard.msgCode.error.sourceArmatureNotInsideAvatar";
        }

        public bool CheckNoMissingScripts(Report report, string errorCode, GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.LogErrorLocalized(t, LogLabel, errorCode, gameObject.transform.parent == null ? gameObject.name : AnimationUtils.GetRelativePath(gameObject.transform));
                    return false;
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                if (!CheckNoMissingScripts(report, errorCode, child.gameObject))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool QuaternionApproximately(Quaternion a, Quaternion b, float threshold)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(a, b)) < threshold;
        }

        private static DynamicsOptions DetectDynamicsOption(Report report, Transform avatarRoot, Transform sourceTrans, Transform targetTrans)
        {
            // Some clothes creators copy body bones from other avatars for the breast bone parts.
            // Probably for saving some time fitting for multiple avatars.
            // Prone to position and rotation issues if we use ParentConstraint.
            // To workaround with this issue, we try to detect frankenstein bones and use IgnoreTransform
            // to let them move their own.

            var posSimilar = Vector3.Distance(sourceTrans.position, targetTrans.position) < 0.01f;
            var rotSimilar = QuaternionApproximately(sourceTrans.localRotation, targetTrans.localRotation, 0.01f);

            // use world position and rotation to determine if they are frankenstein bones
            if (!posSimilar || !rotSimilar)
            {
                report.LogInfo(LogLabel, $"Using IgnoreTransform for frankenstein bone: {AnimationUtils.GetRelativePath(sourceTrans, avatarRoot)} ({sourceTrans.position}) ({sourceTrans.rotation}) -> {AnimationUtils.GetRelativePath(targetTrans, avatarRoot)} ({targetTrans.position}) ({targetTrans.rotation}): pos {posSimilar} rot {rotSimilar}");
                // frankenstein bone, cannot use parentconstraint
                return DynamicsOptions.IgnoreTransform;
            }

            return DynamicsOptions.RemoveDynamicsAndUseParentConstraint;
        }

        private static void AddParentConstraintsDownwards(Transform avatarRoot, Transform sourceTrans, Transform targetTrans, List<ObjectMapping> objectMappings)
        {
            // add object mapping
            objectMappings.Add(new ObjectMapping()
            {
                Type = ObjectMapping.MappingType.ParentConstraint,
                SourceTransform = sourceTrans,
                TargetPath = AnimationUtils.GetRelativePath(targetTrans, avatarRoot)
            });

            // scan for other child bones
            for (var i = 0; i < sourceTrans.childCount; i++)
            {
                var child = sourceTrans.GetChild(i);
                var avatarTrans = DresserUtils.FindMatchingBone(targetTrans, child.name);
                if (avatarTrans != null)
                {
                    AddParentConstraintsDownwards(avatarRoot, child, avatarTrans, objectMappings);
                }
            }
        }

        private static void AddIgnoreTransformsDownwards(Transform avatarRoot, Transform sourceTrans, Transform targetTrans, List<ObjectMapping> objectMappings)
        {
            objectMappings.Add(new ObjectMapping()
            {
                Type = ObjectMapping.MappingType.IgnoreTransform,
                SourceTransform = sourceTrans,
                TargetPath = AnimationUtils.GetRelativePath(targetTrans, avatarRoot),
            });

            for (var i = 0; i < sourceTrans.childCount; i++)
            {
                var child = sourceTrans.GetChild(i);
                var avatarTrans = DresserUtils.FindMatchingBone(targetTrans, child.name);
                if (avatarTrans != null)
                {
                    AddIgnoreTransformsDownwards(avatarRoot, child, avatarTrans, objectMappings);
                }
            }
        }

        private static void HandleBoneDynamics(DynamicsOptions option, Transform avatarRoot, Transform sourceTrans, Transform targetTrans, IDynamics sourceDynamics, IDynamics targetDynamics, List<ObjectMapping> objectMappings, List<ITag> tags)
        {
            if (option == DynamicsOptions.RemoveDynamicsAndUseParentConstraint ||
                (option == DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary &&
                 sourceDynamics == null))
            {
                AddParentConstraintsDownwards(avatarRoot, sourceTrans, targetTrans, objectMappings);
            }
            else if (option == DynamicsOptions.IgnoreTransform)
            {
                AddIgnoreTransformsDownwards(avatarRoot, sourceTrans, targetTrans, objectMappings);
            }
            else if (option == DynamicsOptions.CopyDynamics)
            {
                tags.Add(new CopyDynamicsTag()
                {
                    SourceTransform = sourceTrans,
                    TargetPath = AnimationUtils.GetRelativePath(targetTrans, avatarRoot)
                });
            }
        }

        private static void ProcessBone(Report report, DynamicsOptions dynamicsOption, Transform avatarRoot, Transform sourceArmatureParent, Transform targetArmatureParent, List<IDynamics> allDynamics, List<ObjectMapping> objectMappings, List<ITag> tags, int level)
        {
            for (var i = 0; i < sourceArmatureParent.childCount; i++)
            {
                var child = sourceArmatureParent.GetChild(i);
                if (child.name.EndsWith(BoneContainerSuffix))
                {
                    continue;
                }

                var targetTrans = DresserUtils.FindMatchingBone(targetArmatureParent, child.name);

                if (targetTrans == null)
                {
                    if (level == 0)
                    {
                        report.LogWarnLocalized(t, LogLabel, MessageCode.BonesNotMatchingInArmatureFirstLevel, child.name);
                    }
                    continue;
                }

                // Find whether there is a DynamicBone/PhysBone component controlling the bone
                var sourceDynamics = DynamicsUtils.FindDynamicsWithRoot(allDynamics, child);
                var targetDynamics = DynamicsUtils.FindDynamicsWithRoot(allDynamics, targetTrans);

                if (targetDynamics == null)
                {
                    // move to bone directly if avatar does not have dynamics controlling it
                    objectMappings.Add(new ObjectMapping()
                    {
                        Type = ObjectMapping.MappingType.MoveToBone,
                        SourceTransform = child,
                        TargetPath = AnimationUtils.GetRelativePath(targetTrans, avatarRoot),
                    });

                    ProcessBone(report, dynamicsOption, avatarRoot, child, targetTrans, allDynamics, objectMappings, tags, level + 1);
                    continue;
                }

                var option = dynamicsOption;
                if (option == DynamicsOptions.Auto)
                {
                    option = DetectDynamicsOption(report, avatarRoot, child, targetTrans);
                }
                HandleBoneDynamics(option, avatarRoot, child, targetTrans, sourceDynamics, targetDynamics, objectMappings, tags);
            }
        }

        public void Execute(Report report, GameObject avatarGameObject, IDresserSettings dresserSettings, out List<ObjectMapping> objectMappings, out List<ITag> tags)
        {
            objectMappings = null;
            tags = null;
            if (!(dresserSettings is StandardDresserSettings stdDs)) throw new ArgumentException("dresserSettings is not subclass of StandardDresserSettings");

            if (!CheckNoMissingScripts(report, MessageCode.MissingScriptsDetectedInAvatar, avatarGameObject))
            {
                return;
            }

            if (string.IsNullOrEmpty(stdDs.TargetArmaturePath))
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.MissingTargetArmaturePath);
                return;
            }

            if (stdDs.SourceArmature == null)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.MissingSourceArmature);
                return;
            }

            if (!DKEditorUtils.IsGrandParent(avatarGameObject.transform, stdDs.SourceArmature))
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.SourceArmatureNotInsideAvatar);
                return;
            }

            var targetArmature = avatarGameObject.transform.Find(stdDs.TargetArmaturePath);

            if (targetArmature == null)
            {
                // TODO: try obtain and guess
                report.LogErrorLocalized(t, LogLabel, MessageCode.CouldNotLocateTargetArmature, stdDs.TargetArmaturePath);
                return;
            }

            if (targetArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.NoBonesInTargetArmatureFirstLevel);
            }

            if (stdDs.SourceArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.NoBonesInSourceArmatureFirstLevel);
            }

            if (targetArmature.childCount == 0 || stdDs.SourceArmature.childCount == 0)
            {
                return;
            }

            objectMappings = new List<ObjectMapping>();
            tags = new List<ITag>();

            var allDynamics = DynamicsUtils.ScanDynamics(avatarGameObject);
            ProcessBone(report, stdDs.DynamicsOption, avatarGameObject.transform, stdDs.SourceArmature, targetArmature, allDynamics, objectMappings, tags, 0);
        }
    }
}
