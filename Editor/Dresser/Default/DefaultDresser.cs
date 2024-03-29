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
using static Chocopoi.DressingTools.Dresser.Default.DefaultDresserSettings;

namespace Chocopoi.DressingTools.Dresser.Default
{
    internal class DefaultDresser : IDresser
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        public const string LogLabel = "DefaultDresser";
        private const string BoneContainerSuffix = "_DT";

        public static class MessageCode
        {
            //Warnings
            public const string BonesNotMatchingInArmatureFirstLevel = "dressers.default.msgCode.warn.bonesNotMatchingInArmatureFirstLevel";

            //Errors
            public const string NoBonesInTargetArmatureFirstLevel = "dressers.default.msgCode.error.noBonesInTargetArmatureFirstLevel";
            public const string NoBonesInSourceArmatureFirstLevel = "dressers.default.msgCode.error.noBonesInSourceArmatureFirstLevel";
            public const string MissingScriptsDetectedInAvatar = "dressers.default.msgCode.error.missingScriptsDetectedInAvatar";
            public const string MissingTargetArmaturePath = "dressers.default.msgCode.error.missingTargetArmaturePath";
            public const string MissingSourceArmature = "dressers.default.msgCode.error.missingSourceArmature";
            public const string CouldNotLocateTargetArmature = "dressers.default.msgCode.error.couldNotLocateTargetArmature";
            public const string SourceArmatureNotInsideAvatar = "dressers.default.msgCode.error.sourceArmatureNotInsideAvatar";
        }

        public bool CheckNoMissingScripts(Report report, string errorCode, GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.LogErrorLocalized(t, LogLabel, errorCode, AnimationUtils.GetRelativePath(gameObject.transform));
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

        private static DynamicsOptions DetectDynamicsOption(Report report, Transform avatarRoot, Transform sourceTrans, Transform targetTrans)
        {
            // Some clothes creators copy body bones from other avatars for the breast bone parts.
            // Probably for saving some time fitting for multiple avatars.
            // Prone to position and rotation issues if we use ParentConstraint.
            // To workaround with this issue, we try to detect frankenstein bones and use IgnoreTransform
            // to let them move their own.

            var sourceNormalizedRotation = sourceTrans.rotation.normalized;
            var targetNormalizedRotation = targetTrans.rotation.normalized;

            // use world position and rotation to determine if they are frankenstein bones
            if (!Mathf.Approximately(sourceTrans.position.x, targetTrans.position.x) ||
                !Mathf.Approximately(sourceTrans.position.y, targetTrans.position.y) ||
                !Mathf.Approximately(sourceTrans.position.z, targetTrans.position.z) ||
                !Mathf.Approximately(sourceNormalizedRotation.x, targetNormalizedRotation.x) ||
                !Mathf.Approximately(sourceNormalizedRotation.y, targetNormalizedRotation.y) ||
                !Mathf.Approximately(sourceNormalizedRotation.z, targetNormalizedRotation.z) ||
                !Mathf.Approximately(sourceNormalizedRotation.w, targetNormalizedRotation.w)
            )
            {
                report.LogInfo(LogLabel, $"Using IgnoreTransform for frankenstein bone: {AnimationUtils.GetRelativePath(sourceTrans, avatarRoot)} -> {AnimationUtils.GetRelativePath(targetTrans, avatarRoot)}");
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
            if (!(dresserSettings is DefaultDresserSettings defaultDresserSettings)) throw new ArgumentException("dresserSettings is not subclass of DefaultDresserSettings");

            if (!CheckNoMissingScripts(report, MessageCode.MissingScriptsDetectedInAvatar, avatarGameObject))
            {
                return;
            }

            if (string.IsNullOrEmpty(defaultDresserSettings.TargetArmaturePath))
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.MissingTargetArmaturePath);
                return;
            }

            if (defaultDresserSettings.SourceArmature == null)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.MissingSourceArmature);
                return;
            }

            if (!DKEditorUtils.IsGrandParent(avatarGameObject.transform, defaultDresserSettings.SourceArmature))
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.SourceArmatureNotInsideAvatar);
                return;
            }

            var targetArmature = avatarGameObject.transform.Find(defaultDresserSettings.TargetArmaturePath);

            if (targetArmature == null)
            {
                // TODO: try obtain and guess
                report.LogErrorLocalized(t, LogLabel, MessageCode.CouldNotLocateTargetArmature, defaultDresserSettings.TargetArmaturePath);
                return;
            }

            if (targetArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.NoBonesInTargetArmatureFirstLevel);
            }

            if (defaultDresserSettings.SourceArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, LogLabel, MessageCode.NoBonesInSourceArmatureFirstLevel);
            }

            if (targetArmature.childCount == 0 || defaultDresserSettings.SourceArmature.childCount == 0)
            {
                return;
            }

            objectMappings = new List<ObjectMapping>();
            tags = new List<ITag>();

            var allDynamics = DynamicsUtils.ScanDynamics(avatarGameObject);
            ProcessBone(report, defaultDresserSettings.DynamicsOption, avatarGameObject.transform, defaultDresserSettings.SourceArmature, targetArmature, allDynamics, objectMappings, tags, 0);
        }
    }
}
